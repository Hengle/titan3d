﻿using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Common;
using System.Threading.Tasks;
using EngineNS.Graphics.Pipeline;
using EngineNS.GamePlay;
using NPOI.HSSF.Record.AutoFilter;

namespace EngineNS.Bricks.GpuDriven
{
    //nanite: https://zhuanlan.zhihu.com/p/382687738

    public class TtCullClusterShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(64, 1, 1);
        }
        public TtCullClusterShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/GpuDriven/ClusterCulling.cginc", RName.ERNameType.Engine);
            MainName = "CS_ClusterCullingMain";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtCullClusterNode;

            drawcall.BindSrv("ClusterBuffer", node.Clusters.DataSRV);
            drawcall.BindUav("VisClusterBuffer", node.VisClusters.DataUAV);
        }
    }
    public class TtCullClusterNode : URenderGraphNode
    {
        public URenderGraphPin HzbPinIn = URenderGraphPin.CreateInput("Hzb");
        public URenderGraphPin VerticesPinIn = URenderGraphPin.CreateInputOutput("Vertices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin IndicesPinIn = URenderGraphPin.CreateInputOutput("Indices", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin ClustersPinIn = URenderGraphPin.CreateInputOutput("Clusters", false, EPixelFormat.PXF_UNKNOWN);
        public URenderGraphPin VisibleClutersPinOut = URenderGraphPin.CreateOutput("VisibleClusters", false, EPixelFormat.PXF_UNKNOWN);
        
        public TtCullClusterShading CullClusterShading;
        private NxRHI.UComputeDraw CullClusterShadingDrawcall;

        public TtCpu2GpuBuffer<float> Vertices = new TtCpu2GpuBuffer<float>();
        public TtCpu2GpuBuffer<uint> Indices = new TtCpu2GpuBuffer<uint>();
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FClusterData
        {
            public Vector3 BoundCenter;
            public int VertStart;
            public Vector3 BoundExtent;
            public int VertEnd;
            public Matrix WVPMatrix;
        }
        public TtCpu2GpuBuffer<FClusterData> Clusters = new TtCpu2GpuBuffer<FClusterData>();
        public TtCpu2GpuBuffer<int> VisClusters = new TtCpu2GpuBuffer<int>();

        // TODO: can't repeat InitBuffer, world logic still have problems
        public bool bInitBuffer = false;

        public TtCullClusterNode()
        {
            Name = "CullClusterNode";
        }
        public override void InitNodePins()
        {
            AddInput(HzbPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(VerticesPinIn, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(IndicesPinIn, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(ClustersPinIn, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

            AddOutput(VisibleClutersPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

            HzbPinIn.IsAllowInputNull = true;
            VerticesPinIn.IsAllowInputNull = true;
            IndicesPinIn.IsAllowInputNull = true;
            ClustersPinIn.IsAllowInputNull = true;

            base.InitNodePins();
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            CoreSDK.DisposeObject(ref CullClusterShadingDrawcall);
            CullClusterShadingDrawcall = rc.CreateComputeDraw();
            CullClusterShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtCullClusterShading>();

            unsafe
            {
                VisClusters.Initialize(true);
                VisClusters.SetSize(100 + 1);
                var visClst = stackalloc int[100 + 1];
                visClst[0] = 0;
                VisClusters.UpdateData(0, visClst, sizeof(int) * 100 + sizeof(int));
                VisClusters.Flush2GPU();
            }
        }
        private unsafe void InitBuffers(Vector3[] vb, uint[] ib, List<FClusterData> clusters, EngineNS.Graphics.Pipeline.UCamera camera)
        {
            if (bInitBuffer)
                return;

            // TODO:
            bInitBuffer = true;

            Vertices.Initialize(false);
            Vertices.SetSize(sizeof(FQuarkVertex) * vb.Length / sizeof(float));
            fixed (Vector3* p = &vb[0])
            {
                Vertices.UpdateData(0, p, sizeof(FQuarkVertex) * vb.Length);
            }                
            Vertices.Flush2GPU();

            Indices.Initialize(false);
            Indices.SetSize(ib.Length);
            fixed (uint* p = &ib[0])
            {
                Indices.UpdateData(0, p, sizeof(uint) * ib.Length);
            }
            Indices.Flush2GPU();

            Clusters.Initialize(false);
            Clusters.SetSize(sizeof(FClusterData) * clusters.Count);
            var clst = stackalloc FClusterData[clusters.Count];

            Vector3 cameraPos = camera.GetLocalPosition();
            Matrix worldMatrix = Matrix.Identity;
            worldMatrix.M41 = cameraPos.X;
            worldMatrix.M42 = cameraPos.Y;
            worldMatrix.M43 = cameraPos.Z;

            for (int i = 0; i < clusters.Count; i++)
            {
                clst[i].WVPMatrix = worldMatrix * camera.GetViewProjection();
                clst[i].VertStart = clusters[i].VertStart;
                clst[i].VertEnd = clusters[i].VertEnd;
                // TODO:
                clst[i].BoundCenter = Vector3.Zero;
                clst[i].BoundExtent = Vector3.One;
            }
            
            Clusters.UpdateData(0, clst, sizeof(FClusterData) * clusters.Count);
            Clusters.Flush2GPU();

            var attachment = ImportAttachment(VisibleClutersPinOut);
            attachment.Uav = VisClusters.DataUAV;
            attachment.Srv = VisClusters.DataSRV;

            if (VerticesPinIn.FindInLinker() == null)
            {
                attachment = ImportAttachment(VerticesPinIn);
                attachment.Srv = Vertices.DataSRV;
            }
            if (IndicesPinIn.FindInLinker() == null)
            {
                attachment = ImportAttachment(IndicesPinIn);
                attachment.Srv = Indices.DataSRV;
            }
            if (ClustersPinIn.FindInLinker() == null)
            {
                attachment = ImportAttachment(ClustersPinIn);
                attachment.Srv = Clusters.DataSRV;
            }
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            base.BeforeTickLogic(policy);            
        }
        public unsafe override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            BuildInstances(world, policy.DefaultCamera.VisParameter);

            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();
            
            NxRHI.FBufferWriter bfWriter = new NxRHI.FBufferWriter();
            bfWriter.Buffer = VisClusters.GpuBuffer.mCoreObject;
            bfWriter.Offset = 0;
            bfWriter.Value = 0;
            cmd.WriteBufferUINT32(1, &bfWriter);

            // TODO: dispatch x/y/z
            CullClusterShading.SetDrawcallDispatch(this, policy, CullClusterShadingDrawcall, 1, 1, 1, true);
            cmd.PushGpuDraw(CullClusterShadingDrawcall);

            cmd.BeginEvent(Name);
            cmd.FlushDraws();
            cmd.EndEvent();

            cmd.EndCommand();
            
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }

        public void BuildInstances(GamePlay.UWorld world, GamePlay.UWorld.UVisParameter rp)
        {
            List<FClusterData> clusters = new List<FClusterData>();
            List<Vector3> position = new List<Vector3>();
            List<uint> ib = new List<uint>();

            foreach (var i in rp.VisibleNodes)
            {
                var meshNode = i as GamePlay.Scene.UMeshNode;
                if (meshNode == null)
                    continue;

                var clusterMesh = meshNode.Mesh.MaterialMesh.Mesh.ClusteredMesh;
                if (clusterMesh == null)
                    continue;

                for (int clusterId = 0; clusterId < clusterMesh.Clusters.Count; clusterId++)
                {
                    var cluster = new FClusterData();
                    cluster.VertStart = clusterMesh.Clusters[clusterId].VertexStart;
                    cluster.VertEnd = clusterMesh.Clusters[clusterId].VertexCount;
                    clusters.Add(cluster);                    
                }

                position.AddRange(new List<Vector3>(clusterMesh.Vertices));
                ib.AddRange(new List<uint>(clusterMesh.Indices));
            }

            InitBuffers(position.ToArray(), ib.ToArray(), clusters, rp.CullCamera);
        }
    }
}


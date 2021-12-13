﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UHzbNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public readonly UInt32_3 Dispatch_SetupDimArray2 = new UInt32_3(32, 32, 1);

        public RHI.CTexture2D HzbTexture;
        public RHI.CShaderResourceView HzbSRV;
        public RHI.CUnorderedAccessView[] HzbMipsUAVs;

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();

        private RHI.CShaderDesc CSDesc_Setup;
        private RHI.CComputeShader CS_Setup;
        private RHI.CComputeDrawcall SetupDrawcall;

        private RHI.CShaderDesc CSDesc_DownSample;
        private RHI.CComputeShader CS_DownSample;
        private RHI.CComputeDrawcall[] MipsDrawcalls;
        ~UHzbNode()
        {
            Cleanup();
        }
        public async override System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName);

            var defines = new RHI.CShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.x}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.z}");

            CSDesc_Setup = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine),
                "CS_Setup", EShaderType.EST_ComputeShader, defines, null);
            CS_Setup = rc.CreateComputeShader(CSDesc_Setup);

            CSDesc_DownSample = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine),
                "CS_DownSample", EShaderType.EST_ComputeShader, defines, null);
            CS_DownSample = rc.CreateComputeShader(CSDesc_DownSample);

            SetupDrawcall = rc.CreateComputeDrawcall();
            //ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(IRenderPolicy policy)
        {
            if (CS_Setup == null)
                return;
            SetupDrawcall.mCoreObject.SetComputeShader(CS_Setup.mCoreObject);
            SetupDrawcall.mCoreObject.SetDispatch(MaxSRVWidth / Dispatch_SetupDimArray2.x, MaxSRVHeight / Dispatch_SetupDimArray2.y, 1);

            var srvIdx = CSDesc_Setup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
            if (srvIdx != (IShaderBinder*)0)
            {
                var depth = policy.GetBasePassNode().GBuffers.GetDepthStencilSRV();
                SetupDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, depth.mCoreObject);
            }
            srvIdx = CSDesc_Setup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, HzbMipsUAVs[0].mCoreObject);
            }

            if (HzbMipsUAVs == null)
                return;

            if (MipsDrawcalls != null)
            {
                foreach(var i in MipsDrawcalls)
                {
                    i.Dispose();
                }
            }
            MipsDrawcalls = new RHI.CComputeDrawcall[HzbMipsUAVs.Length - 1];
            uint width = MaxSRVWidth / 2;
            uint height = MaxSRVHeight / 2;
            for (int i = 1; i < HzbMipsUAVs.Length; i++)
            {
                if (width > 1)
                {
                    width = width / 2;
                }
                else
                {
                    width = 1;
                }
                if (height > 1)
                {
                    height = height / 2;
                }
                else
                {
                    height = 1;
                }
                var drawcall = UEngine.Instance.GfxDevice.RenderContext.CreateComputeDrawcall();
                MipsDrawcalls[i - 1] = drawcall;
                drawcall.mCoreObject.SetComputeShader(CS_DownSample.mCoreObject);
                drawcall.mCoreObject.SetDispatch(CoreDefine.Roundup(width, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(height, Dispatch_SetupDimArray2.y), 1);
                srvIdx = CSDesc_DownSample.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "SrcBuffer");
                if (srvIdx != (IShaderBinder*)0)
                {
                    drawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, HzbMipsUAVs[i - 1].mCoreObject);
                }

                srvIdx = CSDesc_DownSample.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
                if (srvIdx != (IShaderBinder*)0)
                {
                    drawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, HzbMipsUAVs[i].mCoreObject);
                }
            }
        }
        public void Cleanup()
        {
            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                {
                    HzbMipsUAVs[i]?.Dispose();
                }
                HzbMipsUAVs = null;
            }
            HzbTexture?.Dispose();
            HzbTexture = null;

            HzbSRV?.Dispose();
            HzbSRV = null;
        }
        uint MaxSRVWidth;
        uint MaxSRVHeight;
        public override unsafe void OnResize(IRenderPolicy policy, float x, float y)
        {
            if (x == 1 || y == 1)
                return;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            x /= 2;
            y /= 2;
            MaxSRVWidth = (uint)x;
            MaxSRVHeight = (uint)y;

            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                {
                    HzbMipsUAVs[i]?.Dispose();
                }
            }

            HzbMipsUAVs = new RHI.CUnorderedAccessView[RHI.CShaderResourceView.CalcMipLevel((int)x, (int)y, true)];
            var dsTexDesc = new ITexture2DDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = (uint)x;
            dsTexDesc.Height = (uint)y;
            dsTexDesc.MipLevels = (uint)HzbMipsUAVs.Length;
            dsTexDesc.Format = EPixelFormat.PXF_R16G16_TYPELESS;
            dsTexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_UNORDERED_ACCESS);
            HzbTexture?.Dispose();
            HzbTexture = rc.CreateTexture2D(in dsTexDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = ESrvType.ST_Texture2D;
            srvDesc.Format = EPixelFormat.PXF_R16G16_UNORM;
            srvDesc.mGpuBuffer = HzbTexture.mCoreObject.NativeSuper;
            srvDesc.Texture2D.MipLevels = dsTexDesc.MipLevels;
            HzbSRV?.Dispose();
            HzbSRV = rc.CreateShaderResourceView(in srvDesc);

            for (int i = 0; i < HzbMipsUAVs.Length; i++)
            {
                var uavDesc = new IUnorderedAccessViewDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = EPixelFormat.PXF_R16G16_UNORM;
                uavDesc.Texture2D.MipSlice = (uint)i;
                HzbMipsUAVs[i] = rc.CreateUnorderedAccessView(HzbTexture.mCoreObject.NativeSuper, in uavDesc);
            }

            ResetComputeDrawcall(policy);
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.IRenderPolicy policy, bool bClear)
        {
            if (CS_Setup == null)
                return;
            var cmd = BasePass.DrawCmdList.mCoreObject;

            SetupDrawcall.mCoreObject.BuildPass(cmd);

            if (MipsDrawcalls != null)
            {
                for (int i = 0; i < MipsDrawcalls.Length; i++)
                {
                    MipsDrawcalls[i].mCoreObject.BuildPass(cmd);
                }
            }
            //cmd.SetComputeShader(CS_Setup.mCoreObject);
            //var srvIdx  = CSDesc_Setup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    var depth = policy.GetBasePassNode().GBuffers.DepthStencilSRV;
            //    cmd.CSSetShaderResource(srvIdx->CSBindPoint, depth.mCoreObject);
            //}

            //srvIdx = CSDesc_Setup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "DstBuffer");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, HzbMipsUAVs[0].mCoreObject, &nUavInitialCounts);
            //}

            //cmd.CSDispatch(MaxSRVWidth / Dispatch_SetupDimArray2.x, MaxSRVHeight / Dispatch_SetupDimArray2.y, 1);

            //uint width = MaxSRVWidth/2;
            //uint height = MaxSRVHeight/2;
            //for (int i = 1; i < HzbMipsUAVs.Length; i++)
            //{
            //    cmd.SetComputeShader(CS_DownSample.mCoreObject);
            //    var srvIdx = CSDesc_DownSample.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "SrcBuffer");
            //    if (srvIdx != (IShaderBinder*)0)
            //    {
            //        cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, HzbMipsUAVs[i - 1].mCoreObject, &nUavInitialCounts);
            //    }

            //    srvIdx = CSDesc_DownSample.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
            //    if (srvIdx != (IShaderBinder*)0)
            //    {
            //        cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, HzbMipsUAVs[i].mCoreObject, &nUavInitialCounts);
            //    }
                
            //    cmd.CSDispatch(CoreDefine.Roundup(width, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(height, Dispatch_SetupDimArray2.y), 1);

            //    if (width > 1)
            //    {
            //        width = width / 2;
            //    }
            //    else
            //    {
            //        width = 1;
            //    }
            //    if (height > 1)
            //    {
            //        height = height / 2;
            //    }
            //    else
            //    {
            //        height = 1;
            //    }
            //}

            if (cmd.BeginCommand())
            {
                cmd.EndCommand();
            }
        }
        public unsafe override void TickRender(Graphics.Pipeline.IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(Graphics.Pipeline.IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}

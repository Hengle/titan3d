﻿using EngineNS.GamePlay;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UPickHollowShading : Shader.UGraphicsShadingEnv
    {
        public UPickHollowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_hollow.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, atom);

            var pickHollowNode = drawcall.TagObject as Common.UPickHollowNode;
            
            var index = drawcall.FindBinder("gPickedSetUpTex");
            if (index.IsValidPointer)
            {
                var attachBuffer = pickHollowNode.GetAttachBuffer(pickHollowNode.PickedPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_gPickedSetUpTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("gPickedBlurTex");
            if (index.IsValidPointer)
            {
                var attachBuffer = pickHollowNode.GetAttachBuffer(pickHollowNode.BlurPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_gPickedBlurTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }
    public class UPickHollowNode : USceenSpaceNode
    {
        public TtRenderGraphPin PickedPinIn = TtRenderGraphPin.CreateInput("Picked");
        public TtRenderGraphPin BlurPinIn = TtRenderGraphPin.CreateInput("Blur");
        public UPickHollowNode()
        {
            Name = "PickHollowNode";
        }
        public override void InitNodePins()
        {
            AddInput(PickedPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(BlurPinIn, NxRHI.EBufferType.BFT_SRV);
            
            ResultPinOut.IsAutoResize = false;
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16_FLOAT;
            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickHollowShading>();
        }
        public override void OnLinkIn(TtRenderGraphLinker linker)
        {
            //ResultPinOut.Attachement.Format = PickedPinIn.Attachement.Format;
        }
        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            float scaleFactor = 1.0f;
            var hitProxyNode = policy.FindFirstNode<UHitproxyNode>();
            if (hitProxyNode != null)
            {
                scaleFactor = hitProxyNode.ScaleFactor;
            }
            ResultPinOut.Attachement.Width = (uint)(x * scaleFactor);
            ResultPinOut.Attachement.Height = (uint)(y * scaleFactor);

            base.OnResize(policy, x * scaleFactor, y * scaleFactor);
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            var PickedManager = policy.GetOptionData("PickedManager") as UPickedProxiableManager;
            if (PickedManager != null && PickedManager.PickedProxies.Count == 0)
                return;
            base.TickLogic(world, policy, bClear);
        }
    }

    public class TtPickHollowBlendShading : Shader.UGraphicsShadingEnv
    {
        public TtPickHollowBlendShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_hollow_blend.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, atom);

            var node = drawcall.TagObject as TtPickHollowBlendNode;

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.DepthPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GPickedTex");
            if (index.IsValidPointer)
            {
                var attachBuffer = node.GetAttachBuffer(node.PickedPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GPickedTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
        }
    }

    public class TtPickHollowBlendNode : USceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInputOutput("Depth");
        public TtRenderGraphPin PickedPinIn = TtRenderGraphPin.CreateInput("Picked");
        public TtPickHollowBlendNode()
        {
            Name = "PickHollowBlendNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PickedPinIn, NxRHI.EBufferType.BFT_SRV);            

            base.InitNodePins();
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtPickHollowBlendShading>();
        }
        public override void OnLinkIn(TtRenderGraphLinker linker)
        {
            //ResultPinOut.Attachement.Format = PickedPinIn.Attachement.Format;
        }
        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {

        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var PickedManager = policy.GetOptionData("PickedManager") as UPickedProxiableManager;
            if (PickedManager != null && PickedManager.PickedProxies.Count == 0)
            {
                this.MoveAttachment(ColorPinIn, ResultPinOut);
                return;
            }

            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                if (ResultPinOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            var PickedManager = policy.GetOptionData("PickedManager") as UPickedProxiableManager;
            if (PickedManager != null && PickedManager.PickedProxies.Count == 0)
                return;
            base.TickLogic(world, policy, bClear);
        }
    }
}

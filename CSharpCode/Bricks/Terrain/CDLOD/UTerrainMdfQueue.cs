﻿using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UTerrainMdfQueue : Graphics.Pipeline.Shader.UMdfQueue
    {
        public UPatch Patch
        {
            get
            {
                return this.MdfDatas as UPatch;
            }
        }
        public int Dimension = 64;
        public UTerrainMdfQueue()
        {
            UpdateShaderCode();
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position, 
                EVertexSteamType.VST_Normal,
                EVertexSteamType.VST_UV,};
        }
        public override void CopyFrom(UMdfQueue mdf)
        {
            Dimension = (mdf as UTerrainMdfQueue).Dimension;
        }
        public override Hash160 GetHash()
        {
            string CodeString = IO.FileManager.ReadAllText(RName.GetRName("shaders/Modifier/TerrainCDLOD.cginc", RName.ERNameType.Engine).Address);
            mMdfQueueHash = Hash160.CreateHash160(CodeString);
            return mMdfQueueHash;
        }
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine("#ifndef _UTerrainMdfQueue_CDLOD_INC_");
            codeBuilder.AddLine("#define _UTerrainMdfQueue_CDLOD_INC_");
            codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/Modifier/TerrainCDLOD.cginc", RName.ERNameType.Engine).Address}\"");

            codeBuilder.AddLine("#endif");
            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
        public unsafe override void OnDrawCall(Graphics.Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Graphics.Pipeline.IRenderPolicy policy, Graphics.Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);
            if (drawcall.TagObject == null)
            {
                //这里可以缓存Effec里面各种VarIndex，不过这里只有两个需要每次都查找，而地形对象又不会太多，那就别缓存了
            }
            var pat = Patch;
            
            SureCBuffer(drawcall.Effect);

            var shaderProg = drawcall.Effect.ShaderProgram;
            var reflector = shaderProg.mCoreObject.GetReflector();
            var index = reflector.GetShaderBinder(EShaderBindType.SBT_Srv, "HeightMapTexture");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, pat.Level.HeightMapSRV.mCoreObject);
            index = reflector.GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_HeightMapTexture");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = reflector.GetShaderBinder(EShaderBindType.SBT_Srv, "HeightMapTextureArray");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, pat.Level.GetTerrainNode().RVTextureArray.TexArraySRV.mCoreObject);

            index = reflector.GetShaderBinder(EShaderBindType.SBT_Srv, "ArrayTextures[1]");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, pat.Level.HeightMapSRV.mCoreObject);

            index = reflector.GetShaderBinder(EShaderBindType.SBT_Srv, "NormalMapTexture");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSrv(index, pat.Level.NormalMapSRV.mCoreObject);
            index = reflector.GetShaderBinder(EShaderBindType.SBT_Sampler, "Samp_NormalMapTexture");
            if (!CoreSDK.IsNullPointer(index))
                drawcall.mCoreObject.BindShaderSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            var cbIndex = reflector.GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerPatch");
            if (!CoreSDK.IsNullPointer(index))
            {
                var varIndex = pat.PatchCBuffer.mCoreObject.FindVar("StartPosition");
                pat.PatchCBuffer.SetValue(varIndex, in pat.StartPosition);

                varIndex = pat.PatchCBuffer.mCoreObject.FindVar("CurrentLOD");
                pat.PatchCBuffer.SetValue(varIndex, pat.CurrentLOD);

                var terrain = pat.Level.GetTerrainNode();
                varIndex = pat.PatchCBuffer.mCoreObject.FindVar("EyeCenter");
                pat.PatchCBuffer.SetValue(varIndex, terrain.EyeLocalCenter - pat.StartPosition);

                pat.TexUVOffset.X = (Patch.XInLevel * 64.0f) / 1024.0f;
                pat.TexUVOffset.Y = (Patch.ZInLevel * 64.0f) / 1024.0f;
                varIndex = pat.PatchCBuffer.mCoreObject.FindVar("TexUVOffset");
                pat.PatchCBuffer.SetValue(varIndex, in pat.TexUVOffset);

                drawcall.mCoreObject.BindShaderCBuffer(cbIndex, pat.PatchCBuffer.mCoreObject);
            }
            cbIndex = reflector.GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerTerrain");
            if (!CoreSDK.IsNullPointer(index))
            {
                drawcall.mCoreObject.BindShaderCBuffer(cbIndex, pat.Level.Level.Node.TerrainCBuffer.mCoreObject);
            }
        }
        private void SureCBuffer(UEffect effect)
        {
            var shaderProg = effect.ShaderProgram;
            var pat = Patch;
            if (pat.PatchCBuffer == null)
            {
                var cbIndex = shaderProg.mCoreObject.GetReflector().FindShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerPatch");
                pat.PatchCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(shaderProg, cbIndex);
            }
            if (pat.Level.Level.Node.TerrainCBuffer == null)
            {
                var cbIndex = shaderProg.mCoreObject.GetReflector().FindShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerTerrain");
                pat.Level.Level.Node.TerrainCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(shaderProg, cbIndex);
            }
        }
        public override Rtti.UTypeDesc GetPermutation(List<string> features)
        {
            if (features.Contains("UMdf_NoShadow"))
            {
                return Rtti.UTypeDescGetter<UTerrainMdfQueuePermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc;
            }
            else
            {
                return Rtti.UTypeDescGetter<UTerrainMdfQueuePermutation<Graphics.Pipeline.Shader.UMdf_Shadow>>.TypeDesc;
            }
        }
    }

    public class UTerrainMdfQueuePermutation<PermutationType> : UTerrainMdfQueue
    {
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine("#ifndef _UTerrainMdfQueue_CDLOD_INC_");
            codeBuilder.AddLine("#define _UTerrainMdfQueue_CDLOD_INC_");
            codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/Modifier/TerrainCDLOD.cginc", RName.ERNameType.Engine).Address}\"");

            codeBuilder.AddLine("#endif");

            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1");
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {
                
            }

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
    }
}

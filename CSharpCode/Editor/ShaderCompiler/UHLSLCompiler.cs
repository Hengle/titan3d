﻿using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace EngineNS.Editor.ShaderCompiler
{
    public class UHLSLInclude
    {
        public unsafe virtual NxRHI.FShaderCode* GetHLSLCode(string includeName, out bool bIncluded)
        {
            bIncluded = false;
            return (NxRHI.FShaderCode*)0;
        }
    }
    public unsafe class UHLSLCompiler
    {
        NxRHI.UShaderCompiler mShaderCompiler = null;
        static CoreSDK.FDelegate_FOnShaderTranslated OnShaderTranslated = OnShaderTranslatedImpl;
        static void OnShaderTranslatedImpl(NxRHI.FShaderDesc arg0)
        {
            var glsl = arg0.GetGLCode();
            if (glsl.Length > 0)
            {
                bool changed = false;
                if (glsl.Contains("#error No extension available for FP16."))
                {
                    glsl = glsl.Replace("#error No extension available for Int16.", "#define float16_t float");
                    changed = true;
                }
                if (glsl.Contains("#error No extension available for Int16."))
                {
                    glsl = glsl.Replace("#error No extension available for Int16.", "#define uint16_t uint");
                    changed = true;
                }
                if (changed)
                {
                    arg0.SetGLCode(glsl);
                }
            }
        }
        static UHLSLCompiler()
        {
            CoreSDK.SetOnShaderTranslated(OnShaderTranslated);
        }
        public UHLSLCompiler()
        {
            GetShaderCodeStream = this.GetHLSLCode;
            mShaderCompiler = new NxRHI.UShaderCompiler(GetShaderCodeStream);
        }
        private static void FixRootPath(ref string file)
        {
            var repPos = file.IndexOf("@Engine/");
            if (repPos >= 0)
            {
                file = file.Substring(repPos);
                file = file.Replace("@Engine/", UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine));
            }
            else
            {
                repPos = file.IndexOf("@Game/");
                if (repPos >= 0)
                {
                    file = file.Substring(repPos);
                    file = file.Replace("@Game/", UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game));
                }
            }
        }
        public static NxRHI.FShaderCode GetIncludeCode(string file)
        {
            FixRootPath(ref file);
            RName rn = null;
            if (file.StartsWith(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine)))
            {
                var path = IO.TtFileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine), file);
                rn = RName.GetRName(path, RName.ERNameType.Engine);
            }
            else if (file.StartsWith(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game)))
            {
                var path = IO.TtFileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game), file);
                rn = RName.GetRName(path, RName.ERNameType.Game);
            }
            else
            {
                rn = RName.GetRName(file, RName.ERNameType.Engine);
            }
            if (rn != null)
            {
                var code = UShaderCodeManager.Instance.GetShaderCodeProvider(rn);
                if (code != null)
                {
                    return code.SourceCode.mCoreObject;
                }
            }
            return new NxRHI.FShaderCode();
        }
        private NxRHI.FShaderCompiler.FDelegate_FnGetShaderCodeStream GetShaderCodeStream;
        public string MaterialCodeForDebug;
        //[UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
        private NxRHI.FShaderCode* GetHLSLCode(sbyte* includeName)
        {
            var file = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)includeName);
            FixRootPath(ref file);

            if (UserInclude != null)
            {
                bool bIncluded;
                var result = UserInclude.GetHLSLCode(file, out bIncluded);
                if (bIncluded)
                    return result;
            }
            bool isVar = false;
            RName rn = null;
            if (file.EndsWith("/Material"))
            {
                if (Material == null)
                    return (NxRHI.FShaderCode*)0;
                MaterialCodeForDebug = Material.SourceCode.TextCode;
                return Material.SourceCode.mCoreObject;
            }
            else if (file.EndsWith("/MaterialVar"))
            {
                if (Material == null)
                    return (NxRHI.FShaderCode*)0;
                return Material.DefineCode.mCoreObject;
            }
            else if (file.EndsWith("/MdfQueue"))
            {
                var mdf = Rtti.UTypeDescManager.CreateInstance(MdfQueueType) as Graphics.Pipeline.Shader.UMdfQueue;
                if (mdf != null)
                {
                    return mdf.SourceCode.mCoreObject;
                }
                else
                    return (NxRHI.FShaderCode*)0;
            }
            else if (file.StartsWith(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine)))
            {
                var path = IO.TtFileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Engine), file);
                rn = RName.GetRName(path, RName.ERNameType.Engine);
            }
            else if (file.StartsWith(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game)))
            {
                var path = IO.TtFileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game), file);
                rn = RName.GetRName(path, RName.ERNameType.Game);
            }
            else
            {
                rn = RName.GetRName(file, RName.ERNameType.Engine);
            }
            if (rn != null)
            {
                var code = UShaderCodeManager.Instance.GetShaderCodeProvider(rn);
                if (code != null)
                {
                    if (isVar)
                        return code.DefineCode.mCoreObject;
                    else
                        return code.SourceCode.mCoreObject;
                }
            }
            return (NxRHI.FShaderCode*)0;
        }
        private UHLSLInclude UserInclude;
        private Graphics.Pipeline.Shader.UMaterial Material;
        private Rtti.UTypeDesc MdfQueueType;
        private string GetVertexStreamDefine(NxRHI.EVertexStreamType type)
        {
            switch (type)
            {
                case NxRHI.EVertexStreamType.VST_Position:
                    return "USE_VS_Position";
                case NxRHI.EVertexStreamType.VST_Normal:
                    return "USE_VS_Normal";
                case NxRHI.EVertexStreamType.VST_Tangent:
                    return "USE_VS_Tangent";
                case NxRHI.EVertexStreamType.VST_Color:
                    return "USE_VS_Color";
                case NxRHI.EVertexStreamType.VST_UV:
                    return "USE_VS_UV";
                case NxRHI.EVertexStreamType.VST_LightMap:
                    return "USE_VS_LightMap";
                case NxRHI.EVertexStreamType.VST_SkinIndex:
                    return "USE_VS_SkinIndex";
                case NxRHI.EVertexStreamType.VST_SkinWeight:
                    return "USE_VS_SkinWeight";
                case NxRHI.EVertexStreamType.VST_TerrainIndex:
                    return "USE_VS_TerrainIndex";
                case NxRHI.EVertexStreamType.VST_TerrainGradient:
                    return "USE_VS_TerrainGradient";
                case NxRHI.EVertexStreamType.VST_InstPos:
                    return "USE_VS_InstPos";
                case NxRHI.EVertexStreamType.VST_InstQuat:
                    return "USE_VS_InstQuat";
                case NxRHI.EVertexStreamType.VST_InstScale:
                    return "USE_VS_InstScale";
                case NxRHI.EVertexStreamType.VST_F4_1:
                    return "USE_VS_F4_1";
                case NxRHI.EVertexStreamType.VST_F4_2:
                    return "USE_VS_F4_2";
                case NxRHI.EVertexStreamType.VST_F4_3:
                    return "USE_VS_F4_3";
            }
            return null;
        }
        private string GetPixelInputDefine(Graphics.Pipeline.Shader.EPixelShaderInput type)
        {
            switch(type)
            {
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Position:
                    return "USE_PS_Position";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal:
                    return "USE_PS_Normal";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Color:
                    return "USE_PS_Color";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_UV:
                    return "USE_PS_UV";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_WorldPos:
                    return "USE_PS_WorldPos";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent:
                    return "USE_PS_Tangent";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_LightMap:
                    return "USE_PS_LightMap";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom0:
                    return "USE_PS_Custom0";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom1:
                    return "USE_PS_Custom1";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom2:
                    return "USE_PS_Custom2";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom3:
                    return "USE_PS_Custom3";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_Custom4:
                    return "USE_PS_Custom4";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_PointLightIndices:
                    return "USE_PS_PointLightIndices";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_1:
                    return "USE_PS_F4_1";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_2:
                    return "USE_PS_F4_2";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_F4_3:
                    return "USE_PS_F4_3";
                case Graphics.Pipeline.Shader.EPixelShaderInput.PST_SpecialData:
                    return "USE_PS_SpecialData";
            }
            return null;
        }
        public unsafe NxRHI.UShaderDesc CompileShader(string shader, string entry, NxRHI.EShaderType type,
            Graphics.Pipeline.Shader.UShadingEnv shadingEnvshadingEnv, Graphics.Pipeline.Shader.UMaterial mtl, Type mdfType,
            NxRHI.UShaderDefinitions defines, UHLSLInclude incProvider, string sm = null, bool bDebugShader = true, string extHlslVersion = null)
        {
            var code_text = IO.TtFileManager.ReadAllText(shader);
            var metaIndex = code_text.IndexOf($"/**Meta Begin:({entry})");
            if (metaIndex >= 0)
            {
                code_text = code_text.Substring(metaIndex + $"/**Meta Begin:({entry})".Length);
                metaIndex = code_text.IndexOf($"Meta End:({entry})**/");
                if (metaIndex >= 0)
                {
                    code_text = code_text.Substring(0, metaIndex);
                    var lines = code_text.Split("\r\n");
                    foreach(var i in lines)
                    {
                        if (string.IsNullOrEmpty(i))
                            continue;
                        var pairs = i.Split('=');
                        if (pairs.Length == 2)
                        {
                            switch (pairs[0])
                            {
                                case "SM":
                                    sm = pairs[1];
                                    break;
                                case "HLSL":
                                    extHlslVersion = pairs[1];
                                    if (extHlslVersion == "none")
                                        extHlslVersion = null;
                                    break;
                            }
                        }
                    }
                }
            }
            var desc = new NxRHI.UShaderDesc(type);
            var mtlName = "";
            if (mtl != null)
                mtlName = mtl.ToString();
            var mdfName = "";
            if (mdfType != null)
                mdfName = mdfType.FullName;
            desc.DebugName = $"{shader}:{entry}[id,{shadingEnvshadingEnv?.CurrentPermutationId}][mtl,{mtlName}][mdf,{mdfName}]";
            if (shadingEnvshadingEnv != null)
                desc.PermutationId = shadingEnvshadingEnv.CurrentPermutationId;
            UserInclude = incProvider;
            Material = mtl;
            MdfQueueType = Rtti.UTypeDesc.TypeOf(mdfType);
            //IShaderDefinitions defPtr = new IShaderDefinitions((void*)0);
            using (var defPtr = new NxRHI.UShaderDefinitions())
            {
                if (defines != null)
                {
                    defPtr.MergeDefinitions(defines);
                }
                if (mtl != null && mtl.Defines != null)
                {
                    defPtr.MergeDefinitions(mtl.Defines);
                    var psInputs = mtl.GetPSNeedInputs();
                    if (psInputs != null)
                    {
                        foreach (var i in psInputs)
                        {
                            defPtr.AddDefine(GetPixelInputDefine(i), "1");
                        }
                    }
                }
                var graphicsEnv = shadingEnvshadingEnv as Graphics.Pipeline.Shader.UGraphicsShadingEnv;
                if (graphicsEnv != null)
                {
                    {
                        var shadingNeeds = graphicsEnv.GetNeedStreams();
                        if (shadingNeeds != null)
                        {
                            foreach (var i in shadingNeeds)
                            {
                                defPtr.AddDefine(GetVertexStreamDefine(i), "1");
                            }
                        }
                        var shadingPSNeeds = graphicsEnv.GetPSNeedInputs();
                        if (shadingPSNeeds != null)
                        {
                            foreach (var i in shadingPSNeeds)
                            {
                                defPtr.AddDefine(GetPixelInputDefine(i), "1");
                            }
                        }
                    }
                    {
                        var mdfObj = Rtti.UTypeDescManager.CreateInstance(MdfQueueType) as Graphics.Pipeline.Shader.UMdfQueue;
                        if (mdfObj != null)
                        {
                            var mdfNeeds = mdfObj.GetNeedStreams();
                            if (mdfNeeds != null)
                            {
                                foreach (var i in mdfNeeds)
                                {
                                    defPtr.AddDefine(GetVertexStreamDefine(i), "1");
                                }
                            }
                            var mdfPSNeeds = mdfObj.GetPSNeedInputs();
                            if (mdfPSNeeds != null)
                            {
                                foreach (var i in mdfPSNeeds)
                                {
                                    defPtr.AddDefine(GetPixelInputDefine(i), "1");
                                }
                            }
                        }
                    }
                }
                else
                {
                    var computeEnv = shadingEnvshadingEnv as Graphics.Pipeline.Shader.UComputeShadingEnv;
                    if (computeEnv != null)
                    {
                        defines.mCoreObject.AddDefine("DispatchX", $"{computeEnv.DispatchArg.X}");
                        defines.mCoreObject.AddDefine("DispatchY", $"{computeEnv.DispatchArg.Y}");
                        defines.mCoreObject.AddDefine("DispatchZ", $"{computeEnv.DispatchArg.Z}");
                    }
                }
                switch (type)
                {
                    case NxRHI.EShaderType.SDT_VertexShader:
                        defPtr.AddDefine("ShaderStage", "0");//VSStage
                        break;
                    case NxRHI.EShaderType.SDT_PixelShader:
                        defPtr.AddDefine("ShaderStage", "1");//PSStage
                        break;
                    case NxRHI.EShaderType.SDT_ComputeShader:
                        defPtr.AddDefine("ShaderStage", "0");//CSStage
                        break;
                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }

                UEngine.Instance.GfxDevice.RenderContext.DeviceCaps.SetShaderGlobalEnv(defPtr.mCoreObject);

                var cfg = UEngine.Instance.Config;
                if (cfg.CookDXBC)
                {
                    defPtr.AddDefine("RHI_TYPE", "RHI_DX11");
                    var compile_sm = sm;
                    if (sm == null)
                    {
                        compile_sm = "5_0";
                    }
                    var ok = mShaderCompiler.CompileShader(desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_DXBC, bDebugShader, extHlslVersion, null);
                    if (ok == false)
                        return null;
                }
                if (cfg.CookDXIL)
                {
                    defPtr.AddDefine("RHI_TYPE", "RHI_DX12");
                    if (extHlslVersion != null)
                    {
                        defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    }
                    var compile_sm = sm;
                    if (sm == null)
                    {
                        compile_sm = "6_5";
                    }
                    var ok = mShaderCompiler.CompileShader(desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_DXIL, bDebugShader, extHlslVersion, null);
                    if (ok == false)
                        return null;
                }
                if (cfg.CookGLSL)
                {
                    defPtr.AddDefine("RHI_TYPE", "RHI_GL");
                    if (extHlslVersion != null)
                    {
                        defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    }
                    var compile_sm = sm;
                    if (sm == null)
                    {
                        compile_sm = "6_5";
                    }
                    var ok = mShaderCompiler.CompileShader(desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_DXBC, bDebugShader, extHlslVersion, null);
                    if (ok == false)
                        return null;
                }
                if (cfg.CookMETAL)
                {
                    defPtr.AddDefine("RHI_TYPE", "RHI_MTL");
                    if (extHlslVersion != null)
                    {
                        defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    }
                    var compile_sm = sm;
                    if (sm == null)
                    {
                        compile_sm = "6_5";
                    }
                    var ok = mShaderCompiler.CompileShader(desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_DXBC, bDebugShader, 
                        extHlslVersion, null);
                    if (ok == false)
                        return null;
                }
                if (cfg.CookSPIRV)
                {
                    //extHlslVersion = "2021";
                    defPtr.AddDefine("RHI_TYPE", "RHI_VK");
                    if (extHlslVersion != null)
                    {
                        defPtr.AddDefine("HLSL_VERSION", extHlslVersion);
                    }
                    var compile_sm = sm;
                    if (sm == null)
                    {
                        compile_sm = "6_5";
                    }
                    var ok = mShaderCompiler.CompileShader(desc, shader, entry, type, compile_sm, defPtr, NxRHI.EShaderLanguage.SL_SPIRV, bDebugShader, 
                        extHlslVersion, "-fspv-extension=SPV_KHR_shader_draw_parameters");
                    if (ok == false)
                        return null;
                }

                return desc;
            }
        }
    }
}

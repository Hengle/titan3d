﻿using System;
using System.Collections.Generic;
using System.Text;
using SDL2;

namespace EngineNS.Graphics.Pipeline
{
    public partial class UGfxDevice : UModule<UEngine>
    {
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine engine)
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) == -1)
                return false;

            var wtType = Rtti.UTypeDescManager.Instance.GetTypeFromString(engine.Config.MainWindowType);
            if (wtType == null)
            {
                wtType = typeof(EngineNS.Editor.UMainEditorApplication);
            }
            MainWindow = Rtti.UTypeDescManager.CreateInstance(wtType) as Graphics.Pipeline.USlateApplication;
            var winRect = engine.Config.MainWindow;

            //if (false == MainWindow.CreateNativeWindow("T3D", (int)winRect.X, (int)winRect.Y, (int)winRect.Z, (int)winRect.W))
            if (false == MainWindow.CreateNativeWindow("T3D", (int)winRect.X, (int)winRect.Y, (int)1, (int)1))
            {
                return false;
            }   
            if (InitGPU(engine.Config.AdaperId, engine.Config.RHIType, MainWindow.NativeWindow.HWindow, engine.Config.HasDebugLayer) == false)
            {
                return false;
            }

            SlateRenderer = new EGui.Slate.BaseRenderer();
            SlateRenderer.Initialize(RName.GetRName("shaders/slate/imgui-vertex.hlsl", RName.ERNameType.Engine), RName.GetRName("shaders/slate/imgui-frag.hlsl", RName.ERNameType.Engine));

            var rpType = Rtti.UTypeDescManager.Instance.GetTypeFromString(engine.Config.MainWindowRPolicy);
            await MainWindow.InitializeApplication(RenderContext, rpType);
            MainWindow.OnResize(winRect.Z, winRect.W);

            engine.TickableManager.AddTickable(TextureManager);
            return true;
        }
        public override void Tick(UEngine engine)
        {
            if (PerFrameCBuffer != null)
            {
                var timeOfSecend = (float)engine.CurrentTickCount * 0.001f;
                PerFrameCBuffer.SetValue(PerFrameCBuffer.PerFrameIndexer.Time, ref timeOfSecend);
                var timeSin = Math.Sin(timeOfSecend);
                PerFrameCBuffer.SetValue(PerFrameCBuffer.PerFrameIndexer.TimeSin, ref timeSin);
                var timeCos = Math.Cos(timeOfSecend);
                PerFrameCBuffer.SetValue(PerFrameCBuffer.PerFrameIndexer.TimeSin, ref timeCos);
            }
        }
        public override void EndFrame(UEngine engine)
        {
            RenderContext?.mCoreObject.EndFrame();
        }
        public override void Cleanup(UEngine engine)
        {
            TextureManager?.Cleanup();
            MaterialManager?.Cleanup();
            MaterialManager = null;
            MaterialInstanceManager?.Cleanup();
            MaterialInstanceManager = null;

            BlendStateManager.Cleanup();
            RasterizerStateManager.Cleanup();
            DepthStencilStateManager.Cleanup();
            SamplerStateManager.Cleanup();
            InputLayoutManager.Cleanup();
            HitproxyManager.Cleanup();

            EffectManager.Cleanup();

            SlateRenderer?.Cleanup();
            SlateRenderer = null;
            RenderContext?.Dispose();
            RenderContext = null;
            RenderSystem?.Dispose();
            RenderSystem = null;
            MainWindow?.Cleanup();
            MainWindow = null;

            Editor.ShaderCompiler.UShaderCodeManager.Instance.Cleanup();
            SDL.SDL_Quit();
        }
        public USlateApplication MainWindow;
        public RHI.CRenderSystem RenderSystem { get; private set; }
        public RHI.CRenderContext RenderContext { get; private set; }
        private unsafe bool InitGPU(UInt32 Adapter, ERHIType rhi, IntPtr window, bool bDebugLayer)
        {
            if (UEngine.Instance.PlayMode != EPlayMode.Game)
            {
                Editor.ShaderCompiler.UShaderCodeManager.Instance.Initialize(RName.GetRName("shaders/", RName.ERNameType.Engine));
            }

            var sysDesc = new IRenderSystemDesc();
            sysDesc.WindowHandle = window.ToPointer();
            sysDesc.CreateDebugLayer = bDebugLayer?1:0;
            RenderSystem = RHI.CRenderSystem.CreateRenderSystem(rhi, ref sysDesc);
            if (RenderSystem == null)
                return false;

            var deviceNum = RenderSystem.NumOfContext;
            if (Adapter >= deviceNum)
                return false;
            var rcDesc = new IRenderContextDesc();
            rcDesc.SetDefault();
            RenderSystem.GetContextDesc(Adapter, ref rcDesc);

            rcDesc.CreateDebugLayer = bDebugLayer?1:0;
            RenderContext = RenderSystem.CreateContext(ref rcDesc);
            if (RenderContext == null)
                return false;

            return true;
        }
        
        #region Manager
        public RHI.UTextureManager TextureManager { get; } = new RHI.UTextureManager();
        public Shader.UMaterialManager MaterialManager { get; private set; } = new Shader.UMaterialManager();
        public Shader.UMaterialInstanceManager MaterialInstanceManager { get; private set; } = new Shader.UMaterialInstanceManager();
        public EGui.Slate.BaseRenderer SlateRenderer { get; private set; }
        public Graphics.Pipeline.Shader.UEffectManager EffectManager
        {
            get;
        } = new Graphics.Pipeline.Shader.UEffectManager();
        public Graphics.Pipeline.UBlendStateManager BlendStateManager
        {
            get;
        } = new Graphics.Pipeline.UBlendStateManager();
        public Graphics.Pipeline.URasterizerStateManager RasterizerStateManager
        {
            get;
        } = new Graphics.Pipeline.URasterizerStateManager();
        public Graphics.Pipeline.UDepthStencilStateManager DepthStencilStateManager
        {
            get;
        } = new Graphics.Pipeline.UDepthStencilStateManager();
        public Graphics.Pipeline.USamplerStateManager SamplerStateManager
        {
            get;
        } = new Graphics.Pipeline.USamplerStateManager();
        public Graphics.Pipeline.UInputLayoutManager InputLayoutManager
        {
            get;
        } = new Graphics.Pipeline.UInputLayoutManager();
        public Graphics.Pipeline.UHitproxyManager HitproxyManager
        {
            get;
        } = new Graphics.Pipeline.UHitproxyManager();
        #endregion

        #region GraphicsData
        public RHI.CConstantBuffer PerFrameCBuffer { get; set; }
        #endregion
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        public Graphics.Pipeline.UGfxDevice GfxDevice { get; } = new Graphics.Pipeline.UGfxDevice();
    }
}
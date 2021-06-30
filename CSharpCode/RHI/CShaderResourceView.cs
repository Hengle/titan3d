﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    [Rtti.Meta]
    public class CShaderResourceViewAMeta : IO.IAssetMeta
    {
        System.Threading.Tasks.Task<CShaderResourceView> Task;
        IntPtr SnapshotPtr;
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public unsafe override void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            if (SnapshotPtr == IntPtr.Zero)
            {
                if (Task == null)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    Task = UEngine.Instance.GfxDevice.TextureManager.GetTexture(this.GetAssetName(), 1);
                }
                else if(Task.IsCompleted)
                {
                    SnapshotPtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(Task.Result));
                    Task = null;
                }
            }

            var start = ImGuiAPI.GetItemRectMin();
            var end = start + sz;

            var name = IO.FileManager.GetPureName(GetAssetName().Name);
            var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
            Vector2 tpos;
            tpos.Y = start.Y + sz.Y - tsz.Y;
            tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
            ImGuiAPI.PushClipRect(ref start, ref end, true);

            end.Y -= tsz.Y;
            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            if (SnapshotPtr != IntPtr.Zero)
            {
                cmdlist.AddImage(SnapshotPtr.ToPointer(), ref start, ref end, ref uv0, ref uv1, 0xFFFFFFFF);
            }

            cmdlist.AddText(ref tpos, 0xFFFF00FF, name, null);
            ImGuiAPI.PopClipRect();

        }
        public override void OnShowIconTimout(int time)
        {
            if (SnapshotPtr != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                handle.Free();
                SnapshotPtr = IntPtr.Zero;
                Task = null;
            }
        }
    }
    [Rtti.Meta]
    [CShaderResourceView.Import]
    [IO.AssetCreateMenu(MenuName = "Texture")]
    public partial class CShaderResourceView : AuxPtrType<IShaderResourceView>, IO.IAsset, IO.IStreaming
    {
        public CShaderResourceView()
        {
            System.Threading.Interlocked.Increment(ref NumOfInstance);
        }
        ~CShaderResourceView()
        {
            System.Threading.Interlocked.Decrement(ref NumOfInstance);
        }
        public static int NumOfInstance = 0;
        public const string AssetExt = ".srv";

        public class UPicDesc
        {
            public ITxPicDesc Desc;
            public int sRGB { get => Desc.sRGB; set => Desc.sRGB = value; }
            public ETCFormat EtcFormat { get => Desc.EtcFormat; set => Desc.EtcFormat = value; }
            public int MipLevel { get => Desc.MipLevel; set => Desc.MipLevel = value; }
            public int Width { get => Desc.Width; set => Desc.Width = value; }
            public int Height { get => Desc.Height; set => Desc.Height = value; }
            public List<Int32_2> MipSizes = new List<Int32_2>();
        }
        public UPicDesc PicDesc { get; set; }
        public class ImportAttribute : IO.IAssetCreateAttribute
        {
            bool bPopOpen = false;
            bool bFileExisting = false;
            RName mDir;
            string mName;
            string mSourceFile;
            public UPicDesc mDesc = new UPicDesc();
            ImGui.ImGuiFileDialog mFileDialog = ImGui.ImGuiFileDialog.CreateInstance();
            EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                mDir = dir;
                mDesc.Desc.SetDefault();
                PGAsset.SingleTarget = mDesc;
            }
            public override unsafe void OnDraw(EGui.Controls.ContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import SRV", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var visible = true;
                if (ImGuiAPI.BeginPopupModal($"Import SRV", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var sz = new Vector2(-1, 0);
                    if (ImGuiAPI.Button("Select Image", ref sz))
                    {
                        mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".png,.jpg,.bmp,.tga", "."); 
                    }
                    // display
                    if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                    {
                        // action if OK
                        if (mFileDialog.IsOk() == true)
                        {
                            mSourceFile = mFileDialog.GetFilePathName();
                            string filePath = mFileDialog.GetCurrentPath();
                            using (var stream = System.IO.File.OpenRead(mSourceFile))
                            {
                                var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                                if (image != null)
                                {
                                    mDesc.Width = image.Width;
                                    mDesc.Height = image.Height;

                                    int height = image.Height;
                                    int width = image.Width;
                                    int mipLevel = 0;
                                    do
                                    {
                                        height = height / 2;
                                        width = width / 2;
                                        mipLevel++;
                                    }
                                    while (height > 0 && width > 0);

                                    mDesc.MipLevel = mipLevel;
                                }
                                mName = IO.FileManager.GetPureName(mSourceFile);
                            }
                        }
                        // close
                        mFileDialog.CloseDialog();
                    }
                    if (bFileExisting)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(ref clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(ref clr, $"Source:{mSourceFile}");
                    }
                    ImGuiAPI.Separator();

                    var buffer = BigStackBuffer.CreateInstance(256);
                    buffer.SetText(mName);
                    ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                    var name = buffer.AsText();
                    if (mName != name)
                    {
                        mName = name;
                        bFileExisting = IO.FileManager.FileExists(mDir.Address + mName + RHI.CShaderResourceView.AssetExt);
                    }
                    buffer.DestroyMe();

                    ImGuiAPI.Separator();

                    PGAsset.OnDraw(false, false, false);

                    ImGuiAPI.Separator();

                    sz = new Vector2(0, 0);
                    if (bFileExisting == false)
                    {
                        if (ImGuiAPI.Button("Create Asset", ref sz))
                        {
                            if (ImportImage())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                ContentBrowser.mAssetImporter = null;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", ref sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }
                    ImGuiAPI.EndPopup();
                }
            }
            private bool ImportImage()
            {
                using (var stream = System.IO.File.OpenRead(mSourceFile))
                {
                    if (stream == null)
                        return false;
                    var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;

                    mDesc.Width = image.Width;
                    mDesc.Height = image.Height;

                    var rn = RName.GetRName(mDir.Name + mName + RHI.CShaderResourceView.AssetExt);

                    var xnd = new IO.CXndHolder("CShaderResourceView", 0, 0);
                    CShaderResourceView.SaveTexture(xnd.RootNode.mCoreObject, image, mDesc);
                    xnd.SaveXnd(rn.Address);

                    var ameta = new CShaderResourceViewAMeta();
                    ameta.SetAssetName(rn);
                    ameta.AssetId = Guid.NewGuid();
                    ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(CShaderResourceView));
                    ameta.Description = $"This is a {typeof(CShaderResourceView).FullName}\n";
                    ameta.SaveAMeta();

                    UEngine.Instance.AssetMetaManager.RegAsset(ameta);
                }
                return true;
            }

            public static bool ImportImage(string sourceFile, RName dir)
            {
                using (var stream = System.IO.File.OpenRead(sourceFile))
                {
                    if (stream == null)
                        return false;
                    var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;

                    var desc = new UPicDesc();

                    desc.Width = image.Width;
                    desc.Height = image.Height;
                    var name = IO.FileManager.GetPureName(sourceFile);

                    var rn = RName.GetRName(dir.Name.TrimEnd('\\').TrimEnd('/') + "/" + name + RHI.CShaderResourceView.AssetExt, dir.RNameType);

                    var xnd = new IO.CXndHolder("CShaderResourceView", 0, 0);
                    CShaderResourceView.SaveTexture(xnd.RootNode.mCoreObject, image, desc);
                    xnd.SaveXnd(rn.Address);

                    var ameta = new CShaderResourceViewAMeta();
                    ameta.SetAssetName(rn);
                    ameta.AssetId = Guid.NewGuid();
                    ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(CShaderResourceView));
                    ameta.Description = $"This is a {typeof(CShaderResourceView).FullName}\n";
                    ameta.SaveAMeta();

                    UEngine.Instance.AssetMetaManager.RegAsset(ameta);
                }
                return true;
            }
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new CShaderResourceViewAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            //这里需要存盘的情况很少，正常来说srb是Image导入的时候生成的，不是保存出来的
            //mCoreObject.Save2Xnd()
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        #region IStreaming
        public int LevelOfDetail { get; set; }
        public int TargetLOD { get; set; }
        public int MaxLOD
        {
            get
            {
                return PicDesc.MipLevel;
            }
        }
        public System.Threading.Tasks.Task<bool> CurLoadTask { get; set; }
        public async System.Threading.Tasks.Task<bool> LoadLOD(int level)
        {
            if (level < 0 || level > MaxLOD)
                return false;
            var tex2d = await UEngine.Instance.EventPoster.Post(() =>
            {
                var xnd = IO.CXndHolder.LoadXnd(AssetName.Address);
                if (xnd == null)
                    return null;

                var tex = LoadPngTexture2DMipLevel(xnd.RootNode.mCoreObject, PicDesc, level);
                if (tex == null)
                    return null;
                return tex;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            LevelOfDetail = level;
            unsafe
            {
                if (tex2d == null)
                {
                    return this.mCoreObject.UpdateTexture2D(rc.mCoreObject, new ITexture2D((void*)0));
                }
                else
                {
                    return this.mCoreObject.UpdateTexture2D(rc.mCoreObject, tex2d.mCoreObject);
                }
            }
        }
        #endregion
        public static unsafe void SaveTexture(XndNode node, StbImageSharp.ImageResult image, UPicDesc desc)
        {
            using (var memStream = new System.IO.MemoryStream(image.Data.Length))
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                var pngData = memStream.ToArray();
                var attr = new XndAttribute(node.GetOrAddAttribute("Png", 0, 0));
                var ar = attr.GetWriter((ulong)memStream.Position);
                ar.WriteNoSize(pngData, (int)memStream.Position);
                attr.ReleaseWriter(ref ar);
            }

            int height = image.Height;
            int width = image.Width;
            int mipLevel = 0;
            var curImage = image;
            desc.MipSizes.Clear();
            var pngMipsNode = new XndNode(node.GetOrAddNode("PngMips", 0, 0));
            do
            {
                using (var memStream = new System.IO.MemoryStream(curImage.Data.Length))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                    //writer.WriteJpg(curImage.Data, curImage.Width, curImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream, 100);
                    var pngData = memStream.ToArray();
                    var attr = new XndAttribute(pngMipsNode.GetOrAddAttribute($"PngMip{mipLevel}", 0, 0));
                    var ar = attr.GetWriter((ulong)memStream.Position);
                    ar.WriteNoSize(pngData, (int)memStream.Position);
                    attr.ReleaseWriter(ref ar);
                }
                desc.MipSizes.Add(new Int32_2() { x = width, y = height });
                height = height / 2;
                width = width / 2;
                mipLevel++;
                if ((height > 0 && width > 0))
                {
                    curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, width, height);
                }
            }
            while (height > 0 && width > 0);

            {
                desc.Desc.MipLevel = mipLevel;
                var attr = new XndAttribute(node.GetOrAddAttribute("Desc", 0, 0));
                var ar = attr.GetWriter((ulong)sizeof(ITxPicDesc));
                ar.Write(desc.Desc);
                ar.Write(desc.MipSizes.Count);
                for (int i = 0; i < desc.MipSizes.Count; i++)
                {
                    ar.Write(desc.MipSizes[i]);
                }
                attr.ReleaseWriter(ref ar);
            }
        }
        public static unsafe CTexture2D LoadPngTexture2DMipLevel(XndNode node, UPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var pngNode = new XndNode(node.TryGetChildNode("PngMips"));
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[mipLevel];
            try
            {
                var pInitData = stackalloc ImageInitData[mipLevel];

                StbImageSharp.ColorComponents colorComp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
                for (uint i = 0; i < mipLevel; i++)
                {
                    var ptr = pngNode.TryGetAttribute($"PngMip{desc.MipLevel - mipLevel + i}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = new XndAttribute(ptr);
                    var ar = mipAttr.GetReader(null);
                    byte[] data;
                    ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    mipAttr.ReleaseReader(ref ar);

                    StbImageSharp.ImageResult image;
                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        image = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                    handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(image.Data, System.Runtime.InteropServices.GCHandleType.Pinned);
                    pInitData[i].SetDefault();
                    pInitData[i].pSysMem = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0).ToPointer();
                    pInitData[i].SysMemPitch = (uint)image.Width * 4;

                    colorComp = image.Comp;
                }

                var texDesc = new ITexture2DDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].x;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                switch (colorComp)
                {
                    case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                        texDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                        break;
                }

                return rc.CreateTexture2D(ref texDesc);
            }
            finally
            {
                for (uint i = 0; i < mipLevel; i++)
                {
                    if (handles[i].IsAllocated)
                        handles[i].Free();
                }
            }
        }
        public static async System.Threading.Tasks.Task<CShaderResourceView> LoadSrvMipmap(RName rn, int mipLevel)
        {
            UPicDesc desc = new UPicDesc();
            var tex2d = await UEngine.Instance.EventPoster.Post(() =>
            {
                var xnd = IO.CXndHolder.LoadXnd(rn.Address);
                if (xnd == null)
                    return null;

                {
                    var attr = xnd.RootNode.TryGetAttribute("Desc");
                    var ar = attr.GetReader(null);
                    ar.Read(out desc.Desc);
                    int len;
                    ar.Read(out len);
                    for (int i = 0; i < len; i++)
                    {
                        Int32_2 tmp;
                        ar.Read(out tmp);
                        desc.MipSizes.Add(tmp);
                    }
                    attr.ReleaseReader(ref ar);
                }

                if (mipLevel == -1 || mipLevel > desc.MipLevel)
                    mipLevel = desc.MipLevel;

                var tex = LoadPngTexture2DMipLevel(xnd.RootNode.mCoreObject, desc, mipLevel);
                if (tex == null)
                    return null;

                return tex;
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (tex2d == null)
                return null;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.mFormat = tex2d.mCoreObject.mDesc.Format;
            unsafe
            {
                srvDesc.m_pTexture2D = tex2d.mCoreObject;
            }
            var result = rc.CreateShaderResourceView(ref srvDesc);
            result.PicDesc = desc;
            result.LevelOfDetail = mipLevel;
            result.TargetLOD = mipLevel;
            result.AssetName = rn;
            return result;
        }
    }
    public class CShaderResources : AuxPtrType<IShaderResources>
    {
        public CShaderResources()
        {
            mCoreObject = IShaderResources.CreateInstance();
        }
    }
    public class CShaderSamplers : AuxPtrType<IShaderSamplers>
    {
        public CShaderSamplers()
        {
            mCoreObject = IShaderSamplers.CreateInstance();
        }
    }

    public class UTextureManager : IO.UStreamingManager, ITickable
    {
        public UTextureManager()
        {
            
        }
        ~UTextureManager()
        {
            Cleanup();
            UEngine.Instance?.TickableManager.RemoveTickable(this);
        }
        public void Cleanup()
        {
            //foreach(var i in StreamingAssets)
            //{                
            //}
            StreamingAssets.Clear();
        }
        List<RName> mWaitRemoves = new List<RName>();
        public async System.Threading.Tasks.Task<CShaderResourceView> GetTexture(RName rn, int mipLevel = 1)
        {
            CShaderResourceView srv;
            IO.IStreaming result;
            if (StreamingAssets.TryGetValue(rn, out result))
            {
                srv = result as CShaderResourceView;
                if (srv == null)
                    return null;
                srv.TargetLOD = mipLevel;
                return srv;
            }

            srv = await CShaderResourceView.LoadSrvMipmap(rn, mipLevel);
            if (srv == null)
                return null;
            if (StreamingAssets.TryGetValue(rn, out result) == false)
            {
                StreamingAssets.Add(rn, srv);
            }
            else
            {
                srv = result as CShaderResourceView;
            }
            return srv;
        }
        public CShaderResourceView TryGetTexture(RName rn)
        {
            CShaderResourceView srv;
            IO.IStreaming result;
            if (StreamingAssets.TryGetValue(rn, out result))
            {
                srv = result as CShaderResourceView;
                if (srv == null)
                    return null;
                return srv;
            }
            return null;
        }
        public unsafe override void UpdateTargetLOD(IO.IStreaming asset)
        {
            var srv = asset as CShaderResourceView;
            if (srv == null)
                return;

            var now = UEngine.Instance.CurrentTickCount;
            var resState = new IResourceState(srv.mCoreObject.GetResourceState());
            if (now - resState.GetAccessTime() > 15 * 1000 * 1000)
            {
                srv.TargetLOD = 0;
            }
            else
            {
                srv.TargetLOD = srv.MaxLOD;
            }
        }
        int TickInterval = 150;
        int EllapsedRemainTime = 150;
        public void TickLogic(int ellapse)
        {
            EllapsedRemainTime -= ellapse;
            if (EllapsedRemainTime <= 0)
            {
                UpdateStreamingState();
                EllapsedRemainTime = TickInterval;
            }
            foreach(var i in mWaitRemoves)
            {
                StreamingAssets.Remove(i);
            }
            mWaitRemoves.Clear();
        }
        public void TickRender(int ellapse)
        {

        }
        public void TickSync(int ellapse)
        {

        }
    }
}

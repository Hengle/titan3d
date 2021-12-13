﻿using EngineNS.EGui.Controls;
using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public partial class UMacrossAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UMacross.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        public override void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            IO.FileManager.DeleteDirectory(address);
            IO.FileManager.DeleteFile(address + IAssetMeta.MetaExt);
        }
        public override bool CanRefAssetType(IAssetMeta ameta)
        {
            // macross可以引用所有类型的资源
            return true;
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            cmdlist.AddText(in start, 0xFFFFFFFF, "Macross", null);
        }
    }

    [Rtti.Meta]
    [UMacross.MacrossCreate]
    [IO.AssetCreateMenu(MenuName = "Macross")]
    [Editor.UAssetEditor(EditorType = typeof(Bricks.CodeBuilder.MacrossNode.ClassGraph))]
    public partial class UMacross : IO.IAsset
    {
        public const string AssetExt = ".macross";

        public class MacrossCreateAttribute : IO.CommonCreateAttribute
        {
            UTypeDesc mSelectedType = null;
            public override void DoCreate(RName dir, UTypeDesc type, string ext)
            {
                base.DoCreate(dir, type, ext);
                mSelectedType = null;
            }

            public override unsafe void OnDraw(ContentBrowser contentBrowser)
            {
                //base.OnDraw(contentBrowser);
                
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"New Macross", ImGuiPopupFlags_.ImGuiPopupFlags_None);

                var visible = true;
                if (ImGuiAPI.BeginPopupModal($"New Macross", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    switch(eErrorType)
                    {
                        case enErrorType.IsExisting:
                            {
                                var clr = new Vector4(1, 0, 0, 1);
                                ImGuiAPI.TextColored(&clr, $"{mName} is existing");
                            }
                            break;
                        case enErrorType.EmptyName:
                            {
                                var clr = new Vector4(1, 0, 0, 1);
                                ImGuiAPI.TextColored(&clr, $"Name is empty");
                            }
                            break;
                    }

                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.Text("Base Type:");
                    ImGuiAPI.SameLine(0, -1);
                    if(EGui.UIProxy.ComboBox.BeginCombo("##TypeSel", (mSelectedType == null)? "None" : mSelectedType.Name))
                    {
                        var comboDrawList = ImGuiAPI.GetWindowDrawList();
                        var searchBar = UEngine.Instance.UIManager["MacrossTypeSearchBar"] as EGui.UIProxy.SearchBarProxy;
                        if(searchBar == null)
                        {
                            searchBar = new EGui.UIProxy.SearchBarProxy()
                            {
                                InfoText = "Search macross base type",
                                Width = -1,
                            };
                            UEngine.Instance.UIManager["MacrossTypeSearchBar"] = searchBar;
                        }
                        if(!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
                            ImGuiAPI.SetKeyboardFocusHere(0);
                        searchBar.OnDraw(ref comboDrawList, ref Support.UAnyPointer.Default);
                        bool bSelected = true;
                        foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
                        {
                            foreach (var type in service.Types.Values)
                            {
                                if (type.IsValueType)
                                    continue;
                                if (type.IsSealed)
                                    continue;

                                var atts = type.SystemType.GetCustomAttributes(typeof(Macross.UMacrossAttribute), false);
                                if (atts == null || atts.Length == 0)
                                    continue;

                                if (!string.IsNullOrEmpty(searchBar.SearchText) && !type.FullName.ToLower().Contains(searchBar.SearchText.ToLower()))
                                    continue;

                                if(ImGuiAPI.Selectable(type.Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                                {
                                    mSelectedType = type;
                                }
                                if(ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                {
                                    CtrlUtility.DrawHelper(type.FullName);
                                }
                            }
                        }

                        EGui.UIProxy.ComboBox.EndCombo();
                    }

                    var buffer = BigStackBuffer.CreateInstance(256);
                    buffer.SetText(mName);
                    ImGuiAPI.SetNextItemWidth(-1);
                    ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                    var name = buffer.AsText();
                    eErrorType = enErrorType.None;
                    if (string.IsNullOrEmpty(name))
                        eErrorType = enErrorType.EmptyName;
                    else if(mName != name)
                    {
                        mName = name;
                        var rn = RName.GetRName(mDir.Name + name + ExtName, mDir.RNameType);
                        if (mAsset != null)
                            mAsset.AssetName = rn;
                        if (IO.FileManager.FileExists(rn.Address))
                            eErrorType = enErrorType.IsExisting;
                    }
                    buffer.DestroyMe();

                    if(ImGuiAPI.Button("Create Asset", in Vector2.Zero))
                    {
                        var rn = RName.GetRName(mDir.Name + name + ExtName, mDir.RNameType);
                        if(IO.FileManager.FileExists(rn.Address) == false && string.IsNullOrWhiteSpace(mName) == false)
                        {
                            ((UMacross)mAsset).mSelectedType = mSelectedType;
                            if (DoImportAsset())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                contentBrowser.mAssetImporter = null;
                            }
                        }
                    }
                    ImGuiAPI.SameLine(0, 20);
                    if(ImGuiAPI.Button("Cancel", in Vector2.Zero))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        contentBrowser.mAssetImporter = null;
                    }

                    ImGuiAPI.EndPopup();
                }// */
            }
        }

        public RName AssetName
        {
            get;
            set;
        }

        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UMacrossAMeta();
            result.Icon = new EGui.UUvAnim();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {

        }

        UTypeDesc mSelectedType = null;
        public void SaveAssetTo(RName name)
        {
            IO.FileManager.CreateDirectory(name.Address);

            var graph = new MacrossNode.ClassGraph();
            graph.AssetName = name;
            graph.DefClass.ClassName = name.PureName;
            graph.DefClass.NameSpace = IO.FileManager.GetParentPathName(name.Name).TrimEnd('/').Replace('/', '.');
            if (mSelectedType != null)
                graph.DefClass.SuperClassName = mSelectedType.FullName;
            graph.SaveClassGraph(name);
        }
    }
}

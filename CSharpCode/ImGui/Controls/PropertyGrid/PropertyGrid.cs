﻿using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public partial class PropertyGrid
    {
        //List<object> mTargetObjects;
        //public List<object> TargetObjects
        //{
        //    get { return mTargetObjects; }
        //    set
        //    {
        //        mTargetObjects = value;
        //    }
        //}
        public object Tag
        {
            get;
            set;
        }
        public object Target { get; set; }
    }

    public class PGCatogoryAttribute : Attribute
    {
        public string Name;
        public PGCatogoryAttribute(string n)
        {
            Name = n;
        }
    }
    public class PGCustomValueEditorAttribute : Attribute
    {
        public PGProvider Provider = null;
        public bool HideInPG = false;
        public bool ReadOnly = false;
        public bool UserDraw = true;
        protected bool FullRedraw = false;
        public bool Expandable = false;
        public bool IsFullRedraw
        {
            get => FullRedraw;
        }

        public struct EditorInfo
        {
            public string Name;
            public Rtti.UTypeDesc Type;
            public object Value;
            public object ObjectInstance;
            public float RowHeight;
            public Controls.PropertyGrid.PropertyGrid HostPropertyGrid;
            public bool Readonly;
            public bool Expand;
            public ImGuiTreeNodeFlags_ Flags;
        }
        //public virtual void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        public virtual bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = default;
            return false;
        }
    }
    public class PGTypeEditorAttribute : PGCustomValueEditorAttribute
    {
        public string AssemblyFilter = null;
        public bool ExcludeSealed = false;
        public bool ExcludeValueType = false;
        public Rtti.UTypeDesc BaseType;
        public PGTypeEditorAttribute()
        {
            BaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(void).FullName);
        }
        protected static EGui.Controls.TypeSelector TypeSlt = new EGui.Controls.TypeSelector();
        public override bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var sz = new Vector2(0, 0);
            var bindType = EGui.UIEditor.EditableFormData.Instance.CurrentForm.BindType;
            if (bindType == null)
                return false;
            //var props = bindType.SystemType.GetProperties();
            ImGuiAPI.SetNextItemWidth(-1);
            TypeSlt.AssemblyFilter = AssemblyFilter;
            TypeSlt.ExcludeSealed = ExcludeSealed;
            TypeSlt.ExcludeValueType = ExcludeValueType;
            TypeSlt.BaseType = BaseType;
            var typeStr = info.Value as string;
            TypeSlt.SelectedType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeStr);
            if (TypeSlt.OnDraw(-1, 12))
            {
                newValue = TypeSlt.SelectedType;
                return true;
                //foreach (var i in props)
                //{
                //    var v = TypeSlt.SelectedType;
                //    prop.SetValue(ref target, v);
                //    //foreach (var j in pg.TargetObjects)
                //    //{
                //    //    EGui.Controls.PropertyGrid.PropertyGrid.SetValue(pg, j, callstack, prop, target, v);
                //    //}
                //}
            }
            return false;
        }
    }

    public class PGProvider
    {
        public virtual string GetDisplayName(object arg)
        {
            return arg.ToString();
        }
        public virtual bool IsReadOnly(object arg)
        {
            return false;
        }
        public virtual Rtti.UTypeDesc GetPropertyType(object arg)
        {
            return null;
        }
        public virtual void SetValue(object arg, object val) { }
        public virtual object GetValue(object arg) { return null; }
    }

    public class PGTypeEditorManager
    {
        public static PGTypeEditorManager Instance { get; } = new PGTypeEditorManager();

        private PGTypeEditorManager()
        {
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(bool)), new BoolEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(int)), new Int32Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(sbyte)), new SByteEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(short)), new Int16Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(long)), new Int64Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(uint)), new UInt32Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(byte)), new ByteEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(ushort)), new UInt16Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(ulong)), new UInt64Editor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(float)), new FloatEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(double)), new DoubleEditor());
            RegTypeEditor(Rtti.UTypeDesc.TypeOf(typeof(string)), new StringEditor());
        }

        public ObjectWithCreateEditor ObjectWithCreateEditor = new ObjectWithCreateEditor();
        public EnumEditor EnumEditor = new EnumEditor();
        public ArrayEditor ArrayEditor = new ArrayEditor();
        public ListEditor ListEditor = new ListEditor();
        public DictionaryEditor DictionaryEditor = new DictionaryEditor();

        Dictionary<Rtti.UTypeDesc, PGCustomValueEditorAttribute> mTypeEditors = new Dictionary<Rtti.UTypeDesc, PGCustomValueEditorAttribute>();
        public PGCustomValueEditorAttribute GetEditorType(Rtti.UTypeDesc type)
        {
            PGCustomValueEditorAttribute result;
            if (mTypeEditors.TryGetValue(type, out result))
                return result;
            return null;
        }
        public void RegTypeEditor(Rtti.UTypeDesc type, PGCustomValueEditorAttribute editorType)
        {
            mTypeEditors[type] = editorType;
        }

        public bool DrawTypeEditor(in PGCustomValueEditorAttribute.EditorInfo info, out object newValue, out bool valueChanged)
        {
            valueChanged = false;
            newValue = info.Value;
            PGCustomValueEditorAttribute editor;
            if(mTypeEditors.TryGetValue(info.Type, out editor))
            {
                valueChanged = editor.OnDraw(in info, out newValue);
                return true;
            }

            return false;
        }
    }
    public class ColorEditorBaseAttribute : PGCustomValueEditorAttribute
    {
        public bool mHDR = false;
        public bool mDragAndDrop = true;
        public bool mOptionMenu = true;
        public bool mAlphaHalfPreview = true;
        public bool mAlphaPreview = true;
    }
    public class Color3PickerEditorAttribute : ColorEditorBaseAttribute
    {
        bool mPopupOn = false;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var id = ImGuiAPI.GetID("#Color3Picker");
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var height = ImGuiAPI.GetFrameHeight();
            startPos.Y += (height - EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            var drawCol = Color3.ToAbgr((Vector3)info.Value);
            drawList.AddRectFilled(ref startPos, ref endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            drawList.AddRect(ref startPos, ref endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(ref startPos, ref endPos, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(ref pos, ImGuiCond_.ImGuiCond_Always, ref pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                var v = (Vector3)info.Value;
                var saved = v;
                ImGuiAPI.ColorPicker3(TName.FromString2("##colorpicker_", info.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview);
                if (v != saved)
                {
                    newValue = v;
                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            return valueChanged;
        }
    }
    public class Color4PickerEditorAttribute : ColorEditorBaseAttribute
    {
        bool mPopupOn = false;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var id = ImGuiAPI.GetID("#Color4Picker");
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var height = ImGuiAPI.GetFrameHeight();
            startPos.Y += (height - EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            var drawCol = Color4.ToAbgr((Vector4)info.Value);
            drawList.AddRectFilled(ref startPos, ref endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            drawList.AddRect(ref startPos, ref endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(ref startPos, ref endPos, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (mPopupOn == false && click)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(ref pos, ImGuiCond_.ImGuiCond_Always, ref pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                var v = (Vector4)info.Value;
                var saved = v;
                ImGuiAPI.ColorPicker4(TName.FromString2("##colorpicker_", info.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview, (float*)0);
                if (v != saved)
                {
                    newValue = v;
                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            return valueChanged;
        }
    }
    public class UByte4ToColor4PickerEditorAttribute : ColorEditorBaseAttribute
    {
        public bool IsABGR = false;
        bool mPopupOn = false;
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = info.Value;

            var id = ImGuiAPI.GetID("#UByte4ToColor4Picker");
            var drawList = ImGuiAPI.GetWindowDrawList();
            var startPos = ImGuiAPI.GetCursorScreenPos();
            var boxSize = EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize;
            var height = ImGuiAPI.GetFrameHeight();
            startPos.Y += (height - boxSize.Y - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.Y * 2) * 0.5f;
            var endPos = startPos + boxSize;
            UInt32 drawCol;
            if (IsABGR)
                drawCol = (UInt32)info.Value;
            else
                drawCol = Color4.Argb2Abgr((UInt32)info.Value);
            drawList.AddRectFilled(ref startPos, ref endPos, drawCol, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None);
            drawList.AddRect(ref startPos, ref endPos, EGui.UIProxy.StyleConfig.Instance.PGItemBorderNormalColor, EGui.UIProxy.StyleConfig.Instance.PGColorBoxRound, ImDrawFlags_.ImDrawFlags_None, 1);
            bool hovered = false;
            bool held = false;
            var click = ImGuiAPI.ButtonBehavior(ref startPos, ref endPos, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if(mPopupOn == false && click)
            {
                var pos = startPos + new Vector2(0, EGui.UIProxy.StyleConfig.Instance.PGColorBoxSize.Y);
                var pivot = Vector2.Zero;
                ImGuiAPI.SetNextWindowPos(ref pos, ImGuiCond_.ImGuiCond_Always, ref pivot);
                ImGuiAPI.OpenPopup("colorPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("colorPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                mPopupOn = true;
                ImGuiColorEditFlags_ misc_flags = (mHDR ? ImGuiColorEditFlags_.ImGuiColorEditFlags_HDR : 0) | (mDragAndDrop ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoDragDrop) | (mAlphaHalfPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreviewHalf : (mAlphaPreview ? ImGuiColorEditFlags_.ImGuiColorEditFlags_AlphaPreview : 0)) | (mOptionMenu ? 0 : ImGuiColorEditFlags_.ImGuiColorEditFlags_NoOptions);
                Color4 v;
                if (IsABGR)
                    v = Color4.FromABGR((UInt32)info.Value);
                else
                    v = new Color4((UInt32)info.Value);
                var saved = v;
                ImGuiAPI.ColorPicker4(TName.FromString2("##colorpicker_", info.Name).ToString(), (float*)&v,
                    misc_flags | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSidePreview | ImGuiColorEditFlags_.ImGuiColorEditFlags_NoSmallPreview, (float*)0);
                if (v != saved)
                {
                    if (IsABGR)
                        newValue = v.ToAbgr();
                    else
                        newValue = v.ToArgb();
                    valueChanged = true;
                }
                ImGuiAPI.EndPopup();
            }
            else
                mPopupOn = false;
            return valueChanged;
        }
    }
}

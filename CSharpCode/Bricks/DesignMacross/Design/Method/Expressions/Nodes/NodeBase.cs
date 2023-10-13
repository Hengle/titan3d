﻿using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using Org.BouncyCastle.Asn1.GM;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Nodes
{
    public class TtNodeStyles
    {
        public static TtNodeStyles DefaultStyles = new TtNodeStyles();
        public TtNodeStyles()
        {
            PinInStyle.Image.TextureName = RName.GetRName(PinConnectedVarImg, RName.ERNameType.Engine);
            PinInStyle.Image.Size = new Vector2(15, 11);
            PinInStyle.DisconnectImage.TextureName = RName.GetRName(PinDisconnectedVarImg, RName.ERNameType.Engine);
            PinInStyle.DisconnectImage.Size = new Vector2(15, 11);

            PinOutStyle.Image.TextureName = RName.GetRName(PinConnectedVarImg, RName.ERNameType.Engine);
            PinOutStyle.Image.Size = new Vector2(15, 11);
            PinOutStyle.DisconnectImage.TextureName = RName.GetRName(PinDisconnectedVarImg, RName.ERNameType.Engine);
            PinOutStyle.DisconnectImage.Size = new Vector2(15, 11);
        }
        public enum EFlowMode
        {
            Horizon,
            Vertical,
        }
        public EFlowMode FlowMode { get; set; } = EFlowMode.Horizon;
        public class PinStyle
        {
            public EGui.UUvAnim Image { get; set; } = new EGui.UUvAnim();
            public EGui.UUvAnim DisconnectImage { get; set; } = new EGui.UUvAnim();
            public float Offset { get; set; } = 3;
            public float TextOffset { get; set; } = 3;
        }
        public PinStyle PinInStyle { get; set; } = new PinStyle();
        public PinStyle PinOutStyle { get; set; } = new PinStyle();
        public float PinSpacing { get; set; } = 5;
        public float PinPadding { get; set; } = 8;
        public Vector2 IconOffset { get; set; } = new Vector2(2, 2);
        public float MinSpaceInOut { get; set; } = 20.0f;
        public UInt32 TitleTextColor { get; set; } = 0xFFFFFFFF;
        public UInt32 TitleTextDarkColor { get; set; } = 0xFF8c8c8c;
        public UInt32 TitleTextErrorColor { get; set; } = 0xFF0000FF;
        public float TitleTextOffset { get; set; } = 5;
        public Vector2 TitlePadding = new Vector2(8, 6);
        public UInt32 PinTextColor { get; set; } = 0xFFFFFFFF;
        public UInt32 SelectedColor { get; set; } = 0xFFFF00FF;
        public UInt32 LinkerColor { get; set; } = 0xFF00FFFF;
        public UInt32 PreOrderLinkerColor { get; set; } = 0xFF00AAAA;
        public float LinkerThinkness { get; set; } = 3;
        public float LinkerMinDelta { get; set; } = 50;
        public float LinkerMaxDelta { get; set; } = 700;
        public UInt32 HighLightColor { get; set; } = 0xFF0000FF;
        public float BezierPixelPerSegement { get; set; } = 10.0f;

        public UInt32 DefaultTitleColor = 0xFF800000;
        public UInt32 DefaultPinColor = 0xFF69936e;
        public UInt32 ValuePinColor = 0xFF00d037;
        public UInt32 VectorPinColor = 0xFF23c9fe;
        public UInt32 ValueTitleColor = 0xFF69936e;
        public UInt32 EndingTitleColor = 0xFF204020;
        public UInt32 FuncTitleColor = 0xFFff9c00;
        public UInt32 VectorTitleColor = 0xFF23c9fe;

        public UInt32 GridStep = 16;
        public UInt32 GridNormalLineColor = 0xFF353535;
        public UInt32 GridSplitLineColor = 0xFF161616;
        public UInt32 GridBackgroundColor = 0xFF262626;

        public string NodeBodyImg = "uestyle/graph/regularnode_body.srv";
        public string NodeTitleImg = "uestyle/graph/regularnode_title_gloss.srv";
        public string NodeColorSpillImg = "uestyle/graph/regularnode_color_spill.srv";
        public string NodeTitleHighlightImg = "uestyle/graph/regularnode_title_highlight.srv";
        public string NodeShadowImg = "uestyle/graph/regularnode_shadow.srv";
        public string NodeShadowSelectedImg = "uestyle/graph/regularnode_shadow_selected.srv";
        public string PinConnectedVarImg = "uestyle/graph/pin_connected_vara.srv";
        public string PinDisconnectedVarImg = "uestyle/graph/pin_disconnected_vara.srv";
        public string PinConnectedExecImg = "uestyle/graph/execpin_connected.srv";
        public string PinDisconnectedExecImg = "uestyle/graph/execpin_disconnected.srv";
        public string PinHoverCueImg = "uestyle/graph/pin_hover_cue.srv";
        public string BreakpointNodeImg = "uestyle/graph/ip_breakpoint.srv";

    }

    public class TtGraphElement_NodeBase : IGraphElement, IDraggable, ILayoutable, IErrorable
    {
        public Guid Id { get; set; } = Guid.Empty;
        string mName = "NoName";
        public virtual string Name 
        { 
            get => mName; 
            set
            {
                if (mName == "NoName")
                    Label = value;
                mName = value;
            }
        }
        public virtual string Label { get; set; } = "NoName";
        public Vector2 Location { get; set; }
        public Vector2 AbsLocation { get; }
        public IGraphElement Parent { get; set; }
        public IDescription Description { get; set; }
        public SizeF Size { get; set; }
        public float TitleHeight { get; set; }
        public uint TitleColor { get; set; }
        public bool HasError { get; set; }
        public FMargin Margin { get; set; } = FMargin.Default;

        public bool Selected = false;
        public bool CanDrag()
        {
            throw new NotImplementedException();
        }

        public void Construct()
        {

        }

        public bool HitCheck(Vector2 pos)
        {
            return false;
        }

        public void OnDragging(Vector2 delta)
        {
            throw new NotImplementedException();
        }

        public void OnSelected()
        {
            Selected = true;
        }

        public void OnUnSelected()
        {
            Selected = false;
        }

        public SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(Size.Width + Margin.Left + Margin.Right, Size.Height + Margin.Top + Margin.Bottom);
        }

        public SizeF Arranging(Rect finalRect)
        {
            return finalRect.Size;
        }
    }

    public class TtGraphElementRender_NodeBase : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var node = renderableElement as TtGraphElement_NodeBase;
            var styles = TtNodeStyles.DefaultStyles;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var nodeStart = context.ViewPortTransform(node.AbsLocation);
            var nodeEnd = context.ViewPortTransform(node.AbsLocation + new Vector2(node.Size.Width, node.Size.Height));

            ImGuiAPI.SetWindowFontScale(context.ViewPortScale(1.0f));

            var shadowExt = new Vector2(12, 12);
            if(node.Selected)
            {
                var selImg = UEngine.Instance.UIProxyManager[styles.NodeShadowSelectedImg] as EGui.UIProxy.ImageProxy;
                if (selImg != null)
                    selImg.OnDraw(cmdlist, nodeStart - shadowExt, nodeEnd + shadowExt);
            }
            else
            {
                var shadowImg = UEngine.Instance.UIProxyManager[styles.NodeShadowImg] as EGui.UIProxy.ImageProxy;
                if (shadowImg != null)
                    shadowImg.OnDraw(cmdlist, nodeStart - shadowExt, nodeEnd + shadowExt);
            }
            var nodeBodyImg = UEngine.Instance.UIProxyManager[styles.NodeBodyImg] as EGui.UIProxy.ImageProxy;
            if (nodeBodyImg != null)
                nodeBodyImg.OnDraw(cmdlist, nodeStart, nodeEnd);

            var endTitle = context.ViewPortTransform(node.AbsLocation + new Vector2(node.Size.Width, node.TitleHeight));
            {//DrawTitle
                var titleImg = UEngine.Instance.UIProxyManager[styles.NodeTitleImg] as EGui.UIProxy.ImageProxy;
                if (titleImg != null)
                    titleImg.OnDraw(cmdlist, nodeStart, endTitle);
                var colorSpillImg = UEngine.Instance.UIProxyManager[styles.NodeColorSpillImg] as EGui.UIProxy.ImageProxy;
                if (colorSpillImg != null)
                    colorSpillImg.OnDraw(cmdlist, nodeStart, endTitle, node.TitleColor);
                var titleHighlightImg = UEngine.Instance.UIProxyManager[styles.NodeTitleHighlightImg] as EGui.UIProxy.ImageProxy;
                if (titleHighlightImg != null)
                    titleHighlightImg.OnDraw(cmdlist, nodeStart, endTitle);

                //Draw Node Name
                var drawStart = context.ViewPortTransform(node.AbsLocation + styles.TitlePadding);
                if (node.Name != node.Label && !string.IsNullOrEmpty(node.Label))
                {
                    cmdlist.AddText(in drawStart, styles.TitleTextDarkColor, node.Label, null);
                    var textSize = ImGuiAPI.CalcTextSize(node.Label, false, 0.0f);
                    drawStart.Y += textSize.Y + styles.TitleTextOffset;
                }
                if (node.HasError)
                    cmdlist.AddText(in drawStart, styles.TitleTextErrorColor, node.Name, null);
                else
                    cmdlist.AddText(in drawStart, styles.TitleTextColor, node.Name, null);
            }

            ImGuiAPI.SetWindowFontScale(1.0f);
        }
    }
}

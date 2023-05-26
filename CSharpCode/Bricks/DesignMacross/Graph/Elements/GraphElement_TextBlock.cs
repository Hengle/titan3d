﻿using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Render;
using EngineNS.DesignMacross.TimedStateMachine;
using Microsoft.CodeAnalysis;
using NPOI.SS.Formula.PTG;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Graph.Elements
{
    public struct FDesignTextBlockRenderingContext
    {
        public Vector2 Location = Vector2.Zero;
        public SizeF Size = new SizeF();
        public FDesignTextBlockRenderingContext()
        {

        }
        public FDesignTextBlockRenderingContext(Vector2 location, SizeF size)
        {
            Location = location;
            Size = size;
        }
    }
    public enum EHorizontalAlignment
    {
        Left,
        Center,
        Right,
    }
    public enum EVerticalAlignment
    {
        Top,
        Center,
        Bottom,
    }

    [ImGuiElementRender(typeof(TtGraphElementRender_TextBlock))]
    public class TtGraphElement_TextBlock : IGraphElement, ILayoutable
    {
        public string Content { get; set; } = string.Empty;
        public string Name { get; set; } = "Textblock";
        public Vector2 Location { get; set; } = Vector2.Zero;
        public Vector2 AbsLocation { get => TtGraphMisc.CalculateAbsLocation(this); }

        public IGraphElement Parent { get; set; } = null;
        public IDescription Description { get; set; } = null;
        public EHorizontalAlignment HorizontalAlign { get; set; } = EHorizontalAlignment.Left;
        public EVerticalAlignment VerticalAlign { get; set; } = EVerticalAlignment.Top;
        public float FontScale { get; set; } = 1;
        public Color4f TextColor { get; set; } = new Color4f(0, 0, 0);
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0, 0);


        public TtGraphElement_TextBlock()
        {
        }
        public TtGraphElement_TextBlock(string content, EVerticalAlignment verticalAlignment = EVerticalAlignment.Top, EHorizontalAlignment horizontalAlignment = EHorizontalAlignment.Left)
        {
            Content = content;
            VerticalAlign = verticalAlignment;
            HorizontalAlign = horizontalAlignment;
        }
        public void Construct()
        {
        }
        public bool HitCheck(Vector2 pos)
        {
            throw new NotImplementedException();
        }

        public void OnSelected()
        {
            throw new NotImplementedException();
        }

        public void OnUnSelected()
        {
            throw new NotImplementedException();
        }   

        #region ILayoutable
        public FMargin Margin { get; set; } = FMargin.Default;
        public SizeF Size
        {
            get
            {
                var oldScale = ImGuiAPI.GetFont().Scale;
                var font = ImGuiAPI.GetFont();
                font.Scale = FontScale;
                ImGuiAPI.PushFont(font);
                var size = ImGuiAPI.CalcTextSize(Content, false, 0);
                font.Scale = oldScale;
                ImGuiAPI.PopFont();

                return new SizeF(size.X, size.Y);
            }
            set
            {
                // nothing
            }
        }
        public SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(Size.Width + Margin.Left + Margin.Right, Size.Height + Margin.Top + Margin.Bottom);
        }

        public SizeF Arranging(Rect finalRect)
        {
            HorizontalAligning(finalRect);
            VerticalAligning(finalRect);
            Location += new Vector2(Margin.Left, Margin.Top);
            return finalRect.Size;
        }
        public void HorizontalAligning(Rect finalRect)
        {
            float hLocation = 0;
            switch (HorizontalAlign)
            {
                case EHorizontalAlignment.Left:
                    {
                        hLocation = 0;
                    }
                    break;
                case EHorizontalAlignment.Center:
                    {
                        hLocation = (finalRect.Width - Size.Width) / 2;
                    }
                    break;
                case EHorizontalAlignment.Right:
                    {
                        hLocation = finalRect.Width - Size.Width;
                    }
                    break;
                default:
                    break;
            }
            Location = new Vector2(hLocation + finalRect.X, Location.Y) ;
        }
        public void VerticalAligning(Rect finalRect)
        {
            float vLocation = 0;
            switch (VerticalAlign)
            {
                case EVerticalAlignment.Top:
                    {
                        vLocation = 0;
                    }
                    break;
                case EVerticalAlignment.Center:
                    {
                        vLocation = (finalRect.Height - Size.Height) / 2;
                    }
                    break;
                case EVerticalAlignment.Bottom:
                    {
                        vLocation = (finalRect.Height - Size.Height);
                    }
                    break;
                default:
                    break;
            }
            Location = new Vector2(Location.X, vLocation + finalRect.Y);
        }
        #endregion ILayoutable

    }
    public class TtGraphElementRender_TextBlock : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            var textBlock = renderableElement as TtGraphElement_TextBlock;
            var cmd = ImGuiAPI.GetWindowDrawList();
            var start = context.ViewPortTransform(textBlock.AbsLocation);
            var end = context.ViewPortTransform(textBlock.AbsLocation + new Vector2(textBlock.Size.Width, textBlock.Size.Height));
            //cmd.AddRectFilled(start, end, ImGuiAPI.ColorConvertFloat4ToU32(textBlock.Style.BackgroundColor), 0, ImDrawFlags_.ImDrawFlags_RoundCornersNone);
            var oldScale = ImGuiAPI.GetFont().Scale;
            var font = ImGuiAPI.GetFont();
            font.Scale = textBlock.FontScale;
            ImGuiAPI.PushFont(font);
            cmd.AddText(start, textBlock.TextColor, textBlock.Content, null);
            font.Scale = oldScale;
            ImGuiAPI.PopFont();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public interface IToolbarItem : IUIProxyBase
    {
        public float NextItemOffset { get; }
        public float NextItemSpacing { get; }
    }

    public class Toolbar : IUIProxyBase
    {
        public List<IToolbarItem> ToolbarItems = new List<IToolbarItem>();
        public float ToolbarHeight
        {
            get => StyleConfig.Instance.ToolbarHeight;
        }

        public void AddToolbarItems(params IToolbarItem[] items)
        {
            ToolbarItems.AddRange(items);
        }
        public void RemoveToolbarItem(IToolbarItem item)
        {
            ToolbarItems.Remove(item);
        }

        public bool OnDraw(ref ImDrawList drawList)
        {
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var windowWidth = ImGuiAPI.GetWindowWidth();
            var rectMax = cursorPos + new Vector2(windowWidth, EGui.UIProxy.StyleConfig.Instance.ToolbarHeight);
            drawList.AddRectFilled(ref cursorPos, ref rectMax, EGui.UIProxy.StyleConfig.Instance.ToolbarBG, 0.0f, ImDrawFlags_.ImDrawFlags_None);
            float itemOffset = 0;
            float itemSpacing = -1;
            ImGuiAPI.BeginGroup();
            for (int i = 0; i < ToolbarItems.Count; ++i)
            {
                ImGuiAPI.SameLine(itemOffset, itemSpacing);
                //ImGuiAPI.SameLine(0, -1);
                ToolbarItems[i].OnDraw(ref drawList);
                itemOffset = ToolbarItems[i].NextItemOffset;
                itemSpacing = ToolbarItems[i].NextItemSpacing;
            }
            ImGuiAPI.EndGroup();
            return true;
        }
    }

    public class ToolbarIconButtonProxy : IToolbarItem
    {
        public string Name = "Unkonw";
        public ImageProxy Icon = null;
        public Action Action = null;

        public float NextItemOffset => 0;
        public float NextItemSpacing => -1;

        bool isMouseDown = false;
        bool isMouseHover = false;

        public bool OnDraw(ref ImDrawList drawList)
        {
            bool retValue = false;
            ImGuiAPI.BeginGroup(); 
            var cursorScrPos = ImGuiAPI.GetCursorScreenPos();
            var tempScrPos = cursorScrPos;
            var clickDelta = 0.0f;
            if(isMouseDown)
            {
                clickDelta = 2.0f * ImGuiAPI.GetWindowDpiScale();
            }

            Vector2 hitRectMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 hitRectMax = new Vector2(float.MinValue, float.MinValue); ;
            if(Icon != null)
            {
                tempScrPos.Y = cursorScrPos.Y + (StyleConfig.Instance.ToolbarHeight - Icon.ImageSize.Y) * 0.5f + clickDelta;
                hitRectMin.X = cursorScrPos.X;
                hitRectMin.Y = tempScrPos.Y;
                Icon.OnDraw(ref drawList, ref tempScrPos);

                tempScrPos.X = cursorScrPos.X + Icon.ImageSize.X + StyleConfig.Instance.ToolbarButtonIconTextSpacing;
                hitRectMax.X = tempScrPos.X;
                hitRectMax.Y = tempScrPos.Y + Icon.ImageSize.Y;
            }

            var textSize = ImGuiAPI.CalcTextSize(Name, false, -1);
            tempScrPos.Y = cursorScrPos.Y + (StyleConfig.Instance.ToolbarHeight - textSize.Y) * 0.5f + clickDelta;
            ImGuiAPI.SetCursorScreenPos(ref tempScrPos);
            hitRectMin.X = System.Math.Min(hitRectMin.X, tempScrPos.X);
            hitRectMin.Y = System.Math.Min(hitRectMin.Y, tempScrPos.Y);
            hitRectMax.X = System.Math.Max(hitRectMax.X, tempScrPos.X + textSize.X);
            hitRectMax.Y = System.Math.Max(hitRectMax.Y, tempScrPos.Y + textSize.Y);
            if(isMouseDown)
            {
                var pressColor = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor_Press);
                ImGuiAPI.TextColored(ref pressColor, Name);
                if(Icon != null)
                    Icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(ref pressColor);
            }
            else if(isMouseHover)
            {
                var hoverColor = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor_Hover);
                ImGuiAPI.TextColored(ref hoverColor, Name);
                if(Icon != null)
                    Icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(ref hoverColor);
            }
            else
            {
                var textColor = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor);
                ImGuiAPI.TextColored(ref textColor, Name);
                if (Icon != null)
                    Icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(ref textColor);
            }
            ImGuiAPI.EndGroup();
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) && isMouseHover)
            {
                isMouseDown = true;
                Action?.Invoke();
                retValue = true;
            }
            else
                isMouseDown = false;

            if (ImGuiAPI.IsMouseHoveringRect(ref hitRectMin, ref hitRectMax, true))
                isMouseHover = true;
            else
                isMouseHover = false;

            return retValue;
        }
    }

    public class ToolbarSeparator : IToolbarItem
    {
        public float NextItemOffset => 0;
        public float NextItemSpacing => StyleConfig.Instance.ToolbarSeparatorThickness + StyleConfig.Instance.ItemSpacing.X * 2;

        public bool OnDraw(ref ImDrawList drawList)
        {
            var cursorScrPos = ImGuiAPI.GetCursorScreenPos();
            cursorScrPos.X += StyleConfig.Instance.ToolbarSeparatorThickness * 0.5f;
            var maxPos = cursorScrPos + new Vector2(0, StyleConfig.Instance.ToolbarHeight);
            drawList.AddLine(ref cursorScrPos, ref maxPos, StyleConfig.Instance.SeparatorColor, StyleConfig.Instance.ToolbarSeparatorThickness);
            return true;
        }
    }
}

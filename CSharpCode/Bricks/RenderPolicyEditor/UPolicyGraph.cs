﻿using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.RenderPolicyEditor
{
    public partial class UPolicyNode : UNodeBase
    {
        public UPolicyNode()
        {
            this.Icon.Size = new Vector2(20, 20);
        }
        Graphics.Pipeline.Common.URenderGraphNode mGraphNode;
        [Rtti.Meta(Order = 1)]
        public Graphics.Pipeline.Common.URenderGraphNode GraphNode 
        {
            get => mGraphNode;
        }
        [Rtti.Meta(Order = 0)]
        public string GraphNodeTypeString
        {
            get
            {
                if (GraphNode == null)
                    return "";
                return Rtti.UTypeDesc.TypeOf(GraphNode.GetType()).TypeString;
            }
            set
            {
                var typeDesc = Rtti.UTypeDesc.TypeOf(value);
                if (typeDesc != null)
                {
                    var rgNode = Rtti.UTypeDescManager.CreateInstance(typeDesc) as Graphics.Pipeline.Common.URenderGraphNode;
                    rgNode.InitNodePins();
                    InitNode(rgNode);
                }
            }
        }
        [Rtti.Meta]
        public override string Name
        {
            get
            {
                if (GraphNode != null)
                {
                    return GraphNode.Name;
                }
                return base.Name;
            }
            set
            {
                base.Name = value;
                if (GraphNode != null)
                {
                    GraphNode.Name = value;
                }
            }
        }
        
        public void InitNode(Graphics.Pipeline.Common.URenderGraphNode node)
        {
            mGraphNode = node;
            
            Inputs.Clear();
            for (int i = 0; i < GraphNode.NumOfInput; i++)
            {
                var pin = GraphNode.GetInput(i);

                var iPin = new PinIn();
                iPin.Name = pin.Name;
                iPin.Link = NewInOutPinDesc("GraphNode");
                AddPinIn(iPin);
            }
            Outputs.Clear();
            for (int i = 0; i < GraphNode.NumOfOutput; i++)
            {
                var pin = GraphNode.GetOutput(i);

                var oPin = new PinOut();
                oPin.Name = pin.Name;
                oPin.Link = NewInOutPinDesc("GraphNode");
                AddPinOut(oPin);
            }
        }
        private LinkDesc NewInOutPinDesc(string linkType = "Value")
        {
            var result = new LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            var graph = this.ParentGraph as UPolicyGraph;

            if (graph != null && graph.PolicyEditor != null)
            {
                graph.PolicyEditor.NodePropGrid.Target = this;
            }
        }
    }
    public class UPolicyGraph : UNodeGraph
    {
        public UPolicyGraph()
        {
            PolicyClassName = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Pipeline.UDeferredPolicyBase));
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        [Rtti.Meta]
        public string PolicyClassName
        {
            get;
            set;
        }
        public UPolicyEditor PolicyEditor;
        List<Rtti.UTypeDesc> mGraphNodeTypes = null;
        public List<Rtti.UTypeDesc> GraphNodeTypes
        {
            get
            {
                if (mGraphNodeTypes == null)
                {
                    mGraphNodeTypes = new List<Rtti.UTypeDesc>();
                    foreach (var i in Rtti.UTypeDescManager.Instance.Services.Values)
                    {
                        foreach (var j in i.Types)
                        {
                            if (j.Value.SystemType.IsSubclassOf(typeof(Graphics.Pipeline.Common.URenderGraphNode)))
                            {
                                mGraphNodeTypes.Add(j.Value);
                            }
                        }
                    }
                }
                return mGraphNodeTypes;
            }
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";

            foreach (var i in GraphNodeTypes)
            {
                CanvasMenus.AddMenuItem(
                $"{i.FullName}", null,
                (UMenuItem item, object sender) =>
                {
                    var node = new UPolicyNode();
                    var rgNode = Rtti.UTypeDescManager.CreateInstance(i) as Graphics.Pipeline.Common.URenderGraphNode;
                    //rgNode.RenderGraph = this;
                    rgNode.InitNodePins();
                    node.InitNode(rgNode);
                    node.Name = rgNode.Name;
                    node.Position = PopMenuPosition;
                    this.AddNode(node);
                });
            }   
        }
    }
}

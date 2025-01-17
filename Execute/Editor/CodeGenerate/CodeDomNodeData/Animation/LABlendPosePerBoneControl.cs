﻿using CodeGenerateSystem.Base;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.AnimStateMachine;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CodeDomNode.Animation
{
    public class LABlendPosePerBoneControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public float Alpha { get; set; } = 1.0f;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "BlendPosePerBone";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LABlendPosePerBoneControlConstructionParams;

            retVal.Alpha = Alpha;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LABlendPosePerBoneControlConstructionParams))]
    public partial class LABlendPosePerBoneControl : CodeGenerateSystem.Base.BaseNodeControl
    {
     
        public List<string> Notifies = new List<string>();

        partial void InitConstruction();

        #region DP

        public float Alpha
        {
            get { return (float)GetValue(AlphaProperty); }
            set
            {
                SetValue(AlphaProperty, value);
                var para = CSParam as LABlendPosePerBoneControlConstructionParams;
                para.Alpha = value;
            }
        }
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(float), typeof(LABlendPosePerBoneControl), new UIPropertyMetadata(0.0f, OnAlphaPropertyyChanged));
        private static void OnAlphaPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LABlendPosePerBoneControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Alpha = (float)e.NewValue;
        }

        #endregion

        #region ShowProperty
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Alpha", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Alpha", LABlendPosePerBoneControl.AlphaProperty, Alpha);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LABlendPosePerBoneControl(LABlendPosePerBoneControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Alpha = csParam.Alpha;
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);
        }
        #region AddDeleteValueLink
        private void AlphaValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            AlphaValueTextBlock.Visibility = Visibility.Visible;
        }

        private void AlphaValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            AlphaValueTextBlock.Visibility = Visibility.Collapsed;
        }
        #endregion AddDeleteValueLink
        #region InitializeLinkControl
        CodeGenerateSystem.Base.LinkPinControl mSrcPoseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mDescPoseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mAlphaValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        void InitializeLinkControl(LABlendPosePerBoneControlConstructionParams csParam)
        {
            mSrcPoseLinkHandle = SrcPoseHandle;
            mDescPoseLinkHandle = DescPoseHandle;
            mAlphaValueLinkHandle = AlphaValueHandle;
            mOutLinkHandle = OutPoseHandle;
            mSrcPoseLinkHandle.MultiLink = false;
            mDescPoseLinkHandle.MultiLink = false;
            mAlphaValueLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;

            mSrcPoseLinkHandle.NameStringVisible = Visibility.Visible;
            mSrcPoseLinkHandle.NameString = "SourceePose";
            mDescPoseLinkHandle.NameStringVisible = Visibility.Visible;
            mDescPoseLinkHandle.NameString = "DestinationPose";
            //mAdditiveLinkHandle.NameStringVisible = Visibility.Visible;
            //mAdditiveLinkHandle.NameString = "AdditivePose";
            mAlphaValueLinkHandle.NameStringVisible = Visibility.Visible;
            mAlphaValueLinkHandle.NameString = "Alpha";
            AlphaValueTextBlock.Visibility = Visibility.Visible;
            mAlphaValueLinkHandle.OnAddLinkInfo += AlphaValueLinkHandle_OnAddLinkInfo;
            mAlphaValueLinkHandle.OnDelLinkInfo += AlphaValueLinkHandle_OnDelLinkInfo;
            AddLinkPinInfo("SrcPoseLinkHandle", mSrcPoseLinkHandle, null);
            AddLinkPinInfo("DescPoseLinkHandle", mDescPoseLinkHandle, null);
            AddLinkPinInfo("AlphaValueLinkHandle", mAlphaValueLinkHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        #endregion InitializeLinkControl
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "SrcPoseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "DescPoseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "AlphaValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LABlendPosePerBoneControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAClipNodeControl);
        }
        public string ValidName
        {
            get { return StringRegex.GetValidName(NodeName + "_" + Id.ToString()); }
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeVariableReferenceExpression(ValidName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var srcLinkObj = mSrcPoseLinkHandle.GetLinkedObject(0, true);
            var srcLinkElm = mSrcPoseLinkHandle.GetLinkedPinControl(0, true);
            await srcLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, srcLinkElm, context);
            var desLinkObj = mDescPoseLinkHandle.GetLinkedObject(0, true);
            var desLinkElm = mDescPoseLinkHandle.GetLinkedPinControl(0, true);
            await desLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, desLinkElm, context);
            return;
        }
    }
}

﻿using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls
{
    [EngineNS.Rtti.MetaClass]
    public class FinalTextureControlConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// FinalTexture.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.CustomConstructionParams(typeof(FinalTextureControlConstructionParams))]
    [CodeGenerateSystem.ShowInNodeList("Params/CaptureTexture", "获得屏幕数据")]
    public partial class FinalTextureControl : BaseNodeControl_ShaderVar
    {
        public FinalTextureControl(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            IsInConstantBuffer = false;

            AddLinkPinInfo("TextureLink", TextureLink, TextureLink.BackBrush);
            AddLinkPinInfo("UVLink_2D", UVLink_2D, null);
            AddLinkPinInfo("Tex2DLink", Tex2DLink, Tex2DLink.BackBrush);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TextureLink", enLinkType.Texture, enBezierType.Right, enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "UVLink_2D", enLinkType.Float2, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "Tex2DLink", enLinkType.Float4, enBezierType.Right, enLinkOpType.Start, true);
        }

        /*/////////////////////////////////////////////////////////////////////////
        D3DTEXF_NONE            = 0,    // filtering disabled (valid for mip filter only)
        D3DTEXF_POINT           = 1,    // nearest
        D3DTEXF_LINEAR          = 2,    // linear interpolation
        D3DTEXF_ANISOTROPIC     = 3,    // anisotropic
        D3DTEXF_PYRAMIDALQUAD   = 6,    // 4-sample tent
        D3DTEXF_GAUSSIANQUAD    = 7,    // 4-sample gaussian
        /////////////////////////////////////////////////////////////////////////*/

        public override string GetValueDefine()
        {
            string retStr = "";

            retStr += "sampler2D " + GetTextureSampName() + " = sampler_state\r\n";
            //retStr += "{\r\n";
            //retStr += "\tTexture = <" + "g_PreFrameBuffer" + ">;\r\n";
            //retStr += "\tMipFilter = " + ((ComboBoxItem)MipFilterComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tMinFilter = " + ((ComboBoxItem)MinFilterComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tMagFilter = " + ((ComboBoxItem)MagFilterComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tAddressU = " + ((ComboBoxItem)AddressUComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tAddressV = " + ((ComboBoxItem)AddressVComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "\tSRGBTexture = " + ((ComboBoxItem)SRGBTextureComboBox.SelectedItem).Content + ";\r\n";
            //retStr += "};\r\n\r\n";

            return retStr;
        }

        public string GetTextureSampName()
        {
            return "Samp_FinalTexture";
        }
        public string GetTextureName()
        {
            return "FinalTexture";
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == TextureLink)
                return "sampler2D";
            else if (element == Tex2DLink)
                return "float4";

            return base.GCode_GetTypeString(element, context);
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == TextureLink)
                return GetTextureSampName();
            else if (element == Tex2DLink)
            {
                string uvName = "input.vUV";
                if (UVLink_2D.HasLink)
                {
                    uvName = UVLink_2D.GetLinkedObject(0, true).GCode_GetValueName(UVLink_2D.GetLinkedPinControl(0, true), context);
                }

                return "vise_tex2D(" + GetTextureName() + "," + GetTextureSampName() + "," + uvName + ")";
            }

            return base.GCode_GetValueName(element, context);
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (UVLink_2D.HasLink)
                UVLink_2D.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            IsDirty = true;
        }
    }
}

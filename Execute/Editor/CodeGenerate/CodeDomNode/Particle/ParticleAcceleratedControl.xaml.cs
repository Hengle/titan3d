﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Data;
using System.Windows.Input;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode.Particle
{
    [CodeGenerateSystem.ShowInNodeList("测试/加速度(Particle Accelerated)", "加速度Particle Accelerated")]
    public partial class ParticleAcceleratedControl
    {
        public ParticleAcceleratedControl()
        {
            InitConstruction();
        }
        partial void InitConstruction()
        {
            InitializeComponent();
            UITitlePanel.Host = this;
            mCtrlValueLinkHandleDown = StructLinkHandleDown;
            mCtrlValueLinkHandleDown.MultiLink = false;
            mCtrlValueLinkHandleUp = StructLinkHandleUp;
            mCtrlValueLinkHandleUp.MultiLink = false;
            BindingOperations.SetBinding(UITitlePanel, TitlePanel.NodeNameProperty, new Binding("NodeName") { Source = this });
        }

        private void UIMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //TitleLabel.Visibility = System.Windows.Visibility.Collapsed;
            //TitleBox.Visibility = System.Windows.Visibility.Visible;
            //TitleBox.Focus();
            e.Handled = true;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var grid = Template.FindName("PART_HeaderGrid", this) as System.Windows.Controls.Grid;
            grid.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 77, 107));//80,117,159
        }

        public override CodeGenerateSystem.Base.BaseNodeControl Duplicate(DuplicateParam param)
        {
            var node = base.Duplicate(param) as ParticleAcceleratedControl;
            node.DataGradient = DataGradient.Duplicate();
            return node;
        }
    }
}

﻿using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Camera
{
    public class UCameraSpringArm : ULightWeightNodeBase, ICameraControlNode
    {
        public class UCameraSpringArmData : UNodeData
        {
            [Rtti.Meta]
            public DVector3 TargetOffset { get; set; } = DVector3.Zero;
            [Rtti.Meta]
            public float ArmLength { get; set; } = 3.0f;
            [Rtti.Meta]
            public float ProbeSize
            {
                get;
                set;
            } = 0.01f;
            [Rtti.Meta]
            public float SpringDamping { get; set; } = 0.2f;
            [Rtti.Meta]
            public bool DoCollisionTest { get; set; } = true;
        }

        public DVector3 TargetOffset
        {
            get => (NodeData as UCameraSpringArmData).TargetOffset;
        }

        public float ArmLength
        {
            get => (NodeData as UCameraSpringArmData).ArmLength;
        }

        public float ProbeSize
        {
            get => (NodeData as UCameraSpringArmData).ArmLength;
        }
        public float SpringDamping
        {
            get => (NodeData as UCameraSpringArmData).SpringDamping;
        }
        public bool DoCollisionTest
        {
            get => (NodeData as UCameraSpringArmData).DoCollisionTest;
        }

        public UCamera Camera
        {
            get
            {
                if (Children.Count == 0)
                    return null;
                //for now just allow a camera in children
                System.Diagnostics.Debug.Assert(Children.Count == 1);
                return Children[0] as UCamera;
            }
        }

        public override bool OnTickLogic(UWorld world, URenderPolicy policy)
        {
            Placement.Position  = Placement.Quat * DVector3.Backward * ArmLength  + TargetOffset;
            return base.OnTickLogic(world, policy);
        }
        #region ICameraControlNode
        public void AddDelta(Vector3 delta)
        {
            Placement.Quat = Placement.Quat * Quaternion.FromEuler(new Vector3(delta.X * 0.01f, delta.Y * 0.01f, 0)) ;
        }
        public void AddYaw(float delta)
        {
            //Placement.Quat = Quaternion.FromEuler(new Vector3(0, delta * 0.01f, 0)) * Placement.Quat;
        }

        public void AddPitch(float delta)
        {
           Placement.Quat = Quaternion.FromEuler(new Vector3(delta * 0.01f, 0, 0)) * Placement.Quat;
        }

        public void AddRoll(float delta)
        {
            throw new NotImplementedException();
        }
        #endregion ICameraControlNode
    }
}

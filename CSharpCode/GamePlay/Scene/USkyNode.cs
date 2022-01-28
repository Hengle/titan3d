﻿using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public class USkyNode : UMeshNode
    {
        public override bool OnTickLogic(UWorld world, URenderPolicy policy)
        {
            var camPos = policy.DefaultCamera.mCoreObject.GetPosition();
            camPos = new DVector3(camPos.X, Placement.TransformRef.mPosition.Y, camPos.Z);
            if (Placement.Position != camPos)
            {
                Placement.Position = camPos;
            }
            return base.OnTickLogic(world, policy);
        }
    }
}

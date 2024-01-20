﻿using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue1<Mesh.Modifier.CStaticModifier>
    {
        
    }

    public class UMdfInstanceStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue2<Modifier.TtInstanceModifier, Mesh.Modifier.CStaticModifier>
    {
        public UMdfInstanceStaticMesh()
        {
            InstanceModifier.SetMode(true);
            UpdateShaderCode();
        }
        public Modifier.TtInstanceModifier InstanceModifier
        {
            get => this.Modifiers[0] as Modifier.TtInstanceModifier;
        }
        public void SetInstantMode(bool bSSBO = true)
        {
            InstanceModifier.SetMode(bSSBO);
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, atom);
            InstanceModifier?.OnDrawCall(cmd, shadingType, drawcall, policy, atom);
        }
    }
}

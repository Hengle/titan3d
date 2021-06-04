﻿using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Base
{
    public interface IAnimatableClassDesc : IO.ISerializer
    {
        public Rtti.UTypeDesc ClassType { get; }
        public VNameString TypeStr { get; set; }
        public VNameString Name { get; set; }
        public List<IAnimatablePropertyDesc> Properties { get;}
    }
    public interface IAnimatablePropertyDesc : IO.ISerializer
    {
        public Rtti.UTypeDesc ClassType { get; }
        public VNameString TypeStr { get; set; }
        public VNameString Name { get; set; }
        public int CurveIndex { get; set; }
    }
    public class AnimHierarchy : IO.BaseSerializer
    {
        [Rtti.Meta]
        public IAnimatableClassDesc Value { get; set; }
        [Rtti.Meta]
        public AnimHierarchy Root { get; set; }
        [Rtti.Meta]
        public AnimHierarchy Parent { get; set; }
        [Rtti.Meta]
        public List<AnimHierarchy> Children { get; set; } = new List<AnimHierarchy>();

    }
}

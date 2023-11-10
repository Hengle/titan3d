﻿using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Collections.Specialized;
using EngineNS.DesignMacross.TimedStateMachine;
using EngineNS.DesignMacross.Base.Description;

namespace EngineNS.DesignMacross.Design
{
    public class TtClassDescription : IClassDescription
    {
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "Class";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public string ClassName { get => Name; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public UNamespaceDeclaration Namespace { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public bool IsStruct { get; set; } = false;
        [Rtti.Meta]
        public List<string> SupperClassNames { get; set; } = new List<string>();
        [OutlineElement_List(typeof(TtOutlineElementsList_Variables))]
        [Rtti.Meta]
        public List<IVariableDescription> Variables { get; set; } = new List<IVariableDescription>();
        [Rtti.Meta]
        [OutlineElement_List(typeof(TtOutlineElementsList_Methods))]
        public List<IMethodDescription> Methods { get; set; } = new List<IMethodDescription>();
        [OutlineElement_List(typeof(TtOutlineElementsList_DesignableVariables))]
        [Rtti.Meta]
        public List<IDesignableVariableDescription> DesignableVariables { get; set; } = new List<IDesignableVariableDescription>();
        public IDescription Parent { get; set; }

        public List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            List<UClassDeclaration> classDeclarationsBuilded = new();
            UClassDeclaration thisClassDeclaration = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(this, ref classBuildContext);

            foreach (var designVarDesc in DesignableVariables)
            {
                classDeclarationsBuilded.AddRange(designVarDesc.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(designVarDesc.BuildVariableDeclaration(ref classBuildContext));
            }
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
            
        }
        #endregion ISerializer
    }
}
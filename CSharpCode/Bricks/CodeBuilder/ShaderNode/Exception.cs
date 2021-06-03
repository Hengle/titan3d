﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public class GraphException : Exception
    {
        public IBaseNode ErrorNode;
        public EGui.Controls.NodeGraph.NodePin ErrorPin;
        public string ErrorPinName
        {
            get
            {
                if (ErrorPin != null)
                    return ErrorPin.Name;
                return "";
            }
        }
        public string ErrorInfo { get; set; }
        public GraphException(IBaseNode node, EGui.Controls.NodeGraph.NodePin pin, string info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            ErrorNode = node;
            ErrorPin = pin;
            ErrorInfo = $"{sourceFilePath}:{sourceLineNumber}->{memberName}->{info}";
        }
    }

}

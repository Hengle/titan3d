﻿using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class GraphException : Exception
    {
        public INodeExpr ErrorNode;
        public NodePin ErrorPin;
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
        public GraphException(INodeExpr node, NodePin pin, string info,
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

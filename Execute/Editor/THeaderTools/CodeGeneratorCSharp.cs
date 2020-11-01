﻿using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public struct IVertexBuffer_Ptr
    {
        public struct PtrType
        {
            public IntPtr NativePointer;
        }
        private PtrType mPtr;
        public PtrType Ptr { get => mPtr; }
    }
    partial class CodeGenerator
    {
        public void GenCodeCSharp(string targetDir)
        {
            foreach (var i in ClassCollector)
            {
                string genCode = "//This cs is generated by THT.exe\n";

                genCode += "\n\n\n";

                var ns = i.GetNameSpace();
                if (ns == null)
                    ns = "EngineNS";

                genCode += $"namespace {ns}\n";
                genCode += "{\n";
                genCode += GenCppReflectionCSharp(i);
                genCode += "}\n";

                var file = targetDir + i.GetGenFileNameCSharp();
                System.IO.File.WriteAllText(file, genCode); ;
            }
        }
        public string GenCppReflectionCSharp(CppClass klass)
        {
            int nTable = 1;
            string code = GenLine(nTable, $"public struct {klass.Name}_Ptr");
            code += GenLine(nTable, "{");
            nTable++;

            code += GenLine(nTable, "public struct PtrType");
            code += GenLine(nTable, "{");
            nTable++;
            code += GenLine(nTable, "public IntPtr NativePointer;");
            nTable--;
            code += GenLine(nTable, "}");
            code += GenLine(nTable, "private PtrType mPtr;");
            code += GenLine(nTable, "public PtrType Ptr { get => mPtr; }");

            if (klass.Methods.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Methods)
                {
                    code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, i.GenPInvokeBindingCSharp(klass));
                }
                code += "\n";
            }

            nTable--;
            code += GenLine(nTable, "}");//for struct klass.Name
            return code;
        }
        private string GenLine(int nTable, string content)
        {
            string codeline = "";
            for(int i=0; i<nTable; i++)
            {
                codeline += "\t";
            }
            codeline += content;
            codeline += "\n";
            return codeline;
        }
    }
}

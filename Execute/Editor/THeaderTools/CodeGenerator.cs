﻿using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CodeGenerator
    {
        public class Symbol
        {
            public const string BeginRtti = "StructBegin";
            public const string EndRtti = "StructEnd";
            public const string DefMember = "StructMember";
            public const string DefMethod = "StructMethodEx";
            public const string DefConstructor = "StructConstructor";

            public const string AppendClassMeta = "AddClassMetaInfo";
            public const string AppendMemberMeta = "AppendMemberMetaInfo";
            public const string AppendMethodMeta = "AppendMethodMetaInfo";
            public const string AppendConstructorMeta = "AppendConstructorMetaInfo";
        }
        public List<CppClass> ClassCollector = new List<CppClass>();
        public CppClass FindClass(string name)
        {
            foreach(var i in ClassCollector)
            {
                if(i.Name == name)
                {
                    return i;
                }
            }
            return null;
        }
        private void CheckValid()
        {
            foreach (var i in ClassCollector)
            {
                i.CheckValid(this);
            }
        }
        public void GenCode(string targetDir)
        {
            CheckValid();
            foreach (var i in ClassCollector)
            {
                string genCode = "//This file is generated by THT.exe\n";

                genCode += $"#include \"{i.HeaderSource}\"\n";

                genCode += "\n\n\n";

                genCode += GenCppReflection(i);

                var file = targetDir + i.GetGenFileName();
                System.IO.File.WriteAllText(file, genCode); ;
            }
        }
        public string GenCppReflection(CppClass klass)
        {
            string code = "";
            var ns = klass.GetNameSpace();
            if (ns == null)
                ns = "EngineNS";
            else
                ns = ns.Replace(".", "::");
            code += $"{Symbol.BeginRtti}({klass.Name},{ns})\n";
            WriteMetaCode(ref code, klass, Symbol.AppendClassMeta);

            if (klass.Members.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Members)
                {
                    code += $"\t{Symbol.DefMember}({i.Name});\n";
                    WriteMetaCode(ref code, i, Symbol.AppendMemberMeta);
                }
                code += "\n";
            }

            if (klass.Methods.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Methods)
                {
                    var returnConverter = i.GetReturnConverter();
                    if (returnConverter == null)
                        returnConverter = i.ReturnType;
                    code += $"\t{Symbol.DefMethod}{i.Arguments.Count}({i.Name}, {returnConverter}";
                    foreach (var j in i.Arguments)
                    {
                        code += $", {j.Key}, {j.Value}";
                    }
                    code += $");\n";
                    WriteMetaCode(ref code, i, Symbol.AppendMethodMeta);
                }
                code += "\n";
            }

            if (klass.Constructors.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Constructors)
                {
                    code += $"\t{Symbol.DefConstructor}{i.Arguments.Count}(";
                    int argIndex = 0;
                    foreach (var j in i.Arguments)
                    {
                        if (argIndex == 0)
                            code += $"{j.Key}";
                        else
                            code += $", {j.Key}";
                    }
                    code += $");\n";
                    WriteMetaCode(ref code, i, Symbol.AppendConstructorMeta);
                }
                code += "\n";
            }

            string parent = klass.ParentName;
            if (parent == null)
            {
                parent = "void";
            }
            else
            {
                var pkls = FindClass(parent);
                if (pkls == null)
                {
                    Console.WriteLine($"class {klass} can't find parent");
                    parent = "void";
                }
            }
            code += $"{Symbol.EndRtti}({parent})\n";
            return code;
        }

        private void WriteMetaCode(ref string code, CppMetaBase meta, string type)
        {
            foreach(var i in meta.MetaInfos)
            {
                code += $"\t{type}({i.Key} , {i.Value});\n";
            }
        }
    }
}

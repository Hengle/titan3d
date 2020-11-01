﻿using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public partial class CodeGenerator
    {
        public static CodeGenerator Instance = new CodeGenerator();
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
        private CodeGenerator()
        {
            Cpp2CSTypes["void"] = "void";
            Cpp2CSTypes["char"] = "char";
            Cpp2CSTypes["unsigned char"] = "byte";
            Cpp2CSTypes["short"] = "short";
            Cpp2CSTypes["unsigned short"] = "ushort";
            Cpp2CSTypes["int"] = "int";
            Cpp2CSTypes["unsigned int"] = "uint";
            Cpp2CSTypes["long"] = "int";
            Cpp2CSTypes["unsigned long"] = "uint";
            Cpp2CSTypes["long long"] = "long";
            Cpp2CSTypes["unsigned long long"] = "ulong";
            Cpp2CSTypes["float"] = "float";
            Cpp2CSTypes["double"] = "double";
            //Cpp2CSTypes["std::string"] = "string";
            Cpp2CSTypes["BYTE"] = "byte";
            Cpp2CSTypes["WORD"] = "ushort";
            Cpp2CSTypes["DWORD"] = "uint";
            Cpp2CSTypes["QWORD"] = "uint64";
            Cpp2CSTypes["SHORT"] = "short";
            Cpp2CSTypes["USHORT"] = "ushort";
            Cpp2CSTypes["INT"] = "int";
            Cpp2CSTypes["UINT"] = "uint";
            Cpp2CSTypes["INT64"] = "int64";
            Cpp2CSTypes["UINT64"] = "uint64";
        }
        public void Reset()
        {
            ClassCollector.Clear();
        }
        Dictionary<string, string> Cpp2CSTypes = new Dictionary<string, string>();
        public string CppTypeToCSType(string type)
        {
            string result;
            if (Cpp2CSTypes.TryGetValue(type, out result))
                return result;

            if(type.EndsWith("*"))
            {
                type = CppClass.RemovePtrAndRef(type);
                return type + "_Ptr.PtrType";
            }
            else
            {
                return type;
            }
        }
        public List<CppClass> ClassCollector = new List<CppClass>();
        public CppClass FindClass(string fullName)
        {
            foreach(var i in ClassCollector)
            {
                if(i.Name == fullName)
                {
                    return i;
                }
            }
            return null;
        }
        public CppClass MatchClass(string name, string[] ns)
        {
            var klass = FindClass(name);
            if (klass != null)
                return klass;
            klass = FindClass("EngineNS." + name);
            if (klass != null)
                return klass;
            if (ns == null)
                return null;
            foreach (var i in ns)
            {
                var fullName = i + "." + name;
                klass = FindClass(fullName);
                if (klass != null)
                    return klass;
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
                string genCode = "//This cpp is generated by THT.exe\n";

                genCode += $"#include \"{i.HeaderSource}\"\n";

                genCode += "\n\n\n";

                var usingNS = i.GetUsingNS();

                genCode += "using EngineNS;\n";
                if (usingNS != null)
                {
                    var segs = usingNS.Split('&');
                    foreach(var j in segs)
                    {
                        genCode += $"using {j.Replace(".","::")};\n";
                    }
                }
                genCode += "\n";

                genCode += GenCppReflection(i);

                genCode += "\n\n\n";
                genCode += GenPInvokeBinding(i);

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
                    if (i.Arguments.Count > 0)
                        code += ", ";
                    code += i.GetParameterString();
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
        public string GenPInvokeBinding(CppClass klass)
        {
            string code = "";
            foreach(var i in klass.Methods)
            {
                code += i.GenPInvokeBinding(klass);
                code += "\n";
            }
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

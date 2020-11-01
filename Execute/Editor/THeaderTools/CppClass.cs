﻿using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public enum EStructType
    {
        Struct,
        Class,
    }
    public enum EVisitMode
    {
        Public,
        Protected,
        Private,
    }
    public class CppMetaBase
    {
        public Dictionary<string, string> MetaInfos
        {
            get;
        } = new Dictionary<string, string>();
        public void AnalyzeMetaString(string klsMeta)
        {
            MetaInfos.Clear();
            var segs = klsMeta.Split(',');
            foreach (var i in segs)
            {
                var pair = i.Split('=');
                if (pair.Length == 2)
                {
                    MetaInfos.Add(pair[0].Trim(), pair[1].Trim());
                }
            }
        }
        public string GetMetaValue(string name)
        {
            string result;
            if (MetaInfos.TryGetValue(name, out result))
                return result;
            return null;
        }
        public class Symbol
        {
            public const string SV_NameSpace = "SV_NameSpace";
            public const string SV_ReturnConverter = "SV_ReturenConverter";
            public const string SV_UsingNS = "SV_UsingNS";
        }
        
        public string GetReturnConverter()
        {
            return this.GetMetaValue(Symbol.SV_ReturnConverter);
        }
    }

    public class CppClass : CppMetaBase
    {
        public override string ToString()
        {
            return $"{GetNameSpace()}.{Name} : {ParentName}";
        }
        public string HeaderSource
        {
            get;
            set;
        }
        public EStructType StructType
        {
            get;
            set;
        } = EStructType.Class;
        public string ApiName
        {
            get;
            set;
        } = null;
        public string Name
        {
            get;
            set;
        }
        public EVisitMode InheritMode
        {
            get;
            set;
        } = EVisitMode.Public;
        public string ParentName
        {
            get;
            set;
        }
        public List<CppFunction> Methods
        {
            get;
        } = new List<CppFunction>();
        public List<CppMember> Members
        {
            get;
        } = new List<CppMember>();
        public List<CppConstructor> Constructors
        {
            get;
        } = new List<CppConstructor>();
        public string GetFullName(bool asCpp)
        {
            var ns = GetNameSpace();
            string fullName;
            if (ns == null)
                return Name;
            else
                fullName = ns + "." + this.Name;
            if (asCpp)
            {
                return fullName.Replace(".", "::");
            }
            else
            {
                return fullName;
            }
        }
        public string GetNameSpace()
        {
            return this.GetMetaValue(Symbol.SV_NameSpace);
        }
        public string GetUsingNS()
        {
            return this.GetMetaValue(Symbol.SV_UsingNS);
        }
        public string GetGenFileName()
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return Name + ".gen.cpp";
            else
                return ns + "." + Name + ".gen.cpp";
        }
        public string GetGenFileNameCSharp()
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return Name + ".gen.cs";
            else
                return ns + "." + Name + ".gen.cs";
        }
        public static bool IsSystemType(string name)
        {
            switch (name)
            {
                case "void":
                case "char":
                case "unsigned char":
                case "short":
                case "unsigned short":
                case "int":
                case "unsigned int":
                case "long":
                case "unsigned long":
                case "long long":
                case "unsigned long long":
                case "float":
                case "double":
                case "std::string":
                case "BYTE":
                case "WORD":
                case "DWORD":
                case "QWORD":
                case "SHORT":
                case "USHORT":
                case "INT":
                case "UINT":
                case "INT64":
                case "UINT64":
                    return true;
                default:
                    return false;
            }
        }
        public static string RemovePtrAndRef(string name)
        {
            int i = name.Length - 1;
            for (; i >= 0; i--)
            {
                if (name[i] != '*' && name[i] != '&')
                {
                    break;
                }
            }
            if (i == name.Length - 1)
                return name;
            else
                return name.Substring(0, i+1);
        }
        public void CheckValid(CodeGenerator manager)
        {
            var usingNS = this.GetUsingNS();
            string[] segs = null;
            if (usingNS!=null)
                segs = usingNS.Split('&');

            foreach (var i in Members)
            {
                var realType = RemovePtrAndRef(i.Type);
                var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                if (klass != null)
                {
                    i.Type = klass.GetFullName(false);
                }
                if (IsSystemType(realType))
                    continue;
                else if (manager.FindClass(realType)!=null)
                    continue;
                else
                {
                    Console.WriteLine($"{realType} used by RTTI member({i.Name}) in {this.ToString()}, Please Reflect this class");
                }
            }

            foreach (var i in Methods)
            {
                var realType = RemovePtrAndRef(i.ReturnType);
                if (!IsSystemType(realType) && manager.FindClass(realType) == null)
                {
                    Console.WriteLine($"{realType} used by RTTI Method({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                }
                foreach(var j in i.Arguments)
                {
                    realType = RemovePtrAndRef(j.Key);
                    var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                    if (klass != null)
                    {
                        j.Key = klass.GetFullName(false);
                    }
                    if (!IsSystemType(realType) && manager.FindClass(realType) == null)
                    {
                        Console.WriteLine($"{realType} used by RTTI Method({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                    }
                }
            }

            foreach (var i in Constructors)
            {
                foreach (var j in i.Arguments)
                {
                    var realType = RemovePtrAndRef(j.Key);
                    var klass = CodeGenerator.Instance.MatchClass(realType, segs);
                    if (klass != null)
                    {
                        j.Key = klass.GetFullName(false);
                    }
                    if (!IsSystemType(realType) && manager.FindClass(realType) == null)
                    {
                        Console.WriteLine($"{realType} used by RTTI Constructor({i.ToString()}) in {this.ToString()}, Please Reflect this class");
                    }
                }
            }
        }
    }
    public class CppMember : CppMetaBase
    {
        public string Type
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }
    public class CppCallParameters : CppMetaBase
    {
        public class ArgKeyValuePair
        {
            public ArgKeyValuePair(string k,string v)
            {
                Key = k;
                Value = v;
            }
            public string Key;
            public string Value;
        }
        public List<ArgKeyValuePair> Arguments
        {
            get;
        } = new List<ArgKeyValuePair>();
        public string GetParameterString()
        {
            string result = "";
            for(int i = 0; i < Arguments.Count; i++)
            {
                var cppName = Arguments[i].Key.Replace(".", "::");
                if (i==0)
                    result += $"{cppName} {Arguments[i].Value}";
                else
                    result += $", {cppName} {Arguments[i].Value}";
            }
            return result;
        }
        public string GetParameterStringCSharp()
        {
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                var type = CodeGenerator.Instance.CppTypeToCSType(Arguments[i].Key);
                if (i == 0)
                    result += $"{type} {Arguments[i].Value}";
                else
                    result += $", {type} {Arguments[i].Value}";
            }
            return result;
        }
        public string GetParameterCallString()
        {
            string result = "";
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (i == 0)
                    result += $"{Arguments[i].Value}";
                else
                    result += $", {Arguments[i].Value}";
            }
            return result;
        }
    }

    public class CppFunction : CppCallParameters
    {
        public override string ToString()
        {
            if(IsVirtual)
            {
                return $"virtual {ReturnType} {Name}({GetParameterString()})";
            }
            else
            {
                return $"{ReturnType} {Name}({GetParameterString()})";
            }
        }
        public bool IsVirtual
        {
            get;
            set;
        }
        public string ApiName
        {
            get;
            set;
        } = null;
        public string Name
        {
            get;
            set;
        }
        public string ReturnType
        {
            get;
            set;
        }
        public string GenPInvokeBinding(CppClass klass)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = $"extern \"C\" VFX_API {ReturnType} SDK_{klass.Name}_{Name}({klass.GetFullName(true)}* self{afterSelf}{this.GetParameterString()})\n";
            code += "{\n";
            code += "\tif(self==nullptr) {\n";
            code += $"\t\treturn TypeDefault({ReturnType});\n";
            code += "\t}\n";
            code += $"\treturn self->{Name}({this.GetParameterCallString()});\n";
            code += "}\n";
            return code;
        }
        public string GenPInvokeBindingCSharp(CppClass klass)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            return $"private extern static {ReturnType} SDK_{klass.Name}_{Name}(PtrType self{afterSelf}{this.GetParameterStringCSharp()})\n";
        }
    }
    public class CppConstructor : CppCallParameters
    {
        public override string ToString()
        {
            return $"Constructor({GetParameterString()})";
        }
        public string ApiName
        {
            get;
            set;
        } = null;
    }
}

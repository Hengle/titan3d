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
        public string GetNameSpace()
        {
            return this.GetMetaValue(Symbol.SV_NameSpace);
        }
        public string GetGenFileName()
        {
            var ns = this.GetMetaValue(Symbol.SV_NameSpace);
            if (ns == null)
                return Name + ".gen.cpp";
            else
                return ns + "." + Name + ".gen.cpp";
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
            foreach(var i in Members)
            {
                var realType = RemovePtrAndRef(i.Type);
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
        public List<KeyValuePair<string, string>> Arguments
        {
            get;
        } = new List<KeyValuePair<string, string>>();
        public string GetParameterString()
        {
            string result = "";
            for(int i = 0; i < Arguments.Count; i++)
            {
                if(i==0)
                    result += $"{Arguments[i].Key} {Arguments[i].Value}";
                else
                    result += $", {Arguments[i].Key} {Arguments[i].Value}";
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

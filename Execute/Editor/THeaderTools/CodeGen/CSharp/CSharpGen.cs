﻿using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools.CodeGen.CSharp
{
    public class CSharpGen : ICodeGen
    {
        public override void GenCode(string targetDir)
        {
            GenCallbackCode(targetDir, CodeGenerator.Instance.CBCollector);
            base.GenCode(targetDir);
        }
        public void MakeSharedProjectCSharp()
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(CodeGenerator.Instance.GenDirectory + "Empty_CodeGenCSharp.projitems");
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(CodeGenerator.Instance.GenDirectory, "*.cs");
            foreach (var i in allFiles)
            {
                if (NewCsFiles.Contains(i))
                    continue;
                else
                    System.IO.File.Delete(i);
            }
            allFiles = System.IO.Directory.GetFiles(CodeGenerator.Instance.GenDirectory, "*.cs");
            foreach (var i in allFiles)
            {
                var cs = myXmlDoc.CreateElement("Compile", root.NamespaceURI);
                var file = myXmlDoc.CreateAttribute("Include");
                file.Value = i;
                cs.Attributes.Append(file);
                compile.AppendChild(cs);
            }

            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, Encoding.UTF8);
            writer.Formatting = System.Xml.Formatting.Indented;
            myXmlDoc.Save(writer);
            var reader = new System.IO.StreamReader(streamXml, Encoding.UTF8);
            streamXml.Position = 0;
            var content = reader.ReadToEnd();
            reader.Close();
            streamXml.Close();

            var projFile = CodeGenerator.Instance.GenDirectory + "CodeGenCSharp.projitems";
            if (System.IO.File.Exists(projFile))
            {
                string old_code = System.IO.File.ReadAllText(projFile);
                if (content == old_code)
                    return;
            }
            System.IO.File.WriteAllText(projFile, content);
        }
        internal List<string> NewCsFiles = new List<string>();
        public string GetGenFileName(CppMetaBase container)
        {
            var ns = container.GetMetaValue(CppMetaBase.Symbol.SV_NameSpace);
            if (ns == null)
                return container.Name + ".gen.cs";
            else
                return ns + "." + container.Name + ".gen.cs";
        }
        public override void GenCallBack(string targetDir, CppCallback desc)
        {

        }
        public override void GenEnum(string targetDir, CppEnum desc)
        {
            string genCode = "//This cs is generated by THT.exe\n";

            genCode += "\n\n\n";

            var ns = desc.GetNameSpace();
            if (ns != null)
            {
                genCode += $"namespace {ns}\n";
                genCode += "{\n";
            }
            genCode += GenEnumDefine(desc);
            if (ns != null)
            {
                genCode += "}\n";
            }

            var file = targetDir + GetGenFileName(desc);
            NewCsFiles.Add(file);
            if (System.IO.File.Exists(file))
            {
                string old_code = System.IO.File.ReadAllText(file);
                if (genCode == old_code)
                    return;
            }
            System.IO.File.WriteAllText(file, genCode); ;
        }
        public override void GenClass(string targetDir, CppClass desc)
        {
            string genCode = "//This cs is generated by THT.exe\n";
            genCode += GenLine(0, "using System;");
            genCode += GenLine(0, "using System.Runtime.InteropServices;");
            genCode += GenLine(0, "using EngineNS;");
            genCode += "\n\n\n";

            var ns = desc.GetNameSpace();

            if (ns != null)
            {
                genCode += $"namespace {ns}\n";
                genCode += "{\n";
            }

            if (desc.GetMetaValue(CppMetaBase.Symbol.SV_LayoutStruct) != null)
            {
                genCode += GenCSharpLayoutStruct(desc);
            }
            else
            {
                genCode += GenCppReflectionCSharp(desc);
            }

            if (ns != null)
            {
                genCode += "}\n";
            }

            var file = targetDir + GetGenFileName(desc);
            NewCsFiles.Add(file);
            if (System.IO.File.Exists(file))
            {
                string old_code = System.IO.File.ReadAllText(file);
                if (genCode == old_code)
                    return;
            }
            System.IO.File.WriteAllText(file, genCode);
        }

        #region Callback Gen
        public void GenCallbackCode(string targetDir, List<CppCallback> cbCollector)
        {
            string genCode = "//This cs is generated by THT.exe\n";
            genCode += GenLine(0, "using System;");
            genCode += GenLine(0, "using System.Runtime.InteropServices;");
            genCode += GenLine(0, "using EngineNS;");
            genCode += "\n\n";

            foreach (var i in cbCollector)
            {
                var ns = i.GetNameSpace();

                int nTable = 0;
                if (ns != null)
                {
                    genCode += GenLine(nTable, $"namespace {ns}");
                    genCode += GenLine(nTable++, "{");
                }
                var callConvention = i.GetMetaValue("SV_CallConvention");
                if (callConvention == null)
                    callConvention = "System.Runtime.InteropServices.CallingConvention.Cdecl";
                genCode += GenLine(nTable, $"[UnmanagedFunctionPointer({callConvention})]");
                genCode += GenLine(nTable, $"public unsafe delegate {GetCallbackReturnType(i)} {i.Name}({GetParameterString(i, false)});");

                if (ns != null)
                {
                    genCode += GenLine(--nTable, "}");
                }
            }

            var file = targetDir + "CppDelegateDefine.gen.cs";
            NewCsFiles.Add(file);
            if (System.IO.File.Exists(file))
            {
                string old_code = System.IO.File.ReadAllText(file);
                if (genCode == old_code)
                    return;
            }
            System.IO.File.WriteAllText(file, genCode); ;
        }
        #endregion

        #region Enum Gen
        public override string GenEnumDefine(CppEnum klass)
        {
            string code = "";

            int nTable = 1;
            code += CodeGenerator.GenLine(nTable, $"public enum {klass.Name}");
            code += CodeGenerator.GenLine(nTable++, "{");
            foreach (var i in klass.Members)
            {
                if (i.Value == null)
                {
                    code += CodeGenerator.GenLine(nTable, $"{i.Name},");
                }
                else
                {
                    code += CodeGenerator.GenLine(nTable, $"{i.Name} = {i.Value},");
                }
            }
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        #endregion

        #region Class Gen
        public string GenCppReflectionCSharp(CppClass klass)
        {
            int nTable = 1;
            string code;
            //if (CodeGenerator.Instance.GenInternalClass)
            //    code = GenLine(nTable, $"internal unsafe partial struct {klass.Name}{Symbol.NativeSuffix}"); 
            //else
            code = GenLine(nTable, $"public unsafe partial struct {klass.Name}{CodeGenerator.Symbol.NativeSuffix}");
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

            //产生对外调用接口，包括c#的构造器，成员属性，成员函数，静态函数
            code += GenLine(nTable, "#region Native Wrapper");
            code += GenLine(nTable, "#region Constructor");
            code += CodeGenerator.GenLine(nTable, $"public {klass.Name}{CodeGenerator.Symbol.NativeSuffix}(IntPtr InPtr)");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, "mPtr.NativePointer = InPtr;");
            code += CodeGenerator.GenLine(--nTable, "}");
            if (klass.Constructors.Count > 0)
            {
                int ConstructorIndex = 0;
                foreach (var i in klass.Constructors)
                {
                    code += GenCallBinding(ref nTable, klass, ConstructorIndex++, i);
                }
            }
            code += GenLine(nTable, "#endregion");
            code += "\n";

            if (klass.Members.Count > 0)
            {
                code += GenLine(nTable, "#region Property");
                foreach (var i in klass.Members)
                {
                    if ((i.DeclareType & (EDeclareType.DT_Const | EDeclareType.DT_Static)) != 0)
                        continue;
                    if (i.Type.TypeCallback != null)
                    {
                        continue;
                    }
                    if (i.IsArray)
                    {
                        continue;
                    }
                    code += GenCallBinding(ref nTable, klass, i);
                }
                code += GenLine(nTable, "#endregion");
                code += "\n";
            }

            if (klass.Methods.Count > 0)
            {
                code += GenLine(nTable, "#region Method");
                int MethodIndex = 0;
                foreach (var i in klass.Methods)
                {
                    if (i.IsFriend)
                        continue;
                    if (i.IsStatic)
                    {
                        code += GenCallBinding_Static(ref nTable, klass, MethodIndex++, i);
                    }
                    else
                    {
                        code += GenCallBinding(ref nTable, klass, MethodIndex++, i);
                    }
                }
                code += GenLine(nTable, "#endregion");
                code += "\n";
            }
            code += GenLine(nTable, "#endregion");

            //下面是产生TSDK_的c++交互函数
            code += GenLine(nTable, "#region SDK");
            code += GenLine(nTable, $"const string ModuleNC={CodeGenerator.Instance.GenImportModuleNC};");
            if (klass.Constructors.Count > 0)
            {
                code += "\n";
                int ConstructorIndex = 0;
                foreach (var i in klass.Constructors)
                {
                    code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, GenPInvokeBinding(klass, ConstructorIndex++, i));
                }
                code += "\n";
            }
            if (klass.Members.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Members)
                {
                    if ((i.DeclareType & (EDeclareType.DT_Const | EDeclareType.DT_Static)) != 0)
                        continue;

                    if (i.Type.TypeCallback != null)
                    {
                        continue;
                    }

                    code += GenLine(nTable, $"[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, GenPInvokeBinding_Getter(klass, i));
                    if (i.IsArray == false)
                    {
                        code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                        code += GenLine(nTable, GenPInvokeBinding_Setter(klass, i));
                    }
                }
                code += "\n";
            }
            if (klass.Methods.Count > 0)
            {
                code += "\n";
                int MethodIndex = 0;
                foreach (var i in klass.Methods)
                {
                    if (i.IsFriend)
                        continue;
                    if (i.IsStatic)
                    {
                        code += GenLine(nTable, $"[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = {i.GetCharSet()})]");
                        code += GenLine(nTable, GenPInvokeBinding_Static(klass, false, MethodIndex++, i));
                    }
                    else
                    {
                        code += GenLine(nTable, $"[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = {i.GetCharSet()})]");
                        code += GenLine(nTable, GenPInvokeBinding(klass, "PtrType", false, MethodIndex++, i));
                    }
                }
                code += "\n";
            }
            code += GenLine(nTable, "#endregion");

            nTable--;
            code += GenLine(nTable, "}");//for struct klass.Name
            return code;
        }
        public string GenCSharpLayoutStruct(CppClass klass)
        {
            int structPack = 8;
            var layout = klass.GetMetaValue(CppMetaBase.Symbol.SV_LayoutStruct);
            if (layout != null)
            {
                structPack = System.Convert.ToInt32(layout);
            }
            int nTable = 1;
            string code;
            code = GenLine(nTable, $"[StructLayout(LayoutKind.Sequential, Pack={structPack})]");//Explicit
            code += GenLine(nTable, $"public unsafe partial struct {CodeGenerator.Symbol.LayoutPrefix}{klass.Name}");
            code += GenLine(nTable++, "{");

            var PtrType = $"{CodeGenerator.Symbol.LayoutPrefix}{klass.Name}*";
            code += GenLine(nTable, $"private unsafe {PtrType} mPtr");
            code += GenLine(nTable++, "{");
            code += GenLine(nTable, $"get {{ fixed ({PtrType} pThis = &this) return pThis; }}");
            code += GenLine(--nTable, "}");

            foreach (var i in klass.Members)
            {
                if ((i.DeclareType & (EDeclareType.DT_Static)) != 0)
                    continue;
                //if ((i.DeclareType & (EDeclareType.DT_Const)) != 0 && i.DefaultValue!="")
                //    continue; const int = 9;这种貌似是不算成员变量，有空确认一下以后再处理吧

                string declMember = "";
                switch (i.VisitMode)
                {
                    case EVisitMode.Public:
                        declMember += "public ";
                        break;
                    case EVisitMode.Protected:
                        declMember += "protected ";
                        break;
                    case EVisitMode.Private:
                        declMember += "private ";
                        break;
                }


                var converter = GetMemberType(i);

                if (i.Type.TypeCallback != null)
                {
                    var pname = i.Name.Replace("*", "");
                    declMember += $"IntPtr {pname};//{i.Type.TypeCallback.Name}";
                }
                else if (i.IsArray)
                {
                    if (IsAllowFixedArrayType(i.Type.Type))
                    {
                        declMember += $"fixed {converter} {i.Name}[";
                        foreach (var j in i.ArraySize)
                        {
                            declMember += $"{j},";
                        }
                        declMember = declMember.Remove(declMember.Length - 1);
                        declMember += "];";
                    }
                    else
                    {
                        int numOfElements = 0;
                        foreach (var j in i.ArraySize)
                        {
                            try
                            {
                                numOfElements += System.Convert.ToInt32(j);
                            }
                            catch
                            {
                                throw new Exception(CppHeaderScanner.TraceMessage($"Array Member Size is not a number"));
                            }
                        }
                        declMember += $"{converter} ";
                        for (int j = 0; j < numOfElements; j++)
                        {
                            if (j == numOfElements - 1)
                                declMember += $"{i.Name}{j};";
                            else
                                declMember += $"{i.Name}{j}, ";
                        }
                    }
                }
                else if (CodeGenerator.Instance.IsSystemType(i.Type.PureType))
                {
                    declMember += $"{converter} {i.Name};";
                }
                else
                {
                    var csType = GetMemberType(i);
                    declMember += $"{converter} {i.Name};";
                }

                code += GenLine(nTable, declMember);
            }

            if (klass.Methods.Count > 0)
            {
                code += GenLine(nTable, "#region Method");
                int MethodIndex = 0;
                foreach (var i in klass.Methods)
                {
                    if (i.IsFriend)
                        continue;
                    if (i.IsStatic)
                    {
                        code += GenCallBinding_Static(ref nTable, klass, MethodIndex++, i);
                    }
                    else
                    {
                        code += GenCallBinding(ref nTable, klass, MethodIndex++, i);
                    }
                }
                code += GenLine(nTable, "#endregion");
                code += "\n";
            }

            code += GenLine(nTable, "#region SDK");
            code += GenLine(nTable, $"const string ModuleNC={CodeGenerator.Instance.GenImportModuleNC};");
            if (klass.Methods.Count > 0)
            {
                code += "\n";
                int MethodIndex = 0;
                foreach (var i in klass.Methods)
                {
                    if (i.IsFriend)
                        continue;
                    if (i.IsStatic)
                    {
                        code += GenLine(nTable, $"[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = {i.GetCharSet()})]");
                        code += GenLine(nTable, GenPInvokeBinding_Static(klass, false, MethodIndex++, i));
                    }
                    else
                    {
                        code += GenLine(nTable, $"[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = {i.GetCharSet()})]");
                        code += GenLine(nTable, GenPInvokeBinding(klass, $"{CodeGenerator.Symbol.LayoutPrefix}{klass.Name}*", true, MethodIndex++, i));
                    }
                }
                code += "\n";
            }
            code += GenLine(nTable, "#endregion");

            code += GenLine(--nTable, "}");
            return code;
        }
        #endregion

        #region Bind&Call

        #region TypeConverter
        public bool IsAllowFixedArrayType(string type)
        {
            switch (type)
            {
                case "bool":
                case "byte":
                case "short":
                case "int":
                case "long":
                case "char":
                case "sbyte":
                case "ushort":
                case "uint":
                case "ulong":
                case "float":
                case "double":
                case "SByte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Byte":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return true;
                default:
                    return false;
            }
        }
        private string GetCSType(CppTypeDesc desc, EDeclareType DeclType, bool tryMashralString)
        {
            if (desc.TypeClass != null)
                return desc.TypeClass.GetCSName(desc.TypeStarNum);
            if (desc.TypeEnum != null)
                return desc.TypeEnum.GetCSName(desc.TypeStarNum);
            if (desc.TypeCallback != null)
                return desc.TypeCallback.GetCSName(desc.TypeStarNum);
            string csType = "";
            if ((DeclType & EDeclareType.DT_Unsigned) == EDeclareType.DT_Unsigned)
            {
                csType = CodeGenerator.Instance.NormalizePureType("unsigned " + desc.PureType);
            }
            else
            {
                csType = CodeGenerator.Instance.NormalizePureType(desc.PureType);
            }
            for (int i = 0; i < desc.TypeStarNum; i++)
            {
                csType += "*";
            }
            if (tryMashralString)
            {
                if (csType == "SByte*")
                    return "string";
                else if (csType == "Wchar16*")
                    return "string";
                else if (csType == "Wchar32*")
                    return "string";
            }
            return csType;
        }
        public string GetMemberType(CppMember member)
        {
            string result = GetCSType(member.Type, member.DeclareType, false);
            if (result[0] == '.')
            {
                return result.Substring(1);
            }
            else
            {
                return result;
            }
        }
        public string GetFunctionReturnType(CppFunction function)
        {
            EDeclareType dtStyle = 0;
            if (function.IsReturnUnsignedType)
            {
                dtStyle |= EDeclareType.DT_Unsigned;
            }
            if (function.IsReturnConstType)
            {
                dtStyle |= EDeclareType.DT_Const;
            }
            string result = GetCSType(function.ReturnType, dtStyle, false);
            if (result[0] == '.')
            {
                return result.Substring(1);
            }
            else
            {
                return result;
            }
        }
        public string GetArgumentType(CppCallParameters function, int index)
        {
            var arg = function.Arguments[index];
            bool tryMashralString = true;
            if (function.GetCharSet() == "CharSet.None")
                tryMashralString = false;
            string result = GetCSType(arg.Type, arg.DeclType, tryMashralString);
            if (result[0] == '.')
            {
                return result.Substring(1);
            }
            else
            {
                return result;
            }
        }
        public string GetCallbackReturnType(CppCallback function)
        {
            string result = GetCSType(function.ReturnType, function.DeclType, false);
            if (result[0] == '.')
            {
                return result.Substring(1);
            }
            else
            {
                return result;
            }
        }
        #endregion

        #region CallBinding
        public string GenCallBinding(ref int nTable, CppClass klass, int index, CppConstructor constructor)
        {
            string mode = "";
            switch (constructor.VisitMode)
            {
                case EVisitMode.Public:
                    mode = "public";
                    break;
                case EVisitMode.Protected:
                    mode = "protected";
                    break;
                case EVisitMode.Private:
                    mode = "private";
                    break;
            }
            var afterSelf = constructor.Arguments.Count > 0 ? ", " : "";
            string code = CodeGenerator.GenLine(nTable, $"{mode} {klass.Name}{CodeGenerator.Symbol.NativeSuffix}({GetParameterString(constructor, true)}{afterSelf}bool _dont_care_just_for_compile)");
            code += CodeGenerator.GenLine(nTable++, "{");
            string fixedCode;
            var callArgs = GetParameterCallStringWithStar2Ref(nTable, constructor, out fixedCode);
            if (fixedCode != "")
            {
                code += fixedCode;
                code += GenLine(nTable++, "{");
            }
            code += CodeGenerator.GenLine(nTable, $"mPtr = {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstructor{index}({callArgs});");
            if (fixedCode != "")
            {
                code += GenLine(--nTable, "}");
            }
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        public string GenCallBinding(ref int nTable, CppClass klass, CppMember member)
        {
            string mode = "";
            switch (member.VisitMode)
            {
                case EVisitMode.Public:
                    mode = "public";
                    break;
                case EVisitMode.Protected:
                    //mode = "protected";//编译器错误 CS0666结构体不接受protected的成员
                    mode = "private";
                    break;
                case EVisitMode.Private:
                    mode = "private";
                    break;
            }
            string code;
            code = CodeGenerator.GenLine(nTable, $"{mode} {GetMemberType(member)} {member.Name}");

            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;

            code += CodeGenerator.GenLine(nTable, $"get");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"return {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{member.Name}(mPtr);");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");

            code += CodeGenerator.GenLine(nTable, $"set");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{member.Name}(mPtr, value);");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");

            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
        public string GenCallBinding(ref int nTable, CppClass klass, int index, CppFunction function)
        {
            string mode = "";
            switch (function.VisitMode)
            {
                case EVisitMode.Public:
                    mode = "public";
                    break;
                case EVisitMode.Protected:
                    mode = "protected";
                    break;
                case EVisitMode.Private:
                    mode = "private";
                    break;
            }
            var returnType = GetFunctionReturnType(function);
            var afterSelf = function.Arguments.Count > 0 ? ", " : "";
            string code = GenLine(nTable, $"{mode} {returnType} {function.Name}({GetParameterString(function, true)})");
            code += GenLine(nTable++, "{");
            string fixedCode;
            var callArgs = GetParameterCallStringWithStar2Ref(nTable, function, out fixedCode);
            if (fixedCode != "")
            {
                code += fixedCode;
                code += GenLine(nTable++, "{");
            }
            if (returnType == "void")
                code += GenLine(nTable, $"{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}(mPtr{afterSelf}{callArgs});");
            else
                code += GenLine(nTable, $"return {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}(mPtr{afterSelf}{callArgs});");
            if (fixedCode != "")
            {
                code += GenLine(--nTable, "}");
            }
            code += GenLine(--nTable, "}");

            var genStatic = function.GetMetaValue(CppClass.Symbol.SV_GenStaticFunction);
            if (genStatic != null)
            {
                var selfType = klass.Name + "_PtrType.PtrType";//
                if (klass.GetMetaValue(CppClass.Symbol.SV_LayoutStruct) != null)
                {
                    selfType = klass.Name;
                }
                code += CodeGenerator.GenLine(nTable, $"{mode} static {returnType} {function.Name}({selfType}* self{afterSelf}{GetParameterString(function, true)})");
                code += CodeGenerator.GenLine(nTable++, "{");
                if (fixedCode != "")
                {
                    code += fixedCode;
                    code += CodeGenerator.GenLine(nTable++, "{");
                }
                if (returnType == "void")
                    code += CodeGenerator.GenLine(nTable, $"{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}(self{afterSelf}{callArgs});");
                else
                    code += CodeGenerator.GenLine(nTable, $"return {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}(self{afterSelf}{callArgs});");
                if (fixedCode != "")
                {
                    code += CodeGenerator.GenLine(--nTable, "}");
                }
                code += CodeGenerator.GenLine(--nTable, "}");
            }
            return code;
        }
        public string GenCallBinding_Static(ref int nTable, CppClass klass, int index, CppFunction function)
        {
            var returnType = GetFunctionReturnType(function);
            string code = GenLine(nTable, $"public static {returnType} {function.Name}({GetParameterString(function, true)})");
            code += GenLine(nTable++, "{");
            string fixedCode;
            var callArgs = GetParameterCallStringWithStar2Ref(nTable, function, out fixedCode);
            if (fixedCode != "")
            {
                code += fixedCode;
                code += GenLine(nTable++, "{");
            }
            if (returnType == "void")
                code += GenLine(nTable, $"Static_{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}({callArgs});");
            else
                code += GenLine(nTable, $"return Static_{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}({callArgs});");
            if (fixedCode != "")
            {
                code += GenLine(--nTable, "}");
            }
            code += GenLine(--nTable, "}");
            return code;
        }
        #endregion

        #region Parameters
        public override string GetParameterString(CppCallParameters callParameters, bool convertStar2Ref)
        {//bPtrType如果false，那么就要把XXX_PtrType.PtrType转换成XXX_PtrType
            string result = "";
            for (int i = 0; i < callParameters.Arguments.Count; i++)
            {
                var csType = GetArgumentType(callParameters,i);
                if (convertStar2Ref)
                {
                    if (callParameters.IsNoStar2RefArgument(i) == false && csType.EndsWith("*") && callParameters.Arguments[i].Type.TypeStarNum == 1 && csType != "void*")
                    {
                        csType = "ref " + csType.Substring(0, csType.Length - 1);
                    }
                }
                var dftValue = callParameters.Arguments[i].CppDefaultValue;
                if (dftValue != null)
                {
                    dftValue = $"/*{dftValue}*/";
                }
                if (i == 0)
                    result += $"{csType} {callParameters.Arguments[i].Value}{dftValue}";
                else
                    result += $", {csType} {callParameters.Arguments[i].Value}{dftValue}";
            }
            return result;
        }
        public string GetParameterCallStringWithStar2Ref(int nTable, CppCallParameters function, out string fixedCode)
        {
            fixedCode = "";
            string result = "";
            for (int i = 0; i < function.Arguments.Count; i++)
            {
                var arg = function.Arguments[i].Value;
                var csType = GetArgumentType(function,i);
                if (csType.EndsWith("*") && function.Arguments[i].Type.TypeStarNum == 1 && csType != "void*")
                {
                    if (function.IsNoStar2RefArgument(i) == false)
                    {
                        fixedCode += CodeGenerator.GenLine(nTable, $"fixed ({csType} native_ptr_{arg} = &{arg})");
                        arg = $"native_ptr_{arg}";
                    }
                }

                if (i == 0)
                    result += $"{arg}";
                else
                    result += $", {arg}";
            }
            return result;
        }
        #endregion

        #region PInvoke
        public string GenPInvokeBinding(CppClass klass, int index, CppConstructor constructor)
        {
            var afterSelf = constructor.Arguments.Count > 0 ? ", " : "";
            return $"private extern static PtrType {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstructor{index}({GetParameterString(constructor, false)});";
        }
        public string GenPInvokeBinding(CppClass klass, string selfType, bool isUnsafe, int index, CppFunction function)
        {
            var CSReturnType = GetFunctionReturnType(function);
            var afterSelf = function.Arguments.Count > 0 ? ", " : "";
            var unsafeDef = isUnsafe ? "unsafe " : "";
            return $"private extern static {unsafeDef}{CSReturnType} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}({selfType} self{afterSelf}{GetParameterString(function, false)});";
        }
        public string GenPInvokeBinding_Static(CppClass klass, bool isUnsafe, int index, CppFunction function)
        {
            var CSReturnType = GetFunctionReturnType(function);
            var unsafeDef = isUnsafe ? "unsafe " : "";
            return $"private extern static {unsafeDef}{CSReturnType} Static_{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{function.Name}{index}({GetParameterString(function, false)});";
        }
        public string GenPInvokeBinding_Getter(CppClass klass, CppMember member)
        {
            var CSType = GetMemberType(member);
            if (member.IsArray)
                return $"private extern static {CSType}* {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{member.Name}_ArrayAddress(PtrType self);";
            else
                return $"private extern static {CSType} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{member.Name}(PtrType self);";
        }
        public string GenPInvokeBinding_Setter(CppClass klass, CppMember member)
        {
            var CSType = GetMemberType(member);
            return $"private extern static void {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{member.Name}(PtrType self, {CSType} InValue);";
        }
        #endregion

        #endregion
    }
}

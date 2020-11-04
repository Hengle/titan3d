﻿using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    partial class CodeGenerator
    {
        public void GenCodeCSharp(string targetDir)
        {
            foreach (var i in ClassCollector)
            {
                string genCode = "//This cs is generated by THT.exe\n";
                genCode += GenLine(0, "using System.Runtime.InteropServices;");

                genCode += "\n\n\n";

                var ns = i.GetNameSpace();
                if (ns == null)
                    ns = "EngineNS";

                genCode += $"namespace {ns}\n";
                genCode += "{\n";
                if(i.GetMetaValue(CppMetaBase.Symbol.SV_LayoutStruct)!=null)
                {
                    genCode += GenCSharpLayoutStruct(i);
                }
                else
                {
                    genCode += GenCppReflectionCSharp(i);
                }
                genCode += "}\n";

                var file = targetDir + i.GetGenFileNameCSharp();
                System.IO.File.WriteAllText(file, genCode); ;
            }
        }
        public string GenCppReflectionCSharp(CppClass klass)
        {
            int nTable = 1;
            string code;
            if (CodeGenerator.Instance.GenInternalClass)
                code = GenLine(nTable, $"internal unsafe struct {klass.Name}{Symbol.NativeSuffix}"); 
            else
                code = GenLine(nTable, $"public unsafe struct {klass.Name}{Symbol.NativeSuffix}");
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

            code += GenLine(nTable, "#region Native Wrapper");
            if (klass.Constructors.Count > 0)
            {
                code += GenLine(nTable, "#region Constructor");
                code += CodeGenerator.GenLine(nTable, $"public {klass.Name}{CodeGenerator.Symbol.NativeSuffix}(IntPtr InPtr)");
                code += CodeGenerator.GenLine(nTable++, "{");
                code += CodeGenerator.GenLine(nTable, "mPtr.NativePointer = InPtr;");
                code += CodeGenerator.GenLine(--nTable, "}");

                int ConstructorIndex = 0;
                foreach (var i in klass.Constructors)
                {
                    code += i.GenCallBindingCSharp(ref nTable, klass, ConstructorIndex++);
                }
                code += GenLine(nTable, "#endregion");
                code += "\n";
            }

            if (klass.Members.Count > 0)
            {
                code += GenLine(nTable, "#region Property");
                foreach (var i in klass.Members)
                {
                    if ((i.DeclareType & (EDeclareType.DT_Const | EDeclareType.DT_Static)) != 0)
                        continue;
                    code += i.GenCallBindingCSharp(ref nTable, klass);
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
                        continue;
                    code += i.GenCallBindingCSharp(ref nTable, klass, MethodIndex++); 
                }
                code += GenLine(nTable, "#endregion");
                code += "\n";
            }
            code += GenLine(nTable, "#endregion");

            code += GenLine(nTable, "#region SDK");
            code += GenLine(nTable, $"const string ModuleNC={CodeGenerator.Instance.GenImportModuleNC};");
            if (klass.Constructors.Count > 0)
            {
                code += "\n";
                int ConstructorIndex = 0;
                foreach (var i in klass.Constructors)
                {
                    code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, i.GenPInvokeBindingCSharp(klass, ConstructorIndex++));
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

                    code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, i.GenPInvokeBindingCSharp_Getter(klass));
                    code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, i.GenPInvokeBindingCSharp_Setter(klass));
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
                        continue;

                    code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, i.GenPInvokeBindingCSharp(klass, "PtrType", false, MethodIndex++));
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
            code += GenLine(nTable, $"public unsafe struct {Symbol.LayoutPrefix}{klass.Name}");
            code += GenLine(nTable++, "{");

            var PtrType = $"{Symbol.LayoutPrefix}{klass.Name}*";
            code += GenLine(nTable, $"private unsafe {PtrType} mPtr");
            code += GenLine(nTable++, "{");
            code += GenLine(nTable, $"get {{ fixed ({PtrType} pThis = &this) return pThis; }}");
            code += GenLine(--nTable, "}");

            foreach (var i in klass.Members)
            {
                if ((i.DeclareType & (EDeclareType.DT_Const | EDeclareType.DT_Static)) != 0)
                    continue;

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

                if (CodeGenerator.Instance.IsSystemType(i.PureType))
                    declMember += $"{i.Type} {i.Name};";
                else
                {
                    var csType = i.CSType;// CppTypeToCSType(i.Type, true, out isNativPtr);
                    declMember += $"{Symbol.LayoutPrefix}{csType} {i.Name};";
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
                        continue;
                    code += i.GenCallBindingCSharp(ref nTable, klass, MethodIndex++);
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
                        continue;
                    code += GenLine(nTable, "[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]");
                    code += GenLine(nTable, i.GenPInvokeBindingCSharp(klass, $"{Symbol.LayoutPrefix}{klass.Name}*", true, MethodIndex++));
                }
                code += "\n";
            }
            code += GenLine(nTable, "#endregion");

            code += GenLine(--nTable, "}");
            return code;
        }
        public static string GenLine(int nTable, string content)
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

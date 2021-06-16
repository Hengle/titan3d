﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
    class UClassCodeCpp : UCodeBase
    {
        public UClass mClass;
        public UClassCodeCpp()
        {

        }
        public UClassCodeCpp(UClass kls)
        {
            this.FullName = kls.FullName;
            mClass = kls;
        }
        public override string GetFileExt()
        {
            return ".cpp2cs.cpp";
        }        
        public override void OnGenCode()
        {
            AddLine($"//generated by CppWeaving");
            ClangSharp.Interop.CXFile tfile;
            uint line, col, offset;
            mClass.Decl.Location.GetFileLocation(out tfile, out line, out col, out offset);
            AddLine($"#include \"{UProjectSettings.Pch}\"");
            AddLine($"#if defined(HasModule_{mClass.ModuleName})");
            AddLine($"#include \"{UTypeManagerBase.GetRegularPath(tfile.ToString())}\"");
            AddLine($"#include \"{UProjectSettings.CppPODStruct}\"");

            NewLine();
            AddLine($"#define new VNEW");
            NewLine();

            if (mClass.VisitorNS!= "")
            {
                AddLine($"namespace {mClass.VisitorNS}");
                PushBrackets();
            }
            
            AddLine($"struct {mClass.VisitorClassName}");
            PushBrackets();
            {
                GenConstructor();
                GenCast();
                GenFields();
                GenFunction();
            }
            PopBrackets(true);

            if (mClass.VisitorNS != "")
            {
                PopBrackets();
            }

            NewLine();
            GenPInvokeConstructor();
            NewLine();
            GenPInvokeCast();
            NewLine();
            GenPInvokeFields();
            NewLine();
            GenPInvokeFunction();

            AddLine($"#endif//HasModule_{mClass.ModuleName}");
        }
        protected void GenFields()
        {
            foreach (var i in mClass.Properties)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;

                string retTypeStr;
                if (i.IsDelegate)
                {
                    retTypeStr = "void*";
                }
                else
                {
                    retTypeStr = i.GetCppTypeName();
                }
                if (i.IsDelegate)
                {
                    AddLine($"static inline void* FieldGet__{i.Name}({mClass.ToCppName()}* self)");
                }
                else
                {
                    AddLine($"static inline {retTypeStr} FieldGet__{i.Name}({mClass.ToCppName()}* self)");
                }
                PushBrackets();
                {
                    AddLine($"if(self==nullptr)");
                    PushBrackets();
                    {
                        if (retTypeStr.EndsWith("&") == false)
                            AddLine($"return {UProjectSettings.VGetTypeDefault}<{retTypeStr}>();");
                        else
                        {
                            retTypeStr = retTypeStr.Substring(0, retTypeStr.Length - 1);
                            AddLine($"{retTypeStr}* tmp = nullptr;");
                            AddLine($"return *tmp;");
                        }
                    }
                    PopBrackets();
                    AddLine($"return ({retTypeStr})self->{i.Name};");
                }
                PopBrackets();

                if (!i.HasMeta(UProjectSettings.SV_ReadOnly))
                {
                    if (i.IsDelegate)
                    {
                        AddLine($"static inline void FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()})");
                    }
                    else
                    {
                        AddLine($"static inline void FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()} value)");
                    }
                    PushBrackets();
                    {
                        AddLine($"if(self==nullptr)");
                        PushBrackets();
                        {
                            AddLine($"return;");
                        }
                        PopBrackets();

                        if (i.NumOfElement > 0)
                        {
                            AddLine($"for (int i = 0; i < {i.NumOfElement}; i++)");
                            PushBrackets();
                            {
                                AddLine($"self->{i.Name}[i] = value[i];");
                            }
                            PopBrackets();
                        }
                        else
                        {
                            if (i.IsDelegate)
                            {
                                AddLine($"*(void**)&(self->{i.Name}) = (void*){i.Name};");
                            }
                            else
                            {
                                AddLine($"self->{i.Name} = value;");
                            }
                        }
                    }
                    PopBrackets();
                }   
            }

        }
        protected void GenPInvokeFields()
        {
            foreach (var i in mClass.Properties)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;
                string retType = i.GetCppTypeName();
                if (i.IsDelegate)
                {
                    retType = "void*";
                }
                else if (i.IsStructType && i.NumOfTypePointer == 0)
                {
                    var structType = i.PropertyType as UStruct;
                    if (structType.ReturnPodName() != null)
                        retType = structType.ReturnPodName();
                    else
                        retType = $"{i.PropertyType.FullName.Replace(".", "_")}_PodType";
                }
                AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retType} TSDK_{mClass.VisitorPInvoke}_FieldGet__{i.Name}({mClass.ToCppName()}* self)");
                PushBrackets();
                {
                    if (i.IsStructType && i.GetCppTypeName() != retType)
                    {
                        AddLine($"auto tmp_result = {mClass.VisitorName}::FieldGet__{i.Name}(self);");
                        AddLine($"return {UProjectSettings.VReturnValueMarshal}<{i.GetCppTypeName()},{retType}>(tmp_result);");
                    }
                    else
                        AddLine($"return {mClass.VisitorName}::FieldGet__{i.Name}(self);");
                }
                PopBrackets();
                
                if (!i.HasMeta(UProjectSettings.SV_ReadOnly))
                {
                    if (i.IsDelegate)
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{mClass.VisitorPInvoke}_FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()})");
                    else
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{mClass.VisitorPInvoke}_FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()} value)");
                    PushBrackets();
                    {
                        if (i.IsDelegate)
                            AddLine($"{mClass.VisitorName}::FieldSet__{i.Name}(self, {i.Name});");
                        else
                            AddLine($"{mClass.VisitorName}::FieldSet__{i.Name}(self, value);");
                    }
                    PopBrackets();
                }   
            }

        }
        protected void GenFunction()
        {
            foreach (var i in mClass.Functions)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;

                string selfArg = $"{mClass.ToCppName()}* self";
                if (i.IsStatic)
                {
                    selfArg = "";
                }
                else
                {
                    if (i.Parameters.Count > 0)
                    {
                        selfArg += ",";
                    }
                }
                if (i.Parameters.Count > 0)
                    AddLine($"static inline {i.ReturnType.GetCppTypeName()} {i.Name}({selfArg} {i.GetParameterDefineCpp()})");
                else
                    AddLine($"static inline {i.ReturnType.GetCppTypeName()} {i.Name}({selfArg})");
                PushBrackets();
                {
                    var retTypeStr = i.ReturnType.GetCppTypeName();
                    if (i.IsStatic == false)
                    {
                        AddLine($"if(self==nullptr)");
                        PushBrackets();
                        {
                            if (retTypeStr.EndsWith("&") == false)
                                AddLine($"return {UProjectSettings.VGetTypeDefault}<{i.ReturnType.GetCppTypeName()}>();");
                            else
                            {
                                retTypeStr = retTypeStr.Substring(0, retTypeStr.Length - 1);
                                AddLine($"{retTypeStr}* tmp = nullptr;");
                                AddLine($"return *tmp;");
                            }
                        }
                        PopBrackets();
                    }
                    if (i.IsStatic)
                    {
                        AddLine($"return ({retTypeStr}){mClass.ToCppName()}::{i.Name}({i.GetParameterCalleeCpp()});");
                    }
                    else
                    {
                        AddLine($"return ({retTypeStr})self->{i.Name}({i.GetParameterCalleeCpp()});");
                    }
                }
                PopBrackets();
            }
        }
        protected void GenPInvokeFunction()
        {
            foreach (var i in mClass.Functions)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;

                string retTypeStr = i.ReturnType.GetCppTypeName();
                if (i.ReturnType.IsStructType && i.ReturnType.NumOfTypePointer == 0)
                {
                    var structType = i.ReturnType.PropertyType as UStruct;
                    if (structType.ReturnPodName() != null)
                        retTypeStr = structType.ReturnPodName();
                    else
                        retTypeStr = $"{i.ReturnType.PropertyType.FullName.Replace(".", "_")}_PodType";
                }

                string callArg = "self";
                if (i.IsStatic)
                {
                    callArg = "";
                }
                if (i.Parameters.Count > 0)
                {
                    if (callArg == "")
                        callArg += $"{i.GetParameterCalleeCpp()}";
                    else
                        callArg += $", {i.GetParameterCalleeCpp()}";
                }

                if (i.IsStatic)
                {
                    if (i.Parameters.Count == 0)
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{mClass.VisitorPInvoke}_{i.Name}_{i.FunctionHash}()");
                    else
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{mClass.VisitorPInvoke}_{i.Name}_{i.FunctionHash}({i.GetParameterDefineCpp()})");
                }
                else
                {
                    if (i.Parameters.Count == 0)
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{mClass.VisitorPInvoke}_{i.Name}_{i.FunctionHash}({mClass.ToCppName()}* self)");
                    else
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{mClass.VisitorPInvoke}_{i.Name}_{i.FunctionHash}({mClass.ToCppName()}* self, {i.GetParameterDefineCpp()})");
                }

                PushBrackets();
                {
                    if (i.ReturnType.IsStructType && i.ReturnType.GetCppTypeName() != retTypeStr)
                    {
                        AddLine($"auto tmp_result = {mClass.VisitorName}::{i.Name}({callArg});");
                        AddLine($"return {UProjectSettings.VReturnValueMarshal}<{i.ReturnType.GetCppTypeName()},{retTypeStr}>(tmp_result);");
                    }
                    else
                    {
                        AddLine($"return {mClass.VisitorName}::{i.Name}({callArg});");
                    }
                }
                PopBrackets();
            }
        }
        protected virtual void GenConstructor()
        {
            foreach (var i in mClass.Constructors)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;

                if (i.Parameters.Count > 0)
                    AddLine($"static inline {mClass.ToCppName()}* CreateInstance({i.GetParameterDefineCpp()})");
                else
                    AddLine($"static inline {mClass.ToCppName()}* CreateInstance()");
                PushBrackets();
                {
                    AddLine($"return new {mClass.ToCppName()}({i.GetParameterCalleeCpp()});");
                }
                PopBrackets();
            }
            if (mClass.HasMeta(UProjectSettings.SV_Dispose))
            {
                var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
                AddLine($"static void Dispose({mClass.ToCppName()}* self)");
                PushBrackets();
                {
                    AddLine($"{dispose};");
                }
                PopBrackets();
            }
        }
        protected virtual void GenPInvokeConstructor()
        {
            foreach (var i in mClass.Constructors)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;
                if (i.Parameters.Count > 0)
                    AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {mClass.ToCppName()}* TSDK_{mClass.VisitorPInvoke}_CreateInstance_{i.FunctionHash}({i.GetParameterDefineCpp()})");
                else
                    AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {mClass.ToCppName()}* TSDK_{mClass.VisitorPInvoke}_CreateInstance_{i.FunctionHash}()");
                PushBrackets();
                {
                    AddLine($"return {mClass.VisitorName}::CreateInstance({i.GetParameterCalleeCpp()});");
                }
                PopBrackets();
            }
            if (mClass.HasMeta(UProjectSettings.SV_Dispose))
            {
                var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
                AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{mClass.VisitorPInvoke}_Dispose({mClass.ToCppName()}* self)");
                PushBrackets();
                {
                    AddLine($"return {mClass.VisitorName}::Dispose(self);");
                }
                PopBrackets();
            }
        }
        protected void GenCast()
        {
            foreach (var i in mClass.BaseTypes)
            {
                AddLine($"static inline {i.ToCppName()}* CastTo_{i.ToCppName().Replace("::", "_")}({mClass.ToCppName()}* self)");
                PushBrackets();
                {
                    AddLine($"return static_cast<{i.ToCppName()}*>(self);");
                }
                PopBrackets();
            }
        }
        protected void GenPInvokeCast()
        {
            foreach (var i in mClass.BaseTypes)
            {
                AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {i.ToCppName()}* TSDK_{mClass.VisitorPInvoke}_CastTo_{i.ToCppName().Replace("::", "_")}({mClass.ToCppName()}* self)");
                PushBrackets();
                {
                    AddLine($"return {mClass.VisitorName}::CastTo_{i.ToCppName().Replace("::", "_")}(self);");
                }
                PopBrackets();
            }
        }
    }
}

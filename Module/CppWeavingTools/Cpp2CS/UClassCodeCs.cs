﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
    class UClassCodeCs : UCodeBase
    {
        public UClass mClass;
        public List<string> DelegateDeclares = new List<string>();
        public UClassCodeCs(UClass kls)
        {
            this.FullName = kls.FullName;
            mClass = kls;
        }
        public override string GetFileExt()
        {
            return ".cpp2cs.cs";
        }
        public override void OnGenCode()
        {
            var visitor_name = $"{this.FullName.Replace(".", "_")}_Visitor";
            bool bExpProtected = false;
            string friendNS = "";
            foreach (var i in mClass.Friends)
            {
                var frd = i.Replace("::", "_");
                if (frd.Contains(visitor_name))
                {
                    bExpProtected = true;
                    break;
                }
            }

            AddLine($"//generated by cmc");
            AddLine($"using System;");
            AddLine($"using System.Runtime.InteropServices;");

            NewLine();

            if (!string.IsNullOrEmpty(mClass.Namespace))
            {
                AddLine($"namespace {mClass.Namespace}");
                PushBrackets();
            }

            if (mClass.HasMeta(UProjectSettings.SV_Dispose))
            {
                AddLine($"public unsafe partial struct {Name} : EngineNS.IPtrType, IDisposable");
            }
            else
            {
                AddLine($"public unsafe partial struct {Name} : EngineNS.IPtrType");
            }

            PushBrackets();
            {
                DefineLayout(bExpProtected, visitor_name);

                AddLine($"#region Constructor&Cast");
                GenConstructor(bExpProtected, visitor_name);
                GenCast(bExpProtected, visitor_name);
                AddLine($"#endregion");

                AddLine($"#region Fields");
                GenFields(bExpProtected, visitor_name);
                AddLine($"#endregion");

                AddLine($"#region Function");
                GenFunction(bExpProtected, visitor_name);
                AddLine($"#endregion");

                AddLine($"#region Core SDK");
                {
                    AddLine($"const string ModuleNC = {UProjectSettings.ModuleNC};");
                    GenPInvokeConstructor(bExpProtected, visitor_name);

                    GenPInvokeFields(bExpProtected, visitor_name);

                    GenPInvokeFunction(bExpProtected, visitor_name);

                    GenPInvokeCast(bExpProtected, visitor_name);
                }
                AddLine($"#endregion");
            }
            PopBrackets();

            if (!string.IsNullOrEmpty(mClass.Namespace))
            {
                PopBrackets();
            }
        }
        protected virtual void DefineLayout(bool bExpProtected, string visitor_name)
        {
            AddLine($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = {mClass.Decl.TypeForDecl.Handle.SizeOf}, Pack = {mClass.Decl.TypeForDecl.Handle.AlignOf})]");
            AddLine($"public struct CppStructLayout");
            PushBrackets();
            {
                foreach(var i in mClass.Properties)
                {
                    var field = i as UField;
                    AddLine($"[System.Runtime.InteropServices.FieldOffset({field.Offset / 8})]");
                    if(i.IsDelegate)
                        AddLine($"public IntPtr {i.Name};");
                    else
                    {
                        var retType = i.GetCsTypeName();
                        if (i.IsTypeDef)
                        {
                            var dypeDef = USysClassManager.Instance.FindTypeDef(i.CxxName);
                            if (dypeDef != null)
                                retType = dypeDef;
                        }
                        AddLine($"public {retType} {i.Name};");
                    }   
                }
            }
            PopBrackets();

            AddLine($"private void* mPointer;");
            AddLine($"public CppStructLayout* UnsafeAsLayout {{ get => (CppStructLayout*)mPointer; }}");
            AddLine($"public {mClass.Name}(void* p) {{ mPointer = p; }}");
            AddLine($"public void UnsafeSetPointer(void* p) {{ mPointer = p; }}");
            AddLine($"public IntPtr NativePointer {{ get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }}");
            AddLine($"public {mClass.Name}* CppPointer {{ get => ({mClass.Name}*)mPointer; }}");
            AddLine($"public bool IsValidPointer {{ get => mPointer != (void*)0; }}");

            AddLine($"public static implicit operator {mClass.Name}* ({mClass.Name} v)");
            PushBrackets();
            {
                AddLine($"return ({Name}*)v.mPointer;");
            }
            PopBrackets();
        }
        protected virtual void BeginInvoke()
        {

        }
        protected virtual void EndInvoke()
        {

        }
        protected virtual void GenConstructor(bool bExpProtected, string visitor_name)
        {
            foreach (var i in mClass.Constructors)
            {
                if (i.Access != EAccess.Public && bExpProtected == false)
                    continue;

                if (i.Parameters.Count > 0)
                    AddLine($"public static {mClass.ToCsName()} CreateInstance({i.GetParameterDefineCs()})");
                else
                    AddLine($"public static {mClass.ToCsName()} CreateInstance()");
                PushBrackets();
                {
                    var sdk_fun = $"TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}";
                    if (i.Parameters.Count > 0)
                        AddLine($"return new {mClass.ToCsName()}({sdk_fun}({i.GetParameterCalleeCs()}));");
                    else
                        AddLine($"return new {mClass.ToCsName()}({sdk_fun}());");
                }
                PopBrackets();
            }
            if (mClass.HasMeta(UProjectSettings.SV_Dispose))
            {
                var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
                AddLine($"public void Dispose()");
                PushBrackets();
                {
                    var sdk_fun = $"TSDK_{visitor_name}_Dispose";
                    BeginInvoke();
                    AddLine($"{sdk_fun}(mPointer);");
                    EndInvoke();
                }
                PopBrackets();
            }
        }
        protected virtual void GenPInvokeConstructor(bool bExpProtected, string visitor_name)
        {
            AddLine($"//Constructor&Cast");
            foreach (var i in mClass.Constructors)
            {
                if (i.Access != EAccess.Public && bExpProtected == false)
                    continue;
                UTypeManager.WritePInvokeAttribute(this, i);
                if (i.Parameters.Count > 0)
                    AddLine($"extern static {mClass.ToCsName()}* TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}({i.GetParameterDefineCs()});");
                else
                    AddLine($"extern static {mClass.ToCsName()}* TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}();");
            }
            if (mClass.HasMeta(UProjectSettings.SV_Dispose))
            {
                UTypeManager.WritePInvokeAttribute(this, null);
                AddLine($"extern static void TSDK_{visitor_name}_Dispose(void* self);");
            }
        }
        protected virtual void GenFields(bool bExpProtected, string visitor_name)
        {
            foreach (var i in mClass.Properties)
            {
                if (i.Access != EAccess.Public && bExpProtected == false)
                    continue;
                bool pointerTypeWrapper = false;
                if (i.NumOfTypePointer == 1 && i.PropertyType.ClassType == UTypeBase.EClassType.PointerType)
                {
                    pointerTypeWrapper = true;
                }

                if (i.IsDelegate)
                {
                    var dlgt = i.PropertyType as UDelegate;
                    if (dlgt != null)
                    {
                        AddLine($"public {dlgt.GetCsDelegateDefine()};");
                    }
                    AddLine($"{GetAccessDefine(i.Access)} {i.GetCsTypeName()} {i.Name}");
                }
                else
                {
                    if (pointerTypeWrapper)
                        AddLine($"{GetAccessDefine(i.Access)} {i.PropertyType.ToCsName()} {i.Name}");
                    else
                    {
                        var retType = i.GetCsTypeName();
                        if (i.HasMeta(UProjectSettings.SV_NoStringConverter) == false)
                        {
                            if (retType == "sbyte*")
                            {
                                retType = "string";
                            }
                        }
                        if (i.IsTypeDef)
                        {
                            var dypeDef = USysClassManager.Instance.FindTypeDef(i.CxxName);
                            if (dypeDef != null)
                                retType = dypeDef;
                        }
                        AddLine($"{GetAccessDefine(i.Access)} {retType} {i.Name}");
                    }
                }
                PushBrackets();
                {
                    AddLine($"get");
                    PushBrackets();
                    {
                        string pinvoke = $"TSDK_{visitor_name}_FieldGet__{i.Name}(mPointer)";
                        BeginInvoke();
                        if (pointerTypeWrapper)
                        {
                            AddLine($"return new {i.PropertyType.ToCsName()}({pinvoke});");
                        }
                        else
                        {
                            AddLine($"return {pinvoke};");
                        }
                        EndInvoke();
                    }
                    PopBrackets();

                    AddLine($"set");
                    PushBrackets();
                    {
                        BeginInvoke();
                        string pinvoke = $"TSDK_{visitor_name}_FieldSet__{i.Name}";
                        if (pointerTypeWrapper)
                        {
                            AddLine($"{pinvoke}(mPointer, value);");
                        }
                        else
                        {
                            AddLine($"{pinvoke}(mPointer, value);");
                        }
                        EndInvoke();
                    }
                    PopBrackets();
                }
                PopBrackets();
            }

        }
        protected virtual void GenPInvokeFields(bool bExpProtected, string visitor_name)
        {
            AddLine($"//Fields");
            foreach (var i in mClass.Properties)
            {
                if (i.Access != EAccess.Public && bExpProtected == false)
                    continue;

                var retType = i.GetCsTypeName();
                if (i.HasMeta(UProjectSettings.SV_NoStringConverter) == false)
                {
                    if (retType == "sbyte*")
                    {
                        retType = "string";
                    }
                }
                if (i.IsTypeDef)
                {
                    var dypeDef = USysClassManager.Instance.FindTypeDef(i.CxxName);
                    if (dypeDef != null)
                        retType = dypeDef;
                }

                UTypeManager.WritePInvokeAttribute(this, i);
                AddLine($"extern static {retType} TSDK_{visitor_name}_FieldGet__{i.Name}(void* self);");

                UTypeManager.WritePInvokeAttribute(this, i);
                AddLine($"extern static void TSDK_{visitor_name}_FieldSet__{i.Name}(void* self, {retType} value);");
            }
        }

        protected void GenFunction(bool bExpProtected, string visitor_name)
        {
            foreach (var i in mClass.Functions)
            {
                if (i.Access != EAccess.Public && bExpProtected == false)
                    continue;
                bool pointerTypeWrapper = false;
                if (i.ReturnType.NumOfTypePointer == 1 && i.ReturnType.PropertyType.ClassType == UTypeBase.EClassType.PointerType)
                {
                    pointerTypeWrapper = true;
                }
                var retTypeStr = i.ReturnType.GetCsTypeName();
                if (pointerTypeWrapper)
                {
                    retTypeStr = i.ReturnType.PropertyType.ToCsName();
                }
                else
                {
                    if (i.HasMeta(UProjectSettings.SV_NoStringConverter) == false)
                    {
                        if (retTypeStr == "sbyte*")
                        {
                            retTypeStr = "string";
                        }
                    }
                }

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

                string retStr = "return ";
                if (retTypeStr == "void")
                    retStr = "";

                string callArg;

                i.GenCsDelegateDefine(this);
                if (i.IsStatic)
                {
                    AddLine($"{GetAccessDefine(i.Access)} static {retTypeStr} {i.Name}({i.GetParameterDefineCs()})");
                    callArg = "";
                }
                else
                {
                    AddLine($"{GetAccessDefine(i.Access)} {retTypeStr} {i.Name}({i.GetParameterDefineCs()})");
                    callArg = "mPointer";
                }
                if (i.Parameters.Count > 0)
                {
                    if (callArg == "")
                        callArg += i.GetParameterCalleeCs();
                    else
                        callArg += ", " + i.GetParameterCalleeCs();
                }
                PushBrackets();
                {
                    BeginInvoke();
                    var invoke = $"TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}";
                    if (pointerTypeWrapper)
                    {
                        AddLine($"{retStr}new {retTypeStr}({invoke}({callArg}));");
                    }
                    else
                    {
                        AddLine($"{retStr}{invoke}({callArg});");
                    }
                    EndInvoke();
                }
                PopBrackets();

                if (i.IsRefConvert())
                {
                    if (i.IsStatic)
                        AddLine($"{GetAccessDefine(i.Access)} static {retTypeStr} {i.Name}({i.GetParameterDefineCsRefConvert()})");
                    else
                        AddLine($"{GetAccessDefine(i.Access)} {retTypeStr} {i.Name}({i.GetParameterDefineCsRefConvert()})");

                    PushBrackets();
                    {
                        i.WritePinRefConvert(this);
                        PushBrackets();
                        {
                            AddLine($"{retStr}{i.Name}({i.GetParameterCalleeCsRefConvert()});");
                        }
                        PopBrackets();
                    }
                    PopBrackets();
                }
            }
        }
        protected void GenPInvokeFunction(bool bExpProtected, string visitor_name)
        {
            AddLine($"//Functions");
            foreach (var i in mClass.Functions)
            {
                if (i.Access != EAccess.Public && bExpProtected == false)
                    continue;

                UTypeManager.WritePInvokeAttribute(this, i);
                string callStr = "";
                if (!i.IsStatic)
                {
                    callStr = "void* Self";
                }
                if (i.Parameters.Count > 0)
                {
                    if (callStr == "")
                        callStr += i.GetParameterDefineCs();
                    else
                        callStr += ", " + i.GetParameterDefineCs();
                }
                var retTypeStr = i.ReturnType.GetCsTypeName();
                if (i.HasMeta(UProjectSettings.SV_NoStringConverter) == false)
                {
                    if (retTypeStr == "sbyte*")
                    {
                        retTypeStr = "string";
                    }
                }
                AddLine($"extern static {retTypeStr} TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}({callStr});");
            }
        }
        protected void GenCast(bool bExpProtected, string visitor_name)
        {
            if (mClass.BaseTypes.Count == 1)
            {
                var bType = mClass.BaseTypes[0];
                AddLine($"public {bType.ToCsName()} CastSuper()");
                PushBrackets();
                {
                    var invoke = $"TSDK_{visitor_name}_CastTo_{bType.ToCppName().Replace("::", "_")}";
                    BeginInvoke();
                    AddLine($"return new {bType.ToCsName()}({invoke}(mPointer));");
                    EndInvoke();
                }
                PopBrackets();

                AddLine($"public {bType.ToCsName()} NativeSuper");
                PushBrackets();
                {
                    AddLine($"get {{ return CastSuper(); }}");
                }
                PopBrackets();
                return;
            }
            else
            {
                foreach (var i in mClass.BaseTypes)
                {
                    AddLine($"public {i.ToCsName()} CastTo_{i.ToCppName().Replace("::", "_")}()");
                    PushBrackets();
                    {
                        var invoke = $"TSDK_{visitor_name}_CastTo_{i.ToCppName().Replace("::", "_")}";
                        BeginInvoke();
                        AddLine($"return new {i.ToCsName()}({invoke}(mPointer);");
                        EndInvoke();
                    }
                    PopBrackets();
                }
            }
        }
        protected void GenPInvokeCast(bool bExpProtected, string visitor_name)
        {
            AddLine($"//Cast");
            foreach (var i in mClass.BaseTypes)
            {
                UTypeManager.WritePInvokeAttribute(this, i);
                AddLine($"extern static {i.ToCsName()}* TSDK_{visitor_name}_CastTo_{i.ToCppName().Replace("::", "_")}(void* self);");
            }
        }
    }
}

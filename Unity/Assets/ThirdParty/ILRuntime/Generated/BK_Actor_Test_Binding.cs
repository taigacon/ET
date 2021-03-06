using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class BK_Actor_Test_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(BK.Actor_Test);

            field = type.GetField("Info", flag);
            app.RegisterCLRFieldGetter(field, get_Info_0);
            app.RegisterCLRFieldSetter(field, set_Info_0);


        }



        static object get_Info_0(ref object o)
        {
            return ((BK.Actor_Test)o).Info;
        }
        static void set_Info_0(ref object o, object v)
        {
            ((BK.Actor_Test)o).Info = (System.String)v;
        }


    }
}

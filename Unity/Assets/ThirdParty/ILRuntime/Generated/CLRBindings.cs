using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {


        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_NotImplementedException_Binding.Register(app);
            System_String_Binding.Register(app);
            System_Exception_Binding.Register(app);
            System_Collections_IDictionary_Binding.Register(app);
            BK_Log_Binding.Register(app);
            BK_Actor_Test_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Type_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            BK_IdGenerater_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt64_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_List_1_ILTypeInstance_Binding.Register(app);
            BK_UnOrderMultiMap_2_Type_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Queue_1_UInt64_Binding.Register(app);
            System_Collections_Generic_List_1_Object_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            BK_Game_Binding.Register(app);
            BK_Hotfix_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding_Enumerator_Binding.Register(app);
            System_Activator_Binding.Register(app);
            BK_EventAttribute_Binding.Register(app);
            BK_EventProxy_Binding.Register(app);
            BK_EventSystem_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_Queue_1_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Collections_Generic_Queue_1_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Action_1_ILTypeInstance_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            BK_Object_Binding.Register(app);
            BK_Packet_Binding.Register(app);
            BK_ProtobufHelper_Binding.Register(app);
            BK_MessageInfo_Binding.Register(app);
            BK_Session_Binding.Register(app);
            System_Threading_Tasks_TaskCompletionSource_1_ILTypeInstance_Binding.Register(app);
            System_Threading_CancellationToken_Binding.Register(app);
            BK_RpcException_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_UInt16_List_1_ILTypeInstance_Binding.Register(app);
            BK_OpcodeTypeComponent_Binding.Register(app);
            BK_MessageProxy_Binding.Register(app);
            BK_MessageDispatherComponent_Binding.Register(app);
            BK_MessageAttribute_Binding.Register(app);
            BK_DoubleMap_2_UInt16_Type_Binding.Register(app);
            ProtoBuf_PType_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            BK_UIBind_UIBindRoot_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Int32_BK_IDisposableClassInheritanceAdaptor_Binding_IDisposableAdaptor_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Type_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_Binding.Register(app);
            AwaitExtensions_Binding.Register(app);
            System_Collections_Generic_List_1_Int32_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            BK_ResourcesComponent_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}

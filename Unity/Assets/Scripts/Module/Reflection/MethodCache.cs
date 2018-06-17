using System;
using System.Collections.Generic;
using System.Reflection;


namespace BK.Reflection
{
	public static class MethodCache
	{
		//考虑到ILRuntime，此时使用Type.FullName作为键存储
		private static readonly Dictionary<string, Dictionary<string, MethodInfo>> methodCache = new Dictionary<string, Dictionary<string, MethodInfo>>();

		public static Dictionary<string, MethodInfo> Get(Type type)
		{
			Dictionary<string, MethodInfo> methods;
			if (methodCache.TryGetValue(type.FullName, out methods))
			{
				return methods;
			}
			else
			{
				methods = new Dictionary<string, MethodInfo>();
				MethodInfo[] methodInfos = type.GetMethods(BindingFlags.CreateInstance | BindingFlags.DeclaredOnly | BindingFlags.Public);
				foreach (var methodInfo in methodInfos)
				{
					methods.Add(methodInfo.Name, methodInfo);
				}
				methodCache.Add(type.FullName, methods);
				return methods;
			}
		}
	}
}
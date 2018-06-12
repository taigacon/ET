using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ETModel.UIBind
{
	public static class UIBindUtils
	{
		private static readonly Dictionary<string, MethodInfo> bindMethodInfos = new Dictionary<string, MethodInfo>();
		private static readonly object[] param = new object[1];
		static UIBindUtils()
		{
			MethodInfo[] methodInfos = typeof(UIBindUtils).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
			foreach (var methodInfo in methodInfos)
			{
				bindMethodInfos.Add(methodInfo.Name, methodInfo);
			}
		}

		public static bool InvokeBindMethod(string bindFuncName, UIDataCell cell)
		{
			MethodInfo method;
			if (bindMethodInfos.TryGetValue(bindFuncName, out method))
			{
				param[0] = cell;
				method.Invoke(null, param);
				param[0] = null;
				return true;
			}

			return false;
		}
	}
}
using System.Collections.Generic;
using System.Reflection;
using BK.Reflection;

namespace BK.UIBind
{
	public interface IUIBindView
	{

	}

	public class UIBindView : DataSet, IUIBindView
	{
		private readonly Dictionary<string, MethodInfo> methods;
		private static readonly object[] param = new object[1];
		public UIBindView()
		{
			methods = PrivateMethodCache.Get(this.GetType());
		}

		public bool InvokeBindMethod(string bindFuncName, UIDataCell cell)
		{
			MethodInfo method;
			if (this.methods.TryGetValue(bindFuncName, out method))
			{
				param[0] = cell;
				method.Invoke(this, param);
				param[0] = null;
				return true;
			}

			return false;
		}
	}
}
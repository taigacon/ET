using System.Collections.Generic;
using System.Reflection;
using BK.Reflection;

namespace BK.UIBind
{
	public class DataSet
	{
		public delegate void OnChanged(object value);

		private readonly Dictionary<string, List<OnChanged>> bindHandlers = new Dictionary<string, List<OnChanged>>();
		private readonly Dictionary<string, PropertyInfo> myProperties;

		public DataSet()
		{
			myProperties = PropertyCache.Get(this.GetType());
		}

		public void AddListener(string key, OnChanged handler)
		{
			List<OnChanged> handlers;
			if (this.bindHandlers.TryGetValue(key, out handlers))
			{
				handlers.Add(handler);
			}
			else
			{
				this.bindHandlers.Add(key, new List<OnChanged> {handler});
			}
		}

		public void RemoveListener(string key, OnChanged handler)
		{
			List<OnChanged> handlers;
			if (this.bindHandlers.TryGetValue(key, out handlers))
			{
				handlers.Remove(handler);
			}
		}

		public void RemoveAllListeners(string key)
		{
			List<OnChanged> handlers;
			if (this.bindHandlers.TryGetValue(key, out handlers))
			{
				handlers.Clear();
			}
		}

		public void RemoveAllListeners()
		{
			this.bindHandlers.Clear();
		}

		public void RefreshData(object data)
		{
			Dictionary<string, PropertyInfo> dataProperties = PropertyCache.Get(data.GetType());
			foreach (var pair in dataProperties)
			{
				PropertyInfo pi;
				if (myProperties.TryGetValue(pair.Key, out pi))
				{
					if (typeof(DataSet).IsAssignableFrom(pi.PropertyType))
					{
						((DataSet)pi.GetValue(this)).RefreshData(pair.Value.GetValue(data));
					}
					else
					{
						pi.SetValue(this, pair.Value.GetValue(data));
					}
				}
				else
				{
					throw new KeyNotFoundException($"{pair.Key} not found");
				}
			}
		}

		public void Set(string key, object value)
		{
			PropertyInfo pi;
			if (myProperties.TryGetValue(key, out pi))
			{
				pi.SetValue(this, value);
			}
			else
			{
				throw new KeyNotFoundException($"{key} not found");
			}
		}

		public object Get(string key)
		{
			PropertyInfo pi;
			if (myProperties.TryGetValue(key, out pi))
			{
				return pi.GetValue(this);
			}
			else
			{
				throw new KeyNotFoundException($"{key} not found");
			}
		}

		public T Get<T>(string key)
		{
			return (T) Get(key);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ETModel.Reflection
{
	public static class PropertyCache
	{
		//考虑到ILRuntime，此时使用Type.FullName作为键存储
		private static readonly Dictionary<string, Dictionary<string, PropertyInfo>> propertyCache = new Dictionary<string, Dictionary<string, PropertyInfo>>();

		public static Dictionary<string, PropertyInfo> Get(Type type)
		{
			Dictionary<string, PropertyInfo> properties;
			if (propertyCache.TryGetValue(type.FullName, out properties))
			{
				return properties;
			}
			else
			{
				properties = new Dictionary<string, PropertyInfo>();
				PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.CreateInstance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
				foreach (var propertyInfo in propertyInfos)
				{
					properties.Add(propertyInfo.Name, propertyInfo);
				}
				propertyCache.Add(type.FullName, properties);
				return properties;
			}
		}
	}
}
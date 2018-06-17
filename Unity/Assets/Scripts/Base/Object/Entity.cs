﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace BK
{
	public class Entity : Object
	{
		private readonly Dictionary<Type, Component> componentDict = new Dictionary<Type, Component>();

		public Entity()
		{
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			foreach (Component component in this.GetComponents())
			{
				try
				{
					component.Parent = null;
					component.Dispose();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}

			this.componentDict.Clear();
		}
		
		public Component AddComponent(Component component)
		{
			Type type = component.GetType();
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.InstanceId}, component: {type.Name}");
			}

			this.componentDict.Add(type, component);
			return component;
		}

		public Component AddComponent(Type type)
		{
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.InstanceId}, component: {type.Name}");
			}

			Component component = ObjectFactory.CreateComponentWithParent(type, this);

			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K>() where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.InstanceId}, component: {typeof(K).Name}");
			}

			K component = ObjectFactory.CreateComponentWithParent<K>(this);

			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K, P1>(P1 p1) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.InstanceId}, component: {typeof(K).Name}");
			}

			K component = ObjectFactory.CreateComponentWithParent<K, P1>(this, p1);

			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K, P1, P2>(P1 p1, P2 p2) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.InstanceId}, component: {typeof(K).Name}");
			}

			K component = ObjectFactory.CreateComponentWithParent<K, P1, P2>(this, p1, p2);

			this.componentDict.Add(type, component);
			return component;
		}

		public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where K : Component, new()
		{
			Type type = typeof (K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.InstanceId}, component: {typeof(K).Name}");
			}

			K component = ObjectFactory.CreateComponentWithParent<K, P1, P2, P3>(this, p1, p2, p3);

			this.componentDict.Add(type, component);
			return component;
		}

		public void RemoveComponent<K>() where K : Component
		{
			Type type = typeof (K);
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return;
			}

			this.componentDict.Remove(type);

			component.Dispose();
		}

		public void RemoveComponent(Type type)
		{
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return;
			}
			
			this.componentDict.Remove(type);

			component.Dispose();
		}

		public K GetComponent<K>() where K : Component
		{
			Component component;
			if (!this.componentDict.TryGetValue(typeof(K), out component))
			{
				return default(K);
			}
			return (K)component;
		}

		public Component GetComponent(Type type)
		{
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return null;
			}
			return component;
		}

		public IEnumerable<Component> GetComponents()
		{
			return this.componentDict.Values;
		}
	}
}
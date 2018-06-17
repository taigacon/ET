using System;

namespace BKHotfix
{
	public static class ObjectFactory
	{
		public static Component CreateComponentWithParent(Type type, Entity parent)
		{
			Component component = (Component)Game.ObjectPool.Fetch(type);
			component.Parent = parent;
			Game.EventSystem.Awake(component);
			return component;
		}

		public static T CreateComponentWithParent<T>(Entity parent) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			Game.EventSystem.Awake(component);
			return component;
		}

		public static T CreateComponentWithParent<T, A>(Entity parent, A a) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			Game.EventSystem.Awake(component, a);
			return component;
		}

		public static Component CreateComponentWithParent<A>(Type type, Entity parent, A a)
		{
			Component component = (Component)Game.ObjectPool.Fetch(type);
			component.Parent = parent;
			Game.EventSystem.Awake(component, a);
			return component;
		}

		public static T CreateComponentWithParent<T, A, B>(Entity parent, A a, B b) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

		public static Component CreateComponentWithParent<A, B>(Type type, Entity parent, A a, B b)
		{
			Component component = (Component)Game.ObjectPool.Fetch(type);
			component.Parent = parent;
			Game.EventSystem.Awake(component, a, b);
			return component;
		}

		public static T CreateComponentWithParent<T, A, B, C>(Entity parent, A a, B b, C c) where T : Component
		{
			T component = Game.ObjectPool.Fetch<T>();
			component.Parent = parent;
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}

		public static Component CreateComponentWithParent<A, B, C>(Type type, Entity parent, A a, B b, C c)
		{
			Component component = (Component)Game.ObjectPool.Fetch(type);
			component.Parent = parent;
			Game.EventSystem.Awake(component, a, b, c);
			return component;
		}

		public static T CreateEntity<T>() where T : Entity
		{
			T entity = Game.ObjectPool.Fetch<T>();
			return entity;
		}

		public static Entity CreateEntity(Type type)
		{
			Entity entity = (Entity)Game.ObjectPool.Fetch(type);
			return entity;
		}
	}
}

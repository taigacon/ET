using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	public abstract class Component : Object
	{

		public Entity Parent { get; internal set; }

		public T GetParent<T>() where T : Entity
		{
			return this.Parent as T;
		}
		
		public Entity Entity => this.Parent;
	}
}
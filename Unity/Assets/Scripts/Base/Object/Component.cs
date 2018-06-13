using System;
using MongoDB.Bson.Serialization.Attributes;

namespace ETModel
{
	public abstract class Component : Object
	{
		private Component()
		{
		}

		public Entity Parent { get; internal set; }

		public T GetParent<T>() where T : Entity
		{
			return this.Parent as T;
		}
		
		public Entity Entity => this.Parent;

		public sealed override bool IsDisposed
		{
			get { return base.IsDisposed; }
			internal set
			{
				if (value != base.IsDisposed)
				{
					base.IsDisposed = value;
					if (value)
					{
						Game.EventSystem.Remove(this.InstanceId);
					}
					else
					{
						Game.EventSystem.Add(this);
					}
				}
			}
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			// 触发Destroy事件
			Game.EventSystem.Destroy(this);
			base.Dispose();
		}
	}
}
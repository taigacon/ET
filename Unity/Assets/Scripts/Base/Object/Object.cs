using System;
using LitJson;

namespace ETModel
{
	public abstract class Object : IDisposable
	{
		public ulong InstanceId { get; internal set; } = IdGenerater.GenerateId();

		private bool isFromPool;

		public bool IsFromPool
		{
			get
			{
				return this.isFromPool;
			}
			set
			{
				this.isFromPool = value;

				if (!this.isFromPool)
				{
					return;
				}

				if (this.InstanceId == 0)
				{
					this.InstanceId = IdGenerater.GenerateId();
				}

				Game.EventSystem.Add(this);
			}
		}

		public bool IsDisposed { get; private set; }
		public override string ToString()
		{
			return JsonMapper.ToJson(this);
		}

		public virtual void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			this.InstanceId = 0;

			Game.EventSystem.Remove(this.InstanceId);


			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
			}

			// 触发Destroy事件
			Game.EventSystem.Destroy(this);
		}
	}
}
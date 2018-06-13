using System;
using LitJson;

namespace ETModel
{
	public abstract class Object : IDisposable
	{
		public ulong InstanceId { get; } = IdGenerater.GenerateId();

		public virtual bool IsDisposed { get; internal set; } = true;

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
			Game.ObjectPool.Recycle(this);
		}
	}
}
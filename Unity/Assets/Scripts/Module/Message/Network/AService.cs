using System;
using System.Net;

namespace BK
{
	public enum NetworkProtocol
	{
		KCP,
		TCP,
	}

	public abstract class AService: Object
	{
		public abstract AChannel GetChannel(ulong id);

		private Action<AChannel> acceptCallback;

		public event Action<AChannel> AcceptCallback
		{
			add
			{
				this.acceptCallback += value;
			}
			remove
			{
				this.acceptCallback -= value;
			}
		}
		
		protected void OnAccept(AChannel channel)
		{
			this.acceptCallback.Invoke(channel);
		}

		public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);

		public abstract void Remove(ulong channelId);

		public abstract void Update();

		public abstract void Start();
	}
}
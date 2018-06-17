using System;
using System.Net;
using System.Threading.Tasks;

namespace BK
{
	public enum NetworkProtocol
	{
		TCP,
		KCP,
	}

	public abstract class AService: Object
	{
		public abstract AChannel GetChannel(ulong id);

		public abstract Task<AChannel> AcceptChannel();

		public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);

		public abstract void Remove(ulong channelId);

		public abstract void Update();
	}
}
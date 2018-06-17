using System;
using BK;

namespace ETHotfix
{
#if ILRuntime
	public interface IMHandler
	{
		void Handle(BK.Session session, object message);
		Type GetMessageType();
	}
#else
	public interface IMHandler : BK.IMHandler
	{
	}
#endif
}
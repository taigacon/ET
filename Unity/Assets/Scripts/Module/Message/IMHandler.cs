using System;

namespace BK
{
	public interface IMHandler
	{
		void Handle(Session session, object message);
		Type GetMessageType();
	}
}
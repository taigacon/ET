using BK;

namespace ETHotfix
{
	[MessageHandler]
	public class Actor_TestHandler : AMHandler<Actor_Test>
	{
		protected override void Run(BK.Session session, Actor_Test message)
		{
			Log.Debug(message.Info);
		}
	}
}

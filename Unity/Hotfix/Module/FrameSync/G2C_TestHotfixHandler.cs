using BK;

namespace ETHotfix
{
	[MessageHandler]
	public class G2C_TestHotfixMessageHandler : AMHandler<G2C_TestHotfixMessage>
	{
		protected override void Run(BK.Session session, G2C_TestHotfixMessage message)
		{
			Log.Debug(message.Info);
		}
	}
}
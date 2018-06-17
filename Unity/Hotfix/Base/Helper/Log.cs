using System;

namespace ETHotfix
{
	public static class Log
	{
		public static void Warning(string msg)
		{
			BK.Log.Warning(msg);
		}

		public static void Info(string msg)
		{
			BK.Log.Info(msg);
		}

		public static void Error(Exception e)
		{
			BK.Log.Error(e.ToStr());
		}

		public static void Error(string msg)
		{
			BK.Log.Error(msg);
		}

		public static void Debug(string msg)
		{
			BK.Log.Debug(msg);
		}
	}
}
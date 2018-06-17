namespace BK
{
	public static class IdGenerater
	{
		public static ulong AppId { private get; set; }

		private static ushort value;

		public static ulong GenerateId()
		{
			long time = TimeHelper.ClientNowSeconds();

			return (AppId << 48) + ((ulong)time << 16) + ++value;
		}

		public static int GetAppIdFromId(long id)
		{
			return (int)(id >> 48);
		}
	}
}
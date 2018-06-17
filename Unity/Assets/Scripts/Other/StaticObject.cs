namespace BK
{
	public static class StaticObject<T> where T : class, new()
	{
		public static T Instance = new T();
	}
}
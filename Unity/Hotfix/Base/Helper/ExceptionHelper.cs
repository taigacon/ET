using System;
using BK;

namespace BKHotfix
{
	public static class ExceptionHelper
	{
		public static string ToStr(this Exception exception)
		{
#if ILRuntime
			return $"{exception.Data["StackTrace"]} \n\n {exception}";
#else
			return exception.ToString();
#endif
		}
	}
}
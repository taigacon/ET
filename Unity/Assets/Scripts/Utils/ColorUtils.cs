using UnityEngine;

namespace BK
{
	public static class ColorUtils
	{
		public static Color IntToColor(int rgba)
		{
			byte r = (byte)((rgba & 0xff000000) >> 24);
			byte g = (byte)((rgba & 0x00ff0000) >> 16);
			byte b = (byte)((rgba & 0x0000ff00) >> 8);
			byte a = (byte)((rgba & 0x000000ff) >> 0);
			return new Color32(r, g, b, a);
		}
	}

}
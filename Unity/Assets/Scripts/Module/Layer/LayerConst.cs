using UnityEngine;

namespace BK
{
	public static class LayerConst
	{
		/// <summary>
		/// UI层
		/// </summary>
		public static int UI = LayerMask.NameToLayer(LayerNames.UI);

		/// <summary>
		/// 游戏单位层
		/// </summary>
		public static int UNIT = LayerMask.NameToLayer(LayerNames.UNIT);

		/// <summary>
		/// 地形层
		/// </summary>
		public static int MAP = LayerMask.NameToLayer(LayerNames.MAP);

		/// <summary>
		/// 默认层
		/// </summary>
		public static int DEFAULT = LayerMask.NameToLayer(LayerNames.DEFAULT);
	}
}
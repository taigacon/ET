using UnityEngine;

namespace BK.UIBind
{
	public class UIDataCell
	{
		public string Key { get; }
		public DataSet DataSet { get; }
		public object Param { get; }
		public GameObject GameObject { get; }
		public object Value { get; set; }
		public object CustomData { get; set; }

		public UIDataCell(string key, DataSet dataSet, object param, GameObject gameObject)
		{
			this.Key = key;
			this.DataSet = dataSet;
			this.Param = param;
			this.GameObject = gameObject;
		}
	}
}
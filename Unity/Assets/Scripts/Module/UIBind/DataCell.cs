namespace ETModel.UIBind
{
	public interface IDataCell
	{
		string Key { get; }
		DataSet DataSet { get; }
		object Param { get; }
		object Value { get; set; }
	}

	public class DataCell : IDataCell
	{
		public string Key { get; }
		public DataSet DataSet { get; }
		public object Param { get; }
		public object Value { get; set; }

		public DataCell(string key, DataSet dataSet, object param)
		{
			this.Key = key;
			this.DataSet = dataSet;
			this.Param = param;
		}
	}

	public class DataCell<V>: DataCell
	{
		public DataCell(string key, DataSet dataSet, object param)
				: base(key, dataSet, param)
		{
		}

		public new V Value  => (V) base.Value;
	}

	public class DataCell<V, P> : DataCell<V>
	{
		public DataCell(string key, DataSet dataSet, object param)
				: base(key, dataSet, param)
		{
		}

		public new P Param => (P) base.Param;
	}
}
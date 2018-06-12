using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ETModel.UIBind
{
	public interface IUIBindArrayData
	{
		ReadOnlyCollection<DataSet> Data { get; }
		DataSet this[int index] { get; }
		void RefreshData(IList<object> data);
		void BindUIArray(BaseUIBindArray array);
		void UnbindUIArray(BaseUIBindArray array);
	}

	public class UIBindArrayData<T> : IUIBindArrayData
		where T : DataSet, new()
	{
		private readonly List<BaseUIBindArray> bindArrays = new List<BaseUIBindArray>();
		ReadOnlyCollection<DataSet> IUIBindArrayData.Data => Array.AsReadOnly((DataSet[])this.Data);
		public void BindUIArray(BaseUIBindArray array)
		{
			this.bindArrays.Add(array);
		}

		public void UnbindUIArray(BaseUIBindArray array)
		{
			this.bindArrays.Remove(array);
		}

		public T[] Data { get; private set; } = new T[0];

		public void RefreshData(IList<object> data)
		{
			ResizeArray(data.Count);
			NotifyUIBindArrayAdjustArrayLength(data.Count);
		}

		private void ResizeArray(int size)
		{
			int oldCount = this.Data.Length;
			if (size != oldCount)
			{
				T[] data = this.Data;
				Array.Resize(ref data, size);
				for (int i = oldCount; i < size; i++)
				{
					data[i] = new T();
				}
			}
		}

		private void NotifyUIBindArrayAdjustArrayLength(int newCount)
		{
			foreach (var bindArray in this.bindArrays)
			{
				bindArray.AdjustArrayLength(newCount);
			}
		}

		public DataSet this[int index] => this.Data[index];
	}
}
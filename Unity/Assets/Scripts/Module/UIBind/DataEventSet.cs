using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ETModel.Reflection;

namespace ETModel.UIBind
{

	public class DataEventSet : IDisposable
	{
		public delegate void Handler(DataCell cell);
		public delegate void Handler<V>(DataCell<V> cell);
		public delegate void Handler<V, P>(DataCell<V, P> cell);

		private readonly List<ICallback> callbacks = new List<ICallback>();

		public int BindHandler<T>(T dataSet, Expression<Func<T, object>> property, Handler handler, object param = null)
			where T : DataSet
		{
			string key = dataSet.GetPropertyName(property);
			return BindHandler(dataSet, key, handler, param);
		}

		public int BindHandler<T, V>(T dataSet, Expression<Func<T, object>> property, Handler<V> handler, object param = null)
				where T : DataSet
		{
			string key = dataSet.GetPropertyName(property);
			return BindHandler(dataSet, key, handler, param);
		}

		public int BindHandler<T, V, P>(T dataSet, Expression<Func<T, object>> property, Handler<V, P> handler, P param)
				where T : DataSet
		{
			string key = dataSet.GetPropertyName(property);
			return BindHandler(dataSet, key, handler, param);
		}

		public int BindHandler(DataSet dataSet, string key, Handler handler, object param = null)
		{
			DataCell cell = new DataCell(key, dataSet, param);
			Callback callback = new Callback(handler, cell);
			return BindHandler(dataSet, key, callback);
		}

		public int BindHandler<V>(DataSet dataSet, string key, Handler<V> handler, object param = null)
		{
			DataCell<V> cell = new DataCell<V>(key, dataSet, param);
			Callback<V> callback = new Callback<V>(handler, cell);
			return BindHandler(dataSet, key, callback);
		}

		public int BindHandler<V, P>(DataSet dataSet, string key, Handler<V, P> handler, P param)
		{
			DataCell<V, P> cell = new DataCell<V, P>(key, dataSet, param);
			Callback<V, P> callback = new Callback<V, P>(handler, cell);
			return BindHandler(dataSet, key, callback);
		}

		public void UnbindHandler(int handle)
		{
			handle--;
			if (handle >= 0 && handle < this.callbacks.Count)
			{
				ICallback callback = this.callbacks[handle];
				if (callback != null)
				{
					callback.Cell.DataSet.RemoveListener(callback.Cell.Key, callback.Run);
				}

				this.callbacks[handle] = null;
			}
		}

		public void Dispose()
		{
			foreach (ICallback callback in this.callbacks)
			{
				if (callback != null)
				{
					callback.Cell.DataSet.RemoveListener(callback.Cell.Key, callback.Run);
				}
			}
			this.callbacks.Clear();
		}

		private int BindHandler(DataSet dataSet, string key, ICallback callback)
		{
			dataSet.AddListener(key, callback.Run);
			callbacks.Add(callback);
			return callbacks.Count;
		}

		private interface ICallback
		{
			IDataCell Cell { get; }
			void Run(object value);
		}

		private class Callback: ICallback
		{
			private readonly Handler handler;
			private readonly DataCell cell;

			public Callback(Handler handler, DataCell cell)
			{
				this.handler = handler;
				this.cell = cell;
			}

			public IDataCell Cell => this.cell;

			public void Run(object value)
			{
				Cell.Value = value;
				this.handler.Invoke(this.cell);
			}
		}

		private class Callback<V> : ICallback
		{
			private readonly Handler<V> handler;
			private readonly DataCell<V> cell;

			public Callback(Handler<V> handler, DataCell<V> cell)
			{
				this.handler = handler;
				this.cell = cell;
			}

			public IDataCell Cell => this.cell;

			public void Run(object value)
			{
				Cell.Value = value;
				this.handler.Invoke(this.cell);
			}
		}

		private class Callback<V, P> : ICallback
		{
			private readonly Handler<V, P> handler;
			private readonly DataCell<V, P> cell;

			public Callback(Handler<V, P> handler, DataCell<V, P> cell)
			{
				this.handler = handler;
				this.cell = cell;
			}

			public IDataCell Cell => this.cell;

			public void Run(object value)
			{
				Cell.Value = value;
				this.handler.Invoke(this.cell);
			}
		}
	}
}
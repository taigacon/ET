using System;
using System.Collections.Generic;
using ILRuntime.Other;
using UnityEngine;

namespace BK.UIBind
{
	public class UIBindRoot
	{
		private readonly List<UIDataCellCallback> callbacks = new List<UIDataCellCallback>();
		
		public static Func<string, UIDataCell, bool> OnInvokeBindMethod;

		public GameObject GameObject { get; private set; }
		public UIBindView BindView { get; private set; }

		public UIBindRoot(GameObject gameObject, Type bindViewType)
		{
			this.GameObject = gameObject;
			this.BindView = (UIBindView)Activator.CreateInstance(bindViewType);
			InitBindDatas(gameObject);
		}

		private void InitBindDatas(GameObject gameObject)
		{
			if (gameObject.GetComponent<UIBindRoot>() != null && gameObject != this.GameObject) return;
			//数组下的内容交给数组来注册了
			if (gameObject.GetComponent<UIBindArrayTemplate>() != null)
			{
				return;
			}

			//注册Group
			BaseUIBindGroup bindGroup = gameObject.GetComponent<BaseUIBindGroup>();
			if (bindGroup)
			{
				bindGroup.Init(this);
				return;
			}

			//注册数组
			BaseUIBindArray bindArray = gameObject.GetComponent<BaseUIBindArray>();
			if (bindArray)
			{
				bindArray.Init(this);
			}

			var sNodes = StaticObject<List<BaseUINode>>.Instance;
			//注册事件和绑定
			gameObject.GetComponents<BaseUINode>(sNodes);
			foreach (var component in sNodes)
			{
				component.InitOnRoot(this);
			}
			sNodes.Clear();
			
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				InitBindDatas(gameObject.transform.GetChild(i).gameObject);
			}
		}

		public void AddRefOnRoot(string refName, object value)
		{
			this.BindView.Set(refName, value);
		}

		public void AddRefOnArray(string refName, object value, BaseUIBindArray array, int arrayIndex)
		{
			var dataSet = array.Data[arrayIndex];
			dataSet.Set(refName, value);
		}

		public void AddRefOnGroup(string refName, object value, BaseUIBindGroup group)
		{
			var dataSet = group.DataSet;
			dataSet.Set(refName, value);
		}

		public int CreateCellOnArray(GameObject gameObject, string bindFuncName, string bindDataName, BaseUIBindArray array, int index, object param)
		{
			var dataSet = array.Data[index];
			UIDataCell cell = new UIDataCell(bindDataName, dataSet, param, gameObject);
			UIDataCellCallback callback = new UIDataCellCallback(this, bindFuncName, cell);
			dataSet.AddListener(bindDataName, callback.Run);
			this.callbacks.Add(callback);
			return this.callbacks.Count;
		}

		public int CreateCellOnRoot(GameObject gameObject, string bindFuncName, string bindDataName, object param)
		{
			var dataSet = this.BindView;
			UIDataCell cell = new UIDataCell(bindDataName, dataSet, param, gameObject);
			UIDataCellCallback callback = new UIDataCellCallback(this, bindFuncName, cell);
			dataSet.AddListener(bindDataName, callback.Run);
			this.callbacks.Add(callback);
			return this.callbacks.Count;
		}

		public int CreateCellOnGroup(GameObject gameObject, string bindFuncName, string bindDataName, BaseUIBindGroup group, object param)
		{
			var dataSet = group.DataSet;
			UIDataCell cell = new UIDataCell(bindDataName, dataSet, param, gameObject);
			UIDataCellCallback callback = new UIDataCellCallback(this, bindFuncName, cell);
			dataSet.AddListener(bindDataName, callback.Run);
			this.callbacks.Add(callback);
			return this.callbacks.Count;
		}

		public void DestroyCell(int handle)
		{
			handle--;
			if (handle >= 0 && handle < this.callbacks.Count)
			{
				var callback = this.callbacks[handle];
				if (callback != null)
				{
					callback.Cell.DataSet.RemoveListener(callback.Cell.Key, callback.Run);
				}

				this.callbacks[handle] = null;
			}
		}

		private void InvokeBindMethod(string bindFuncName, UIDataCell cell)
		{
			if (this.BindView.InvokeBindMethod(bindFuncName, cell))
				return;
			if (OnInvokeBindMethod != null && OnInvokeBindMethod.Invoke(bindFuncName, cell))
				return;
			if (UIBindUtils.InvokeBindMethod(bindFuncName, cell))
				return;
			throw new Exception($"Bind Function Not Found: ${bindFuncName}");
		}

#if UNITY_EDITOR
		public string GetRuntimeValue(int tableIndex)
		{
			return null;
		}
#endif

		public bool IsDisposed { get; private set; } = false;

		public void Dispose()
		{
			this.IsDisposed = true;
			foreach (var callback in this.callbacks)
			{
				if (callback != null)
				{
					callback.Cell.DataSet.RemoveListener(callback.Cell.Key, callback.Run);
				}
			}
			this.callbacks.Clear();
			this.GameObject = null;
			this.BindView = null;
		}

		private class UIDataCellCallback
		{
			private UIBindRoot Root { get; }
			private string BindFuncName { get; }
			public UIDataCell Cell { get; }
			public UIDataCellCallback(UIBindRoot root, string bindFuncName, UIDataCell cell)
			{
				this.Root = root;
				this.BindFuncName = bindFuncName;
				this.Cell = cell;
			}

			public void Run(object value)
			{
				this.Cell.Value = value;
				this.Root.InvokeBindMethod(this.BindFuncName, this.Cell);
			}
		}
	}
}
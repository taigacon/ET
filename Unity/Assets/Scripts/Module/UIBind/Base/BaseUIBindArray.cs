using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel.UIBind
{
    //UI绑定数组，负责Template的创建销毁，底下BindCell的数据初始化
    public class BaseUIBindArray : BaseUIBindContainer
    {
        public UIBindArrayTemplate template;
#if UNITY_EDITOR
        private UIBindArrayTemplate oldTemplate;
#endif
		[NonSerialized]
        public List<GameObject> bindItems = new List<GameObject>();
        public bool ForceReposition = false;
        public bool ResetScrollView = false;
	    public IUIBindArrayData Data { get; private set; }

	    protected override void Init(DataSet container)
	    {
		    this.Data = container.Get<IUIBindArrayData>(this.bindDataName);
			this.Data.BindUIArray(this);

			//隐藏Template的GameObject，理论上这个template不能为null所以不检查了（
		    template.gameObject.SetActive(false);
		}

	    public void AdjustArrayLength(int length)
        {
            bool needSort = bindItems.Count != length;
            int oldLength = bindItems.Count;
            //不足时，创建新的template
            for (int i = bindItems.Count; i < length; i++)
            {
	            var go = GameObject.Instantiate(this.template.gameObject);
				go.transform.SetParent(this.gameObject.transform, false);
                go.SetActive(true);
                bindItems.Add(go);
                InitBindDatas(go.transform, i + 1, go.transform);
            }
            if (bindItems.Count - length > 0)
            {
                //数量过多时，删除多余的template
                for (int i = length; i < bindItems.Count; i++)
                {
                    DestroyImmediate(bindItems[i]);
                }
                bindItems.RemoveRange(length, bindItems.Count - length);
            }
            //给各个位置排序
            if (ForceReposition || needSort)
            {
                ReSortPosition(oldLength, length);
            }
        }

        private void InitBindDatas(Transform transform, int index, Transform rootTrans)
        {
            if (transform.GetComponent<UIBindRoot>() != null) return;
            if (transform != rootTrans && transform.GetComponent<UIBindArrayTemplate>() != null) return;

	        var sNodes = StaticObject<List<BaseUINode>>.Instance;
			transform.GetComponents<BaseUINode>(sNodes);
            foreach (var component in sNodes)
            {
                component.InitOnArray(root, this, index);
            }
            sNodes.Clear();

            //注册数组
            BaseUIBindArray bindArray = transform.GetComponent<BaseUIBindArray>();
            if (bindArray)
            {
                bindArray.Init(this.root, this, index);
            }

            //注册Group
            BaseUIBindGroup bindGroup = transform.GetComponent<BaseUIBindGroup>();
            if (bindGroup)
            {
                bindGroup.Init(this.root, this, index);
                return;
            }

            //防止调不到OnDestroy
            for (int i = 0; i < transform.childCount; i++)
            {
                InitBindDatas(transform.GetChild(i), index, rootTrans);
            }
        }
        
        public int GetIndexByObject(GameObject obj)
        {
            for (int i = 0; i < bindItems.Count; i++)
            {
                if (bindItems[i] == obj)
                    return i;
            }
            return -1;
        }

        protected virtual void ReSortPosition(int oldLength, int newLength)
        {

        }

	    protected override DataSet GetContainer(int arrayIndex = -1)
	    {
		    return this.Data.Data[arrayIndex];
	    }

	    public void ReSort()
        {
            ReSortPosition(bindItems.Count, bindItems.Count);
        }

#if UNITY_EDITOR
        void OnReset()
        {
            oldTemplate = null;
        }

        void OnValidate() 
        {
            if (oldTemplate != null)
            {
                oldTemplate.BindArray = null;
            }
            if (template != null)
            {
                template.BindArray = this;
            }
            oldTemplate = template;
        }
#endif
    }
}

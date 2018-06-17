using System.Collections.Generic;
using UnityEngine;

namespace BK.UIBind
{
    public abstract class BaseUIBindGroup : BaseUIBindContainer
	{
		public DataSet DataSet { get; private set; }
		protected override void Init(DataSet container)
		{
			this.DataSet = container.Get<DataSet>(this.bindDataName);

			InitBindDatas(this.transform);
		}

		protected override DataSet GetContainer(int arrayIndex = -1)
		{
			return this.DataSet;
		}

		private void InitBindDatas(Transform transform, BaseUIBindContainer parent = null, int arrayIndex = 0)
        {
            if (transform.GetComponent<UIBindRoot>() != null && transform != this.transform) return;
            if (transform.GetComponent<UIBindArrayTemplate>() != null) return;

	        var sNodes = StaticObject<List<BaseUINode>>.Instance;
			transform.GetComponents<BaseUINode>(sNodes);
            foreach (var component in sNodes)
            {
                component.InitOnGroup(root, this, arrayIndex);
            }
            sNodes.Clear();

            //注册数组
            BaseUIBindArray bindArray = transform.GetComponent<BaseUIBindArray>();
            if (bindArray)
            {
                bindArray.Init(this.root, this, arrayIndex);
            }

            //注册Group
            BaseUIBindGroup bindGroup = transform.GetComponent<BaseUIBindGroup>();
            if (bindGroup && bindGroup != this)
            {
                bindGroup.Init(this.root, this, arrayIndex);
                return;
            }
			
            for (int i = 0; i < transform.childCount; i++)
            {
                InitBindDatas(transform.GetChild(i), parent, arrayIndex);
            }
        }
    }
}

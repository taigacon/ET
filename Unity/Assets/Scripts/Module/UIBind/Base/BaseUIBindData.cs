using System;
using System.Collections;
using UnityEngine;

namespace BK.UIBind
{
    public abstract class BaseUIBindData : BaseUINode
    {
        public string bindDataName;
        internal abstract string GetBindFunctionName();

        private UIBindRoot root = null;
        private int cellIndex = 0;

        //额外参数
        protected virtual object GetParam()
        {
	        return null;
        }

        //初始化，事件注册在Array下
        public override void InitOnArray(UIBindRoot root, BaseUIBindArray array, int index)
        {
            this.root = root;
	        cellIndex = root.CreateCellOnArray(this.gameObject, this.GetBindFunctionName(), this.bindDataName, array, index, this.GetParam());
		}

        //初始化，事件注册在root下
        public override void InitOnRoot(UIBindRoot root)
        {
            this.root = root;
			cellIndex = root.CreateCellOnRoot(this.gameObject, this.GetBindFunctionName(), this.bindDataName, this.GetParam());
		}

        //初始化，事件注册在root下
        public override void InitOnGroup(UIBindRoot root, BaseUIBindGroup group, int arrayIndex)
        {
            this.root = root;
	        cellIndex = root.CreateCellOnGroup(this.gameObject, this.GetBindFunctionName(), this.bindDataName, group, this.GetParam());
		}

        void OnDestroy()
        {
            if (this.cellIndex > 0)
            {
                //解一下事件
                if(this.root != null && !this.root.IsDisposed)
                {
                    this.root.DestroyCell(this.cellIndex);
                }
            }
        }

#if UNITY_EDITOR
        public string runtimeValue
        {
            get
            {
                if (Application.isPlaying && this.root != null && !this.root.IsDisposed)
                {
	                return this.root.GetRuntimeValue(this.cellIndex);
                }
                return null;
            }
        }

#endif
    }
}

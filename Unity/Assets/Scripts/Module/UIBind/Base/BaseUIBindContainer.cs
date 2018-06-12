using System;
using UnityEngine;

namespace ETModel.UIBind
{
    public abstract class BaseUIBindContainer : MonoBehaviour
    {
        public string bindDataName;
        protected UIBindRoot root = null;

	    public void Init(UIBindRoot root, BaseUIBindContainer parent = null, int arrayIndex = -1)
	    {
		    this.root = root;

		    DataSet container = this.GetContainer(parent, arrayIndex);
		    Init(container);
		}

	    protected abstract void Init(DataSet container);

	    private DataSet GetContainer(BaseUIBindContainer parent = null, int arrayIndex = -1)
	    {
		    if (parent != null)
		    {
			    return parent.GetContainer(arrayIndex);
		    }

		    return this.root.BindView;
	    }

	    protected abstract DataSet GetContainer(int arrayIndex = -1);
    }
}

namespace ETModel.UIBind
{
    public abstract class BaseUIReference : BaseUINode
    {
	    public string refName;

        public override void InitOnArray(UIBindRoot root, BaseUIBindArray array, int index)
        {
            root.AddRefOnArray(this.refName, this.GetRefObject(), array, index);
        }

        public override void InitOnRoot(UIBindRoot root)
		{
			root.AddRefOnRoot(this.refName, this.GetRefObject());
		}

        public override void InitOnGroup(UIBindRoot root, BaseUIBindGroup group, int arrayIndex)
        {
			root.AddRefOnGroup(this.refName, this.GetRefObject(), group);
		}

	    protected abstract object GetRefObject();
    }
}

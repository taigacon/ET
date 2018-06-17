using UnityEngine;

namespace BK.UIBind
{
    //数据绑定和发送事件的节点
    public abstract class BaseUINode : MonoBehaviour
    {
        public virtual void InitOnArray(UIBindRoot root, BaseUIBindArray array, int index)
        {
            
        }

        public virtual void InitOnRoot(UIBindRoot root)
        {

        }
        public virtual void InitOnGroup(UIBindRoot root, BaseUIBindGroup group, int arrayIndex)
        {

        }
    }
}

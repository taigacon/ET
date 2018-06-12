using UnityEngine;
using System;

namespace ETModel.UIBind
{
    public class UIBindArrayTemplate : MonoBehaviour
    {
#if UNITY_EDITOR
        [NonSerialized]
        public BaseUIBindArray BindArray;
#endif
    }
}

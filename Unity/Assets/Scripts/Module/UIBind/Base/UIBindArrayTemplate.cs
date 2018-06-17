using UnityEngine;
using System;

namespace BK.UIBind
{
    public class UIBindArrayTemplate : MonoBehaviour
    {
#if UNITY_EDITOR
        [NonSerialized]
        public BaseUIBindArray BindArray;
#endif
    }
}

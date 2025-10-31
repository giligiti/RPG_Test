using UnityEngine;

namespace InteractSystem
{
    /// <summary>
    /// 所有需要产生交互的物体都继承这个类
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        [SerializeField]
        protected InteractInterfaceHandle interfaceInfo;
        public InteractInterfaceHandle InterfaceInfo => interfaceInfo;
        protected virtual void Awake()
        {
            interfaceInfo = new(this.gameObject);
        }
    }
}
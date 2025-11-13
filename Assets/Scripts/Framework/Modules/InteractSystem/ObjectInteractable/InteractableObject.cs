using Framework;
using UnityEngine;

namespace InteractSystem
{
    /// <summary>
    /// 所有需要产生交互的物体都继承这个类
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        [SerializeField]
        protected InteractInterfaceHandle interfaceHandle;
        public InteractInterfaceHandle InterfaceHandle => interfaceHandle;
        protected virtual void Awake()
        {
            // interfaceHandle = new(this.gameObject);
            // //把接口对象注册到单例管理器中
            // GameCore.InteractMgr.RegisterHandle(this);
        }
        protected virtual void Start()
        {
            interfaceHandle = new(this.gameObject);
            //把接口对象注册到单例管理器中
            GameCore.InteractMgr.RegisterHandle(this);
        }
    }
}
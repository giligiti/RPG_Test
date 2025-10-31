using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractSystem
{
    /// <summary>
    /// 可交互对象的封装类
    /// </summary>
    public class InteractInterfaceHandle
    {
        // 用字典存储“接口类型→实例”，方便查询
        private Dictionary<Type, IInteractableBase> _interfaces = new Dictionary<Type, IInteractableBase>();
        public GameObject obj;

        public InteractInterfaceHandle(GameObject gameObject)
        {
            // 反射获取物体上所有实现了IInteractableBase的接口
            obj = gameObject;
            var components = gameObject.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                var interfaces = comp.GetType().GetInterfaces()
                    .Where(t => typeof(IInteractableBase).IsAssignableFrom(t));
                foreach (var inter in interfaces)
                {
                    _interfaces[inter] = (IInteractableBase)comp;
                }
            }
        }
        
        
        public bool TryGetInterface<T>(out T itf) where T : IInteractableBase
        {
            if (_interfaces.TryGetValue(typeof(T), out var inter))
            {
                itf = (T)inter;
                return true;
            }
            itf = default(T);
            return false;
        }
    }
    /// <summary>
    /// 交互物体接口的基类接口
    /// </summary>
    public interface IInteractableBase { }

    /// <summary>
    /// 会弹出交互UI提示的物体（能被检测到但是尚未开始交互的状态）
    /// </summary>
    public interface IOnInteractable : IInteractableBase
    {
        /// <summary>
        /// 检查当前交互对象是否可交互（如是否在冷却、是否满足条件）
        /// </summary>
        bool CanInteract { get; }
        /// <summary>
        /// 提供提示UI配置文件
        /// </summary>
        /// <returns></returns>
        InteractTipSO ProvideConfig();
    }
    /// <summary>
    /// 可交互物体的开始交互接口
    /// </summary>
    public interface IOnInteractStart : IInteractableBase
    {
        /// <summary>
        /// 检查当前交互对象是否可交互（如是否在冷却、是否满足条件）
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// 执行开始交互逻辑
        /// </summary>
        /// <param name="interactor"></param>
        void OnInteractStart(MainInteractor interactor);
        /// <summary>
        /// 内部验证（可选）：接收输入上下文，由物体自己判断是否符合触发条件（如某些物体需要长按，可在此处判断按压时长）
        /// </summary>
        /// <param name="context">输入上下文（包含按压状态、时长等）</param>
        /// <returns>是否满足内部输入条件</returns>
        bool ValidateInput(UnityEngine.InputSystem.InputAction.CallbackContext context); // 默认返回true
    }
    /// <summary>
    /// 可交互物体的停止交互接口
    /// </summary>
    public interface IOnInteractStop : IInteractableBase
    {
        /// <summary>
        /// 执行停止交互逻辑
        /// </summary>
        /// <param name="interactor"></param>
        void OnInteractStop(MainInteractor interactor);

    }
    /// <summary>
    /// 可以被选中的物体的接口
    /// </summary>
    public interface ISelectable : IInteractableBase
    {
        //选中的时候
        void OnSelect();
        //未选中的时候
        void OnLoseSelect();
    }
}
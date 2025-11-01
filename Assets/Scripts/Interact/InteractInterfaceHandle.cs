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
            //并且提前注册到交互管理器的字典中存储
            gameObject.GetInstanceID();
        }
        
        
        public bool TryGetInterface<T>(out T itf) where T : class, IInteractableBase
        {
            //如果物体为空（已经被销毁）就让外界无法获取接口并返回false
            if (obj == null)
            {
                itf = null;
                return false;
            }
            if (_interfaces.TryGetValue(typeof(T), out var inter))
            {
                itf = (T)inter;
                return true;
            }
            itf = default(T);
            return false;
        }
    }
}
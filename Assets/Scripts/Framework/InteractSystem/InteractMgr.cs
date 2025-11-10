using System;
using System.Collections.Generic;
using ToolSpace;
using UnityEngine;

namespace InteractSystem
{
    public class InteractMgr : BaseManager<InteractMgr>, IDisposable
    {
        private readonly Dictionary<int, InteractableObject> objToInteractDic;
        /// <summary>
        /// 子级物体 - 父级物体 的映射
        /// </summary>
        private LRUCache<int, InteractableObject> LRUCache;
        private InteractMgr()
        {
            objToInteractDic = new();
            LRUCache = new(100);
        }

        public void Dispose()
        {
            LRUCache.Clear();
            objToInteractDic.Clear();
        }

        /// <summary>
        /// 把交互接口对象注册到管理器中，方便查询
        /// 物体不为空且对象不为空才会被注册
        /// </summary>
        /// <param name="objInstanceID">物体的InsstanceID</param>
        /// <param name="handle">交互接口对象</param>
        public void RegisterHandle(InteractableObject interactObj)
        {
            if (interactObj == null) return;
            if (interactObj != null && interactObj.InterfaceHandle != null)
            {
                int objInstanceID = interactObj.gameObject.GetInstanceID();
                objToInteractDic.Add(objInstanceID, interactObj);
            }
            else UnityEngine.Debug.Log("交互接口对象注册失败");
        }
        /// <summary>
        /// 获取这个物体的交互接口对象
        /// </summary>
        /// <param name="objInstanceID">GameObject的InstanceID(obj.GetInstanceID())</param>
        /// <param name="handle">交互接口对象</param>
        /// <returns></returns>
        public bool TryGetInteractHandle(GameObject obj, out InteractableObject interactObj)
        {
            int objInstanceID = obj.GetInstanceID();

            if (LRUCache.TryGetValue(objInstanceID, out interactObj)) return true;

            if (objToInteractDic.TryGetValue(objInstanceID, out interactObj)) return true;

            interactObj = GetInteractInParent(obj.transform);
            // 无论是否找到都注册到LRU中，免得重复查找
            LRUCache.Put(objInstanceID, interactObj);

            return interactObj;
        }

        /// <summary>
        /// 在父母身上查找是否有对应映射（包括自己）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private InteractableObject GetInteractInParent(Transform obj)
        {
            Transform current = obj;
            InteractableObject inObj;
            while (current != null)
            {
                if (objToInteractDic.TryGetValue(current.gameObject.GetInstanceID(), out inObj))
                {
                    return inObj;
                }
                current = current.parent;
            }
            return null;
        }
    }

}
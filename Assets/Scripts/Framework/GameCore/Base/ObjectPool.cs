using UnityEngine;

namespace Framework
{
    public abstract class ObjectPool<T> where T : class, new()
    {
        protected Transform father;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="father">用于层次面板上管理该对象池物体的空物体</param>
        public ObjectPool(Transform father)
        {
            this.father = father;
            father.name = nameof(T);
        }
    }
}
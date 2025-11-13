using System.Collections.Generic;

namespace Framework.ToolSpace
{
    /// <summary>
    /// 各种小物体的对象池，无特殊new逻辑只需要实现一个简单的复用对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PoolQuene<T> where T : class, new()
    {
        private Queue<T> valueQueue;

        public PoolQuene(int capacity = 10)
        {
            valueQueue = new(capacity);
        }
        public T GetValue()
        {
            if (!valueQueue.TryDequeue(out T result))
                return new T();
                
            return result;
        }
        public void ReturnBack(T value)
        {
            if (value is IReset resetItem)
                resetItem.ResetSelf();
            valueQueue.Enqueue(value);
        }
    }
}
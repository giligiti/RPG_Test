using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToolSpace
{
    //默认最小堆
    /*优先队列提供方法：
        Clear：清空
        Enqueue 添加
        TryEnquque 尝试添加
        Dequeue 取出
        TryDequeue 尝试取出
        Peek 查看顶部
        TryPeek 尝试查看顶部
        AsList 返回排序后的队列
    */
    public class CustomePriorityQueue<TElement, TPriority> : IEnumerable<TElement> where TPriority : IComparable<TPriority>
    {
        public CustomePriorityQueue()
        {
            heapList = new();
        }
        public CustomePriorityQueue(int length)
        {
            heapList = new(length);
        }

        private readonly List<PriorityNode<TElement, TPriority>> heapList;

        public int Count => heapList.Count;

        //得到父节点索引
        private int Parent(int i) => (i - 1) / 2;
        //得到左节点索引
        private int LeftChild(int i) => 2 * i + 1;
        //得到右节点索引
        private int RightChidld(int i) => 2 * i + 2;

        #region 操作方法

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">排序依据</param>
        /// 把元素添加的列表中，并执行上浮操作
        public void Enqueue(TElement element, TPriority priority)
        {
            PriorityNode<TElement, TPriority> node = new PriorityNode<TElement, TPriority>(element, priority);
            //添加到末尾
            heapList.Add(node);
            //执行上浮操作
            HeapifyShiftUp(heapList.Count - 1);
        }
        /// <summary>
        /// 取出队首元素
        /// </summary>
        public TElement Dequeue()
        {
            if (heapList.Count == 0)
            {
                throw new Exception("队列已成空");
            }
            //得到队首元素
            TElement element = Peek();
            //交换根节点和尾节点
            Swap(0, heapList.Count - 1);
            //移除尾节点
            heapList.RemoveAt(heapList.Count - 1);
            //对根节点进行下沉操作
            HeapifyShiftDown();

            return element;
        }
        /// <summary>
        /// 尝试取出元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">排序依据</param>
        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (!TryPeek(out element, out priority)) return false;
            //交换根节点和尾节点
            Swap(0, heapList.Count - 1);
            //移除尾节点
            heapList.RemoveAt(heapList.Count - 1);
            //对根节点进行下沉操作
            HeapifyShiftDown();

            return true;
        }
        /// <summary>
        /// 添加元素并取出元素
        /// </summary>
        /// <param name="element"></param>
        /// <param name="priority"></param>
        public TElement EnqueueDequeue(TElement element, TPriority priority)
        {
            Enqueue(element, priority);

            return Dequeue();
        }
        /// <summary>
        /// 查看队首元素
        /// </summary>
        /// <returns></returns>
        public TElement Peek()
        {
            return heapList[0].Element;
        }
        /// <summary>
        /// 尝试查看队首元素
        /// </summary>
        public bool TryPeek(out TElement element, out TPriority priority)
        {
            if (heapList.Count == 0)
            {
                element = default(TElement);
                priority = default(TPriority);
                return false;
            }
            PriorityNode<TElement, TPriority> node = heapList[0];
            element = node.Element;
            priority = node.Priority;
            return true;
        }
        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            heapList.Clear();
        }
        /// <summary>
        /// 返回排序后的列表
        /// </summary>
        /// <returns></returns>
        public List<TElement> AsList()
        {
            return heapList.Select(element => element.Element).ToList();
        }
        /// <summary>
        /// 返回的是原来的列表，没有创建新列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<TElement> AsList(List<TElement> list)
        {
            list.Clear();
            foreach (var item in heapList)
            {
                list.AddRange(heapList.Select(element => element.Element));
            }
            return list;
        }

        #endregion

        /// <summary>
        /// 上浮
        /// </summary>
        /// 添加元素后，从队尾进行上浮操作，寻找元素合适位置
        private void HeapifyShiftUp(int index)
        {
            //当找到合适位置了或者为根节点的时候就会跳出循环
            while (index > 0)
            {
                int parentIndex = Parent(index);
                PriorityNode<TElement, TPriority> parent = heapList[parentIndex];
                //进行比较
                int sort = heapList[index].Priority.CompareTo(parent.Priority);
                if (sort >= 0) break;
                //如果比父节点还要小就交换
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        /// <summary>
        /// 下沉
        /// </summary>
        /// 默认对根节点进行下沉操作
        private void HeapifyShiftDown(int index = -1)
        {
            //确定当前节点和边界避免越界
            int length = heapList.Count - 1;
            if (index < 0) index = 0;
            int smallest = index;
            //循环查找需要交换的子节点，分别比较左右节点，记录最小的节点的索引值
            while (true)
            {
                int left = LeftChild(index);
                int right = RightChidld(index);
                //比较左节点
                if (left <= length && heapList[left].Priority.CompareTo(heapList[smallest].Priority) < 0)
                {
                    smallest = left;
                }

                //比较右节点
                if (right <= length && heapList[right].Priority.CompareTo(heapList[smallest].Priority) < 0)
                {
                    smallest = right;
                }

                if (smallest == index) break;
                Swap(smallest, index);
                index = smallest;               //重新赋值，进行下一次循环下沉
            }
        }

        //辅助方法
        private void Swap(int a, int b)
        {
            (heapList[a], heapList[b]) = (heapList[b], heapList[a]);
        }


        #region 迭代器接口实现
        public IEnumerator<TElement> GetEnumerator()
        {
            foreach (var item in heapList)
                yield return item.Element;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    public struct PriorityNode<TElement, TPriority>
    {
        public TElement Element;
        public TPriority Priority;
        public PriorityNode(TElement element, TPriority priority)
        {
            this.Element = element;
            this.Priority = priority;
        }
    }

}

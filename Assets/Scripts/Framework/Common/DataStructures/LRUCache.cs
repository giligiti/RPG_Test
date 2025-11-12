using System.Collections.Generic;

namespace ToolSpace
{
    /// <summary>
    /// LRU缓存：自动淘汰最近最少使用的项目
    /// 适合缓存频繁访问但数量有限的数据
    /// </summary>
    public class LRUCache<K, V>
    {
        // 容量限制
        private readonly int _capacity;

        // 快速查找：键 -> 链表节点
        private readonly Dictionary<K, LinkedListNode<CacheItem>> _map;

        // 访问顺序：最近访问的在头部，最久未访问的在尾部
        private readonly LinkedList<CacheItem> _accessList;

        /// <summary>
        /// 当前缓存数量
        /// </summary>
        public int Count => _map.Count;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 缓存项
        /// </summary>
        private class CacheItem
        {
            public K Key { get; }
            public V Value { get; }

            public CacheItem(K key, V value)
            {
                Key = key;
                Value = value;
            }
        }

        public LRUCache(int capacity)
        {
            _capacity = capacity;
            _map = new Dictionary<K, LinkedListNode<CacheItem>>(capacity);
            _accessList = new LinkedList<CacheItem>();
        }

        /// <summary>
        /// 尝试从缓存获取值
        /// </summary>
        public bool TryGetValue(K key, out V value)
        {
            if (_map.TryGetValue(key, out var node))
            {
                // 命中缓存：将节点移到头部（标记为最近使用）
                _accessList.Remove(node);
                _accessList.AddFirst(node);

                value = node.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        public void Put(K key, V value)
        {
            if (_map.TryGetValue(key, out var existingNode))
            {
                // 键已存在：更新值并移到头部
                _accessList.Remove(existingNode);
                existingNode.Value = new CacheItem(key, value);
                _accessList.AddFirst(existingNode);
            }
            else
            {
                // 新键：检查容量
                if (_map.Count >= _capacity)
                {
                    // 缓存已满：移除最久未使用的（尾部）
                    RemoveLast();
                }

                // 添加新节点到头部
                var newNode = new LinkedListNode<CacheItem>(new CacheItem(key, value));
                _accessList.AddFirst(newNode);
                _map[key] = newNode;
            }
        }

        /// <summary>
        /// 移除最久未使用的项目
        /// </summary>
        private void RemoveLast()
        {
            var lastNode = _accessList.Last;
            if (lastNode != null)
            {
                _map.Remove(lastNode.Value.Key);
                _accessList.RemoveLast();
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            _map.Clear();
            _accessList.Clear();
        }
    }
}

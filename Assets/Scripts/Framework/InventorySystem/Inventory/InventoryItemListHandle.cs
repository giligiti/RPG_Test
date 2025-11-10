using System;
using System.Collections.Generic;
using System.Linq;

namespace InventorySystem
{
    public class InventoryItemListHandle
    {
        // 索引→物品（空格：键存在，值为null；有效物品：键存在，值不为null）
        // 索引从0开始连续，中间无缺失键（空格已填充）
        private readonly Dictionary<int, Item> _itemDict = new();
        // 物品GUID→对应所有索引（包括空格索引，删除时同步清理）
        private readonly Dictionary<string, List<int>> _itemIndexDict = new();
        // 空索引列表（仅存储「键存在且值为null」的索引，优先复用）
        private readonly SortedSet<int> _emptyIndexes = new();

        #region 核心对外方法
        /// <summary>
        /// 索引器：直接访问物品（空格返回null，有效物品返回实例）
        /// </summary>
        public Item this[int index]
        {
            get => _itemDict.TryGetValue(index, out var item) ? item : null;
            private set
            {
                // 赋值时确保键存在（空格也保留键，维持索引连续）
                if (!_itemDict.ContainsKey(index))
                    _itemDict.Add(index, value);
                else
                    _itemDict[index] = value;
            }
        }
        // ------------- 添加物体 -------------
        /// <summary>
        /// 添加物品（优先复用空索引，无空索引则用「最大索引+1」）
        /// 注意：该方法会寻找空的索引来存储，而不是存储在现有的非空未满的索引所以外界需要自行先实现存储到目前的已经存在的索引当中
        /// 需要寻找新索引再使用这个方法
        /// </summary>
        public int AddItem(Item item)
        {
            if (item == null || string.IsNullOrEmpty(item.Guid))
                throw new ArgumentException("物品或物品GUID不能为空");

            int targetIndex;

            // 1. 优先复用空索引（O(1)，无需遍历）
            if (_emptyIndexes.Count > 0)
            {
                targetIndex = _emptyIndexes.Min;
                _emptyIndexes.Remove(targetIndex);
            }
            // 2. 无空索引：直接用「最大索引+1」（Count-1是最大索引）
            else
            {
                targetIndex = GetMaxIndex() + 1;
                // 确保目标索引键存在（值为null，后续覆盖）
                if (!_itemDict.ContainsKey(targetIndex))
                    _itemDict.Add(targetIndex, null);
            }

            // 3. 赋值并同步所有结构
            this[targetIndex] = item;
            SyncGuidToIndexesDict(item.Guid, targetIndex, true);

            return targetIndex;
        }

        /// <summary>
        /// 手动指定索引添加物品（自动填充「当前最大索引+1」到「指定索引-1」的空格）
        /// </summary>
        public bool AddItemWithIndex(int targetIndex, Item item)
        {
            if (item == null || string.IsNullOrEmpty(item.Guid) || IsIndexOccupied(targetIndex))
                return false;

            // 1. 填充中间空格（确保从0到targetIndex的键都存在）
            FillEmptySpacesUpTo(targetIndex);

            // 2. 处理目标索引（移除空索引列表，覆盖为有效物品）
            _emptyIndexes.Remove(targetIndex);
            this[targetIndex] = item;
            SyncGuidToIndexesDict(item.Guid, targetIndex, true);

            return true;
        }

        // ------------- 删除物品 -------------
        /// <summary>
        /// 按索引直接删除该索引的物品（不删除键，仅设为null，重新加入空索引列表）
        /// </summary>
        public bool DeleteItemByIndex(int index)
        {
            if (!_itemDict.ContainsKey(index))
                return false;

            var oldItem = this[index];
            // 1. 保留键，设为null（维持索引连续）
            this[index] = null;
            // 2. 重新加入空索引列表（后续可复用）
            if (!_emptyIndexes.Contains(index))
                _emptyIndexes.Add(index);
            // 3. 同步GUID索引列表（仅有效物品需要清理）
            if (oldItem != null)
                SyncGuidToIndexesDict(oldItem.Guid, index, false);

            return true;
        }

        /// <summary>
        /// 按物品实例删除（删除所有同种物体的所有实例）
        /// </summary>
        public bool DeleteAllAssignItem(Item item)
        {
            if (item == null || !_itemIndexDict.TryGetValue(item.Guid, out var indexes))
                return false;

            foreach (var index in indexes.ToList())
            {
                if (_itemDict.TryGetValue(index, out var targetItem) && targetItem == item)
                {
                    return DeleteItemByIndex(index);
                }
            }
            return false;
        }

        // ------------- 更新物品（覆盖值，不改变索引）-------------
        public bool UpdateItem(int index, Item newItem)
        {
            if (newItem == null || string.IsNullOrEmpty(newItem.Guid) || !_itemDict.ContainsKey(index))
                return false;

            var oldItem = this[index];
            // 1. 同步GUID索引列表（清理旧的，添加新的）
            if (oldItem != null)
                SyncGuidToIndexesDict(oldItem.Guid, index, false);
            SyncGuidToIndexesDict(newItem.Guid, index, true);

            // 2. 覆盖值（从空索引列表移除，标记为有效物品）
            _emptyIndexes.Remove(index);
            this[index] = newItem;

            return true;
        }
        #endregion

        #region 查询方法
        /// <summary>
        /// 获取当前最大索引（直接用Count-1，O(1)，支持UI高频调用）
        /// 空字典时返回-1，逻辑一致
        /// </summary>
        public int GetMaxIndex() => _itemDict.Count - 1;

        /// <summary>
        /// 获取有效物品总数（过滤null值）
        /// </summary>
        public int GetValidItemCount() => _itemDict.Values.Count(item => item != null);

        /// <summary>
        /// 按GUID获取所有有效物品（自动过滤null）
        /// </summary>
        public List<Item> GetItemsByGuid(string guid)
        {
            if (!_itemIndexDict.TryGetValue(guid, out var indexes))
                return new List<Item>();

            return indexes
                .Select(index => this[index])
                .Where(item => item != null)
                .ToList();
        }

        /// <summary>
        /// 按GUID获取所有索引（包括空格索引，外部可自行过滤）
        /// </summary>
        public List<int> GetIndexesByGuid(string guid)
        {
            _itemIndexDict.TryGetValue(guid, out var indexes);
            return indexes ?? new List<int>();
        }

        /// <summary>
        /// 得到对应物体的总数
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public int GetItemCountByGuid(string guid)
        {
            return _itemIndexDict.TryGetValue(guid, out var indexes) ? indexes.Sum(i => this[i]?.amount ?? 0) : 0;
        }

        /// <summary>
        /// 检查索引是否已被占用（true ：键存在且值不为null）
        /// </summary>
        public bool IsIndexOccupied(int index) => _itemDict.TryGetValue(index, out var item) && item != null;
        #endregion

        #region 内部辅助方法
        /// <summary>
        /// 填充「当前最大索引+1」到「targetIndex」之间的空格（创建null键+加入空索引）
        /// 确保索引连续，无缺失键
        /// </summary>
        private void FillEmptySpacesUpTo(int targetIndex)
        {
            int currentMaxIndex = GetMaxIndex();
            if (targetIndex <= currentMaxIndex)
                return;

            // 遍历填充中间空格，创建null键并加入空索引列表
            for (int i = currentMaxIndex + 1; i <= targetIndex; i++)
            {
                _itemDict.Add(i, null);
                _emptyIndexes.Add(i);
            }
        }

        /// <summary>
        /// 同步GUID→索引列表（添加/移除索引，确保数据一致）
        /// </summary>
        private void SyncGuidToIndexesDict(string guid, int index, bool isAdd)
        {
            if (!_itemIndexDict.ContainsKey(guid))
            {
                if (!isAdd) return;
                _itemIndexDict[guid] = new List<int>();
            }

            var indexes = _itemIndexDict[guid];
            if (isAdd)
            {
                if (!indexes.Contains(index))
                    indexes.Add(index);
            }
            else
            {
                indexes.Remove(index);
                if (indexes.Count == 0)
                    _itemIndexDict.Remove(guid);
            }
        }

        #endregion
    }
}
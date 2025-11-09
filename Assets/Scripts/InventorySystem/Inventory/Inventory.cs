using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// 物品仓库对外接口类（内部依赖 InventoryItemListHandle 维护数据，仅保留 _onUpdate 事件）
    /// </summary>
    public class Inventory
    {
        // 内部数据核心管理器（复用之前封装好的逻辑）
        private readonly InventoryItemListHandle _itemHandle = new InventoryItemListHandle();

        // 更新事件：传递需要刷新的索引列表（触发后清空，无残留）
        private event Action<IEnumerable<int>> _onUpdate;
        // 待更新索引列表（避免事件残留）
        private readonly LinkedList<int> _eventIndexList;       //用来存储发生变化的物体索引
        private readonly E_InventoryPlace inventoryPlace;       //表示当前仓库对象所代表的具体仓库
        #region 构造函数（初始化待更新索引列表）
        public Inventory(E_InventoryPlace place)
        {
            _eventIndexList = new LinkedList<int>();
            inventoryPlace = place;
        }
        #endregion

        #region 公开接口（完全兼容原有调用，移除 _onChange 事件相关）
        /// <summary>
        /// 添加物品（不管哪个索引，直接添加,支持堆叠，自动分槽，优先复用空索引）
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isBatch">是否是批量添加（如果是批量添加，就不会触发更新事件）</param>
        /// <returns>是否添加成功</returns>
        /// 先填满已存在的索引，然后才会开辟新的索引
        public bool AddItem(Item item)
        {
            if (item == null || item.itemData == null || item.amount <= 0 || string.IsNullOrWhiteSpace(item.Guid))
            {
                Debug.LogWarning("添加失败：物品为空或数量无效");
                return false;
            }

            int remaining = item.amount;

            // 1. 尝试堆叠到已有同类型物品槽位（从 ItemHandle 获取同GUID索引）
            List<int> existingIndices = _itemHandle.GetIndexesByGuid(item.Guid);
            foreach (int targetIndex in existingIndices)
            {
                if (remaining <= 0) break;

                Item targetItem = _itemHandle[targetIndex];
                if (targetItem == null) continue;

                // 尝试堆叠，返回实际添加的数量
                int added = targetItem.TryAddItem(item.itemData, remaining);
                if (added > 0)
                {
                    remaining -= added;
                    _eventIndexList.AddLast(targetIndex); // 记录待更新索引
                }
            }

            // 2. 剩余物品创建新槽位（优先复用空索引，无则新增）
            while (remaining > 0)
            {
                int addCount = Mathf.Min(remaining, item.itemData.maxStack);
                Item newItem = new Item(item.itemData) { amount = addCount };

                // 调用 Handle 自动分配索引（复用空索引或新增）
                int newIndex = _itemHandle.AddItem(newItem);
                _eventIndexList.AddLast(newIndex); // 记录待更新索引
                remaining -= addCount;
            }
            
            UpdateEventInvoke(); // 触发更新事件
            //Debug.Log($"添加完成：{item.itemData.itemName}，剩余未添加：{remaining}");
            return true;
        }

        /// <summary>
        /// 在指定索引插入物品（支持拖动排序，自动填充中间空格）
        /// </summary>
        public bool InsertItemAt(Item item, int index)
        {
            if (item == null || item.itemData == null || index < 0)
            {
                Debug.LogWarning("插入失败：物品无效或索引越界");
                return false;
            }

            // 检查目标索引是否已被占用（有效物品）
            if (_itemHandle.IsIndexOccupied(index))
            {
                Debug.LogWarning($"插入失败：索引 {index} 已存在有效物品");
                return false;
            }

            // 调用 Handle 手动指定索引添加（自动填充中间空格）
            bool success = _itemHandle.AddItemWithIndex(index, item);
            if (success)
            {
                _eventIndexList.AddLast(index);
                UpdateEventInvoke();
            }
            return success;
        }
        
        /// <summary>
        /// 移除对应数量的物体,不管哪个索引
        /// </summary>
        /// <param name="item">物体本身，需要有对应的数量</param>
        /// <returns></returns>
        public bool RemoveItem(Item item)
        {
            if (item == null || item.itemData == null || item.amount <= 0 || string.IsNullOrWhiteSpace(item.Guid))
            {
                Debug.Log("移除失败：物品或者数字无效");
                return false;
            }
            int remaining = item.amount;
            //先进行可行性分析，检查是否能够完全删除该数量的物体
            int count = _itemHandle.GetItemCountByGuid(item.Guid);
            if (remaining > count)
            {
                Debug.Log($"数量不足以删除该物体{item.itemData.itemName},数量{item.amount},仓库中总共数量{count}");
                return false;
            }

            //找到该种物体的所有索引
            List<int> indexs = _itemHandle.GetIndexesByGuid(item.Guid);
            indexs.Sort();//便于从后往前减少，这个排序行为不会产生什么影响，因为其中的序列并没有用处

            for (int i = indexs.Count - 1; i >= 0; i--)
            {
                if (remaining <= 0) break;

                int targetIndex = indexs[i];
                Item targetItem = _itemHandle[targetIndex];
                int removed = targetItem.TryRemoveItem(item.itemData, remaining);
                //如果确实产生了移除行为
                if (removed > 0)
                {
                    remaining -= removed;
                    _eventIndexList.AddLast(targetIndex); // 记录待更新索引
                    //如果直接减少到0了，那就直接删除这个索引
                    if (targetItem.amount == 0) _itemHandle.DeleteItemByIndex(targetIndex);
                }
            }
            UpdateEventInvoke(); // 触发批量更新事件
            return true;
        }

        /// <summary>
        /// 抽取出指定索引的物品（保留索引，设为空格）
        /// </summary>
        public bool ExtractItemAt(Item item, int index)
        {
            if (item == null || item.itemData == null || index < 0)
            {
                Debug.LogWarning("移除失败：物品无效或索引越界");
                return false;
            }

            // 校验：索引对应的物品是否与目标一致
            Item targetItem = _itemHandle[index];
            if (targetItem == null || targetItem.itemData.GUID != item.itemData.GUID)
            {
                Debug.LogWarning("移除失败：索引对应的物品与目标物品不一致");
                return false;
            }

            // 调用 Handle 移除物品（保留索引，设为null）
            bool success = _itemHandle.DeleteItemByIndex(index);
            if (success)
            {
                _eventIndexList.AddLast(index);
                UpdateEventInvoke();
            }
            return success;
        }

        /// <summary>
        /// 更新指定索引的物品（覆盖原有物品）
        /// </summary>
        public bool UpdateItemAt(int index, Item newItem)
        {
            if (newItem == null || newItem.itemData == null || index < 0)
            {
                Debug.LogWarning("更新失败：物品无效或索引越界");
                return false;
            }

            // 检查索引是否存在（Handle 中索引连续，只要index <= MaxIndex就存在）
            if (index > _itemHandle.GetMaxIndex())
            {
                Debug.LogWarning($"更新失败：索引 {index} 不存在");
                return false;
            }

            // 调用 Handle 更新物品
            bool success = _itemHandle.UpdateItem(index, newItem);
            if (success)
            {
                _eventIndexList.AddLast(index);
                UpdateEventInvoke();
            }
            return success;
        }

        /// <summary>
        /// 获取仓库总槽位数量（包括空格）
        /// </summary>
        public int GetCount() => _itemHandle.GetMaxIndex() + 1; // 最大索引+1 = 总槽位

        /// <summary>
        /// 获取有效物品总数（过滤空格）
        /// </summary>
        public int GetValidItemCount() => _itemHandle.GetValidItemCount();

        /// <summary>
        /// 按索引获取物品（空格返回null）
        /// </summary>
        public Item GetItem(int index) => _itemHandle[index];

        /// <summary>
        /// 按GUID获取所有有效物品
        /// </summary>
        public List<Item> GetItemsByGuid(string guid) => _itemHandle.GetItemsByGuid(guid);

        /// <summary>
        /// 检查索引是否存在有效物品
        /// </summary>
        public bool IsIndexOccupied(int index) => _itemHandle.IsIndexOccupied(index);

        /// <summary>
        /// 清空仓库所有物品（保留槽位索引，设为空格）
        /// </summary>
        public void Clear()
        {
            Debug.LogWarning("外界调用了仓库的清空方法");
            if (inventoryPlace == E_InventoryPlace.Inventory)
            {
                Debug.LogError("！！！外界调用了主仓库的清空方法，这将会删除所有的仓库对象（已拦截，并未清空，请检查代码）！！！");
            }
            int maxIndex = _itemHandle.GetMaxIndex();
            if (maxIndex < 0) return;

            // 记录所有索引，用于触发批量更新
            for (int i = 0; i <= maxIndex; i++)
            {
                _eventIndexList.AddLast(i);
                _itemHandle.DeleteItemByIndex(i); // 调用 Handle 移除（设为null）
            }
            // 触发批量更新事件
            UpdateEventInvoke();
        }
        #endregion

        #region 更新事件订阅/取消（仅保留 _onUpdate 相关）
        public void SubUpdateEvent(Action<IEnumerable<int>> action) => _onUpdate += action;
        public void UnSubUpdateEvent(Action<IEnumerable<int>> action) => _onUpdate -= action;
        #endregion

        #region 内部辅助方法（事件触发，无残留）
        /// <summary>
        /// 触发更新事件，之后清空待更新索引列表
        /// </summary>
        private void UpdateEventInvoke()
        {
            if (_eventIndexList.Count == 0) return;

            _onUpdate?.Invoke(_eventIndexList);
            _eventIndexList.Clear(); // 清空，避免下次残留
        }
        #endregion
    }
}
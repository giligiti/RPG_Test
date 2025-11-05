using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [RequireComponent(typeof(InventoryBoxView))]
    [RequireComponent(typeof(InventoryBoxMode))]
    public class InventoryContiner : MonoBehaviour, IInventoryContiner
    {
        public E_InventoryPlace place;          //具体的仓库
        private IInventory inventory;           //存储箱对应的存储接口

        //物品字典，Key为物品GUID，Value为物品格子索引列表
        public Dictionary<string, List<int>> gridDic;   
        public List<Item> itemList;                      //物品列表
        private int Index => itemList.Count - 1;         //当前物品格子索引
        Action<E_InventoryUpdateMode, List<(Item, int)>> onContainerChanged;

        void Awake()
        {
            gridDic = new();
            //向管理器申请对应的仓库
            inventory = InventoryMgr.Instance.GetInventory(place);
            ApplyItemDatas(inventory);
        }

        private void ApplyItemDatas(IInventory inventory)
        {
            //得到该仓库的所有物品
            foreach (var item in inventory.GetItems())
            {
                //如果已有该物品的格子，尝试添加到已有格子
                if (gridDic.TryGetValue(item.itemData.GUID, out var gridList))
                {
                    if (gridList is null)
                    {
                        Debug.LogError("获取物品格子失败");
                        return;
                    }

                    bool isFinished = false;
                    //遍历对应物品索引列表，满足条件则添加
                    foreach (var index in gridList)
                    {
                        if (itemList[index].isMaxStack) continue;
                        
                        Item griditem = itemList[index];
                        int addAmount = griditem.TryAddItem(item.itemData, item.amount);
                        if (addAmount == item.amount)
                        {
                            item.TryRemoveItem(item.amount);
                            isFinished = true;
                            Debug.Log($"添加物品格子成功: {item.itemData.itemName} x{item.amount}");
                            break;
                        }
                        else
                        {
                            item.TryRemoveItem(addAmount);
                        }
                    }
                    if (isFinished) continue;
                    //如果所有已有格子均已满，创建新格子
                    AddToList(item, true);
                }
                else AddToList(item, true);
            }
        }
        
        #region IInventoryContiner接口实现
        public Item GetItem(int index)
        {
            if (index < 0 || index >= itemList.Count) return null;
            return itemList[index];
        }
        public int GetCount() => itemList.Count;

        public void SubscribeOnChanged(Action<E_InventoryUpdateMode, List<(Item, int)>> onContainerChanged)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeOnChanged(Action<E_InventoryUpdateMode, List<(Item, int)>> onContainerChanged)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 辅助函数

        /// <summary>
        /// 添加物品到列表
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isNew"></param>
        private void AddToList(Item item, bool isNew)
        {
            ItemData data = item.itemData;
            itemList.Add(item);
            if (isNew)
            {
                gridDic.Add(data.GUID, new List<int>() { Index });
            }
            else
            {
                gridDic[data.GUID].Add(Index);
            }
        }
        private void RemoveFromList(Item item)
        {
            ItemData data = item.itemData;
            itemList.Remove(item);
            if (itemList.FindAll(i => i.itemData.GUID == data.GUID).Count == 0)
            {
                gridDic.Remove(data.GUID);
            }
            else
            {
                gridDic[data.GUID].Remove(Index);
            }
        }
        #endregion
    }
    public class InventoryBoxMode : MonoBehaviour
    {

    }
    public interface IInventoryContiner
    {
        int GetCount();
        Item GetItem(int index);
        void SubscribeOnChanged(Action<E_InventoryUpdateMode, List<(Item, int)>> onContainerChanged);
        void UnsubscribeOnChanged(Action<E_InventoryUpdateMode, List<(Item, int)>> onContainerChanged);
    }
}
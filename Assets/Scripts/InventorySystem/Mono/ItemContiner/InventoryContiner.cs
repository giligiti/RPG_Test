using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [RequireComponent(typeof(InventoryBoxView))]
    public class InventoryContiner : MonoBehaviour, IInventoryContiner
    {
        public E_InventoryPlace place;                   //具体的仓库
        public E_InventoryPlace Place { get => place; }
        private Inventory inventory;                    //存储箱对应的存储接口


        void Awake()
        {
            //向管理器申请对应的仓库
            inventory = InventoryMgr.Instance.GetInventory(Place);
        }
        #region IInventoryContiner接口实现
        public Item GetItem(int index) => inventory.GetItem(index);
        public int GetCount() => inventory.GetCount();

        public void SubscribeOnChanged(Action<IEnumerable<int>> onContainerChanged)
        {
            inventory.SubUpdateEvent(onContainerChanged);
        }

        public void UnsubscribeOnChanged(Action<IEnumerable<int>> onContainerChanged)
        {
            inventory.UnSubUpdateEvent(onContainerChanged);
        }
        #endregion

    }
    public interface IInventoryContiner
    {
        public E_InventoryPlace Place { get; }
        int GetCount();
        Item GetItem(int index);
        void SubscribeOnChanged(Action<IEnumerable<int>> onContainerChanged);
        void UnsubscribeOnChanged(Action<IEnumerable<int>> onContainerChanged);
    }
}
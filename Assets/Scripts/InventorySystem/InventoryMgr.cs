using System.Collections.Generic;

namespace InventorySystem
{
    public class InventoryMgr : BaseManager<InventoryMgr>
    {
        private SelfInventory self;
        private MainInventory main;
        public Dictionary<string, ItemData> itemPairs;

        private InventoryMgr()
        {
            //初始化各个存储箱数据
            this.self = new SelfInventory();
            this.main = new MainInventory();
            itemPairs = new();
        }
        public IInventory GetInventory(E_InventoryPlace place)
        {
            switch (place)
            {
                case E_InventoryPlace.Inventory:
                    return main;
                case E_InventoryPlace.PlayerBag:
                    return self;
                default:
                    UnityEngine.Debug.LogWarning("出现未配置的仓库");
                    return null;
            }
        }
        /// <summary>
        /// 存储物体
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="amount">物品数量</param>
        /// <param name="place">存储的具体仓库仓库</param>
        /// <returns></returns>
        public bool TryAddItem(ItemData data, int amount, E_InventoryPlace place)
        {
            //可以优化（使用共同基础的统一接口），暂时如此，
            switch (place)
            {
                case E_InventoryPlace.Inventory:
                    return main.AddItem(data, amount);
                case E_InventoryPlace.PlayerBag:
                    return self.AddItem(data, amount);
                default:
                    UnityEngine.Debug.LogWarning("出现未配置的仓库");
                    return false;
            }
        }
        public bool TryRemoveItem(ItemData data, int amount, E_InventoryPlace place)
        {
            switch (place)
            {
                case E_InventoryPlace.Inventory:
                    return main.RemoveItem(data, amount);
                case E_InventoryPlace.PlayerBag:
                    return self.RemoveItem(data, amount);
                default:
                    UnityEngine.Debug.LogWarning("出现未配置的仓库");
                    return false;
            }
        }

    }
}
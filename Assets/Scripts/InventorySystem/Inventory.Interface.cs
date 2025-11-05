namespace InventorySystem
{
    /// <summary>
    /// 可存储物品的仓库接口
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// 往仓库中添加物品
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <returns>是否存储成功</returns>
        bool AddItem(ItemData data, int amount);
        bool RemoveItem(ItemData data, int amount);
        System.Collections.Generic.IEnumerable<Item> GetItems();
    }
    /// <summary>
    /// 存储物体的接口（把物体放到背包）
    /// </summary>
    public interface Inventorable
    {
        /// <summary>
        /// 存储物品
        /// </summary>
        /// <param name="data">物品的数据</param>
        /// <param name="place">物品要存储的地方（玩家背包还是仓库）</param>
        /// <returns>是否存储成功（背包容量满就会失败）</returns>
        bool StorageItem(ItemData data, E_InventoryPlace place);
    }
    public interface IItemGrid
    {
        bool isMaxStack { get; }
        int Index { get; set; }
        UnityEngine.GameObject GridSelfObject { get; }

        void UpdateItemData(ItemData data, int amount);
        void ResetGrid();
    }
}
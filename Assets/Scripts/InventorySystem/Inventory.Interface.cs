namespace InventorySystem
{
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
        int Index { get; set; }
        UnityEngine.GameObject GridSelfObject { get; }

        void UpdateItemData(ItemData data, int amount);
        void ResetGrid();
        void InjurtInventoryPlace(E_InventoryPlace place);
    }
}
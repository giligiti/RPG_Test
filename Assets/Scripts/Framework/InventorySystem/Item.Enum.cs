namespace InventorySystem
{
    /// <summary>
    /// 物体大类
    /// </summary>
    public enum E_ItemType
    {
        Consumable,     //消耗品
        Material,       //素材（怪物素材，合成素材）
        Equipment,      //装备
        Armor,          //弹药或瓶
    }
    /// <summary>
    /// 可装备物体中的装备的类别
    /// </summary>
    public enum E_EquipmentSlot
    {
        None,
        Head,    // 头部（头盔、帽子）
        Chest,   // 胸部（上衣、铠甲）
        Pants,   // 腿部（裤子、护腿）
        Boots,   // 脚部（靴子、鞋子）
        Weapon,  // 武器（可扩展）
        Gloves   // 手套（可扩展）
    }
    /// <summary>
    /// 可使用物品中的“使用”类别
    /// </summary>
    public enum E_UsableType
    {
        PlayerItem,
        HealthItem,
        CustomEvent,
    }
    public enum E_InventoryPlace
    {
        PlayerBag,      //玩家背包
        Inventory,      //仓库
    }
    public enum E_InventoryUpdateMode
    {
        Single,         //更新单个格子
        Everything,     //更新所有格子
    }

}
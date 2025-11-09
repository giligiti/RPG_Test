using System.Collections.Generic;

namespace InventorySystem
{
    public class InventoryMgr : BaseManager<InventoryMgr>
    {
        public Dictionary<E_InventoryPlace, Inventory> InventoryDic;
        public Dictionary<string, ItemData> itemPairs;
        private Inventory _mainInventory;
        public Inventory MainInventory 
        { 
            get => _mainInventory; 
            set 
            { 
                if (value == null) throw new System.ArgumentNullException("主仓库不能为null");
                _mainInventory = value;
                // 后续可添加：主仓库切换事件、日志等
            } 
        }

        private InventoryMgr()
        {
            //初始化各个存储箱数据
            itemPairs = new();
            MainInventory = new Inventory(E_InventoryPlace.Inventory);
            InventoryDic = new()
            {
                { E_InventoryPlace.Inventory, MainInventory }
            };
        }

        /// <summary>
        /// 获取对应的仓库对象方法
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public Inventory GetInventory(E_InventoryPlace place)
        {
            return InventoryDic.ContainsKey(place) ? InventoryDic[place] : null;
        }

        #region 主仓库便捷交互方法
        //这里的添加和删除方法都是操作主仓库的，由于提供了公开的主仓库对象属性，所以以下包装的方法并无必要
        //仅仅是为了方便外界直接调用而已，只封装了主要常用的添加和删除方法，如果需要调用其他方法，请获取主仓库对象再调用对应方法

        /// <summary>
        /// 添加方法
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AddItemToMainInventory(Item item) => MainInventory.AddItem(item);
        /// <summary>
        /// 移除物体
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemoveItemFormMainInventory(Item item) => MainInventory.RemoveItem(item);
        #endregion
    
    }
}
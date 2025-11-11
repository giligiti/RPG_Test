using UnityEngine;

namespace InventorySystem
{
    public class Item 
    {
        public ItemData itemData;
        public int amount = 1;              //数量
        public bool isMaxStack => amount >= itemData.maxStack;
        public string Guid => itemData.GUID;

        public Item(ItemData itemData)
        {
            this.itemData = itemData;
        }
        
        /// <summary>
        /// 尝试添加物品
        /// </summary>
        /// <param name="data"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public int TryAddItem(ItemData data, int amount)
        {
            //如果不是同种物品或者已满，返回0
            if (this.itemData.GUID != data.GUID || this.amount >= this.itemData.maxStack) return 0;
            int count = this.amount + amount;
            if (count > this.itemData.maxStack)
            {
                int addNum = this.itemData.maxStack - this.amount;
                this.amount = this.itemData.maxStack;
                return addNum;
            }
            this.amount += amount;
            return amount;
        }

        /// <summary>
        /// 尝试移除物品
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>实际移除的数量</returns>
        public int TryRemoveItem(ItemData data, int amount)
        {
            if (this.itemData.GUID != data.GUID || amount < 0) return 0;
            if (amount >= this.amount)
            {
                int removeNum = this.amount;
                this.amount = 0;
                return removeNum;
            }
            this.amount -= amount;
            return amount;
        }
        
    }

}

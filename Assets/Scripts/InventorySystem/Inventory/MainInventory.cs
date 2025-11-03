using System.Collections.Generic;

namespace InventorySystem
{
    public class MainInventory : IInventory
    {
        public bool AddItem(ItemData data, int amount)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ItemData> GetItems()
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveItem(ItemData data, int amount)
        {
            throw new System.NotImplementedException();
        }

        IEnumerable<Item> IInventory.GetItems()
        {
            throw new System.NotImplementedException();
        }
    }
}
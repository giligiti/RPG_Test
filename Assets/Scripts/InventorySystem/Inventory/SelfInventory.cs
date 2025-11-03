using System.Collections.Generic;

namespace InventorySystem
{
    public class SelfInventory : IInventory
    {
        public bool AddItem(ItemData data, int amount)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Item> GetItems()
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveItem(ItemData data, int amount)
        {
            throw new System.NotImplementedException();
        }
    }
}
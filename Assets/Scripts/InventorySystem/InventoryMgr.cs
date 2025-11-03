namespace InventorySystem
{
    public class InventoryMgr : BaseManager<InventoryMgr>
    {
        private SelfInventory self;
        private MainInventory main;

        private InventoryMgr()
        {
            this.self = new SelfInventory();
            this.main = new MainInventory();
        }

    }
}
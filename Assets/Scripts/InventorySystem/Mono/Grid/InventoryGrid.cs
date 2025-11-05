using UnityEngine;

namespace InventorySystem
{
    [RequireComponent(typeof(InventoryGridView))]
    public class InventoryGrid : MonoBehaviour//, IItemGrid
    {
        private ItemData itemData;
        private int amount;
        private InventoryGridView gridView;

        public bool isMaxStack => throw new System.NotImplementedException();
        #region 初始化
        void Awake()
        {
            gridView = GetComponent<InventoryGridView>();
        }
        void OnEnable()
        {
            gridView.ShowUI();
        }
        public void DisableItem()
        {
            gridView.HideUI(() => this.gameObject.SetActive(false));
        }
        #endregion
        public void Init(ItemData data, int amount)
        {
            this.itemData = data;
            this.amount = amount;
        }
        /// <summary>
        /// 尝试添加物品到格子
        /// </summary>
        /// <param name="data">物品数据</param>
        /// <param name="amount">要添加的物品数量</param>
        /// <returns>成功添加到这个格子的物品数量</returns>
        public int TryAddItem(ItemData data, int amount)
        {
            //如果不是同种物品或者已满，返回0
            if (this.itemData.GUID != data.GUID || isMaxStack) return 0;
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
    }
}
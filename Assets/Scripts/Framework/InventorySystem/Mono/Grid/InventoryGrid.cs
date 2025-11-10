using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem
{
    public class InventoryGrid : MonoBehaviour, IItemGrid
    {
        public Image image;
        public TextMeshProUGUI textAmount;
        private ItemData itemData;
        private int amount;
        private int index;
        private E_InventoryPlace inventoryPlace;
        public int Index { get => index; set => index = value; }

        public GameObject GridSelfObject => this.gameObject;
        private void Awake()
        {
            image.enabled = false;
            textAmount.enabled = false;
        }
        
        #region ItemGrid接口内容
        public void UpdateItemData(ItemData data, int amount)
        {
            if (data == null)
            {
                amount = 0;
                itemData = null;
                SetImageEnable(false);
                return;
            }
            else SetImageEnable(true);
            this.itemData = data;
            this.amount = amount;
            image.sprite = data.itemIcon;
            textAmount.text = amount.ToString();
        }
        private void SetImageEnable(bool isEnable)
        {
            image.enabled = isEnable;
            textAmount.enabled = isEnable;
        }
        public void ResetGrid()
        {
            itemData = null;
            image.sprite = null;
            image.enabled = false;
            textAmount.enabled = false;
            amount = 0;
        }
        /// <summary>
        /// 注入格子所代表的仓库
        /// </summary>
        /// <param name="place"></param>
        public void InjurtInventoryPlace(E_InventoryPlace place) => inventoryPlace = place;
        #endregion
    
    
    
    
    }
}
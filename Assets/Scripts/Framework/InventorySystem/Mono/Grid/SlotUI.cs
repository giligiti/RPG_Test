using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem
{
    public class SlotUI : MonoBehaviour
    {
        public Image itemImage;
        public TMPro.TMP_Text amountText;

        public void UpdateSlot(Item item)
        {
            if (item == null || item.itemData == null)
            {
                itemImage.enabled = false;
                amountText.text = "";
                return;
            }
            itemImage.enabled = true;
            itemImage.sprite = item.itemData.itemIcon;
            amountText.text = item.amount > 1 ? item.amount.ToString() : "";
        }
    }
}
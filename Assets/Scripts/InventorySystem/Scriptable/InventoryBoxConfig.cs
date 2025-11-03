using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "NewInventoryBoxConfig", menuName = "InventorySystem/InventoryBoxConfig")]
    public class InventoryBoxConfig : ScriptableObject
    {
        public E_InventoryPlace place = E_InventoryPlace.Inventory;
        public int columns;            //列数
        public int rows;               //行数
        public bool isSellable;       //是否可出售物品
    }
}
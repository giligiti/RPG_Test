using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "NewInventoryBoxConfig", menuName = "InventorySystem/InventoryBoxConfig")]
    public class InventoryBoxConfig : ScriptableObject
    {
        public int columns;                 //列数
        public int rows;                    //行数
        public bool isSellable;             //是否可出售物品
        public float cellUpDownSpacing;     //每个格子上下间距
        public int cellWidth;               //每个物品的宽度
        public int cellHeight;              //每个物品的高度
    }
}
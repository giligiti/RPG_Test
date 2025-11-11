using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "NewInventoryBoxConfig", menuName = "我的框架/库存系统/存储箱UI布局配置")]
    public class InventoryBoxConfig : ScriptableObject
    {
        public int columns;                 //列数
        public int rows;                    //行数
        public bool isSellable;             //是否可出售物品
        public float paddingTop;            //格子相对于上边框的距离
        public float paddingLeft;           //格子对于左边边框的距离
        public float cellUpDownSpacing;     //每个格子上下间距
        public float cellRightLeftSpacing;  //格子左右间隔
        public int cellWidth;               //每个物品的宽度
        public int cellHeight;              //每个物品的高度
    }
}
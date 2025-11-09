using UnityEngine;

namespace InventorySystem
{
    
    [CreateAssetMenu(fileName = "NewItemData", menuName = "库存系统/物体基础数据")]
    public class ItemData : ScriptableObject
    {
        public string GUID;         //分配的id
        public string itemName;     //物体名字
        public Sprite itemIcon;     //物体图标
        public E_ItemType type;     //物体类别
        [TextArea] 
        public string descript;//物体描述
        public GameObject prefab;   //物体对应的预制体
        public int maxStack = 1;    //最大堆叠数量，为1说明不可堆叠
        public int price;           //价格
        [SerializeField]
        public ItemSetting setting; //物品基本属性设置（是否可以丢弃）

    }

}
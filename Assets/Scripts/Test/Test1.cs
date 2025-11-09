using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestSystem
{
    public class Test1 : MonoBehaviour
    {
        public Button increase;
        public Button Decress;
        public Button DecressOnIndex;
        public GameObject panel;
        public Toggle openBagPanel;
        public TMP_InputField inputIdex;
        public Canvas canvas;

        public ItemData itemData;
        public ItemData otherData;
        private GameObject Obj;

        void Start()
        {
            increase.onClick.AddListener(() =>
            {
                Item item = null;
                
                //增加10个物体
                for (int i = 0; i < 10; i++)
                {
                    item = new Item(itemData);
                    item.amount = 2;
                    InventoryMgr.Instance.AddItemToMainInventory(item);
                }
                item = new Item(otherData);
                item.amount = 2;
                InventoryMgr.Instance.AddItemToMainInventory(item);
            });
            Decress.onClick.AddListener(() =>
            {
                Item item = null;
                //减少10个物体
                for (int i = 0; i < 10; i++)
                {
                    item = new Item(itemData);
                    item.amount = 2;
                    InventoryMgr.Instance.RemoveItemFormMainInventory(item);
                }
                item = new Item(otherData);
                item.amount = 2;
                InventoryMgr.Instance.RemoveItemFormMainInventory(item);
            });
            DecressOnIndex.onClick.AddListener(() =>
            {
                if (string.IsNullOrWhiteSpace(inputIdex.text))
                {
                    UnityEngine.Debug.Log("没有输入，无法删除");
                    return;
                }
                var item = new Item(itemData);
                item.amount = 2;
                bool isSuc = InventoryMgr.Instance.MainInventory.ExtractItemAt(item,int.Parse(inputIdex.text));
                if (isSuc) Debug.Log($"移除索引{int.Parse(inputIdex.text)}的物体成功");
            });
            openBagPanel.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    Obj = Instantiate(panel, canvas.transform);
                    Debug.Log("实例化背包面板");
                }
                else
                {
                    if (Obj != null)
                        Destroy(Obj);
                    Obj = null;
                }
            });
        }
        
        
    }

}
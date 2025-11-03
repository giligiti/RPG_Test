using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InventorySystem
{
    [RequireComponent(typeof(InventoryBoxUI))]
    [RequireComponent(typeof(InventoryBoxMode))]
    public class InventoryContiner : MonoBehaviour
    {
        public InventoryBoxConfig config;       //存储箱配置文件
        private IInventory inventory;           //存储箱对应的存储接口
        public GameObject gridPrefab;

        //物品格子字典，Key为物品GUID，Value为物品格子索引列表
        public Dictionary<string, List<int>> gridDic;   
        public List<IItemGrid> grids;                   //物品格子列表
        private int Index => grids.Count - 1;

        void Awake()
        {
            gridDic = new();
            inventory = InventoryMgr.Instance.GetInventory(config.place);
            _ = ApplyItemDatas(inventory);
        }

        public async UniTask ApplyItemDatas(IInventory inventory)
        {
            //得到该仓库的所有物品
            foreach (var item in inventory.GetItems())
            {
                //如果已有该物品的格子，尝试添加到已有格子
                if (gridDic.TryGetValue(item.itemData.GUID, out var gridList))
                {
                    if (gridList is null)
                    {
                        Debug.LogError("获取物品格子失败");
                        return;
                    }
                    //遍历对应物品的格子索引列表，满足条件则添加
                    foreach (var index in gridList)
                    {
                        var grid = grids[index];
                        if (grid.isMaxStack) continue;
                        if (grid.TryAddItem(item.itemData, item.amount))
                        {
                            Debug.Log($"添加物品格子成功: {item.itemData.itemName} x{item.amount}");
                            break;
                        }
                    }
                }
                else CreateItemGrid(item.itemData, item.amount, true);
                //等待下一帧，减少初始化压力
                await UniTask.NextFrame();
            }
        }
        /// <summary>
        /// 创建物品格子
        /// </summary>
        /// <param name="data"></param>
        /// <param name="amount"></param>
        private void CreateItemGrid(ItemData data, int amount,bool isNew=false)
        {
            //实例化物品格子
            var grid = Instantiate(gridPrefab, this.transform).GetComponent<IItemGrid>();
            if (grid.TryAddItem(data, amount))
            {
                Debug.Log($"添加物品格子成功: {data.itemName} x{amount}");
                if (isNew) gridDic[data.GUID].Add(Index);
                else gridDic.Add(data.GUID, new List<int>() { Index });
            }
            else
            {
                Debug.LogError($"添加物品格子失败: {data.itemName} x{amount}");
            }
            
        }

    }
    public class InventoryBoxUI : MonoBehaviour, UIView
    {
        public void HideUI(Action action = null)
        {
            throw new NotImplementedException();
        }

        public void ShowUI(Action action = null)
        {
            throw new NotImplementedException();
        }
    }
    public class InventoryBoxMode : MonoBehaviour
    {

    }
}
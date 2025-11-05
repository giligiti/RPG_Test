using System;
using System.Collections.Generic;
// 反射已移除，改为直接使用 IInventoryContiner 提供的方法（由容器实现）
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem
{
    [RequireComponent(typeof(InventoryContiner))]
    public class InfiniteScrollUI : MonoBehaviour
    {
        public InventoryBoxConfig config;                       //存储箱配置文件
        [Header("UI 绑定")]
        [SerializeField] private RectTransform content;         // ScrollRect.Content
        [SerializeField] private GameObject slotPrefab;         // 格子预制体
        [SerializeField] private ScrollRect parentScroll;       // ScrollRect 组件

        private int totalItemCount;            // 总物品数量（模拟无限滚动）
        private int visibleColumns;            // 背包显示的物品列数
        private int visibleRows;               // 背包显示的行数
        [SerializeField] private int extraRows = 2;               // 额外缓冲行数
        private float cellHeight = 100f;
        private int totalRows;
        private float lastScrollPosition = 0;

        private IInventoryContiner continer;                    //物品容器接口
        private GridManager gridManager;                        //格子管理器

        private ItemGridPool pooledSlots = new ItemGridPool(); //格子接口对象池
        //对应索引的格子，仅存储当前可见的格子，其他的是null 
        private Dictionary<int, IItemGrid> slotDict = new Dictionary<int, IItemGrid>();       
        private LinkedList<IItemGrid> activeSlots = new LinkedList<IItemGrid>();


        #region 初始化
        void Awake()
        {
            // 直接按接口获取容器（不用反射）。
            continer = GetComponent<IInventoryContiner>();
            if (continer == null)
            {
                Debug.LogError("InfiniteScrollUI: 未找到 IInventoryContiner 组件，请确保该组件已挂在同一 GameObject 上");
            }
            gridManager = new GridManager(pooledSlots, slotPrefab, content);
            GridInit();

            parentScroll.onValueChanged.AddListener(_ => UpdateVisibleSlots());

            // 订阅容器变化(单个格子发生变化的时候更新对应格子)
            continer.SubscribeOnChanged(OnItemChanged);

            UpdatePageGrid();
        }
        private void GridInit()
        {
            // 初始化 totalItemCount
            continer = GetComponent<IInventoryContiner>();
            totalItemCount = continer.GetCount();
            // 初始化格子尺寸和间距
            cellHeight = config.cellHeight + config.cellUpDownSpacing;
            visibleColumns = config.columns;
            visibleRows = config.rows;
            int poolCount = (visibleRows + 2 *extraRows + 1) * visibleColumns;
            //实例化格子
            for (int i = 0; i < poolCount; i++)
            {
                IItemGrid itemGrid = gridManager.ActivateGrid(i);
                var slot = itemGrid.GridSelfObject;
                slot.name = $"Slot_{i}";
            }
        }

        private void OnDestroy()
        {
            // 退订容器变化
            continer.UnsubscribeOnChanged(OnItemChanged);
        }
        
        #endregion

        /// <summary>
        /// 更新当前页面的所有格子
        /// </summary>
        private void UpdatePageGrid()
        {
            totalItemCount = continer.GetCount();
            int rows = Mathf.CeilToInt((float)totalItemCount / visibleColumns);
            parentScroll.content.sizeDelta = new Vector2(parentScroll.content.sizeDelta.x, rows * cellHeight);
            //如果增加了行数且当前最底部行已经显示，则添加新行
            int targetRow = CalculateBottomRow();
            if (rows > totalRows && CalculateBottomRow() == totalRows)
            {
                for (int i = 0; i < extraRows; i++)
                {
                    for (int col = 0; col < visibleColumns; col++)
                    {
                        //从对象池中取出格子并计算将要分配的索引，然后重新绑定索引和格子接口对象的绑定
                        int toIndex = CalculateIndex(targetRow, col);
                        IItemGrid itemGrid = gridManager.ActivateGrid(toIndex);
                        // 更新格子数据
                        var item = continer.GetItem(toIndex);
                        itemGrid.UpdateItemData(item?.itemData, item?.amount ?? 0);
                    }
                }
                SyncSlotSiblingIndex();
                totalRows = rows;
            }

            foreach (var itemGrid in gridManager.GetAllActiveGrids())
            {
                Item slot = continer.GetItem(itemGrid.Index);
                if (slot == null || slot.itemData == null)
                {
                    itemGrid.UpdateItemData(null, 0);
                    continue;
                }
                itemGrid.UpdateItemData(slot.itemData, slot.amount);
            }
        }
        /// <summary>
        /// 滑动更新可见格子
        /// </summary>
        private void UpdateVisibleSlots()
        {
            float scrollDelta = parentScroll.content.anchoredPosition.y - lastScrollPosition;
            while (Math.Abs(scrollDelta) >= cellHeight)
            {
                lastScrollPosition = parentScroll.content.anchoredPosition.y;
                // 计算是向上滑动还是向下滑动
                if (scrollDelta > 0)
                {
                    // 向上滑动
                    ScrollUp();
                }
                else
                {
                    // 向下滑动
                    ScrollDown();
                }

            }
            void ScrollUp()
            {
                int topRow = CalculateTopRow();
                if (topRow == 0) return;
                // 回收最底部的一行格子
                OnRemoveRows(false);
                // 分配新的最顶部一行格子
                OnAddRows(true);
                //更新层级面板上的顺序
                SyncSlotSiblingIndex();
            }

            void ScrollDown()
            {
                // 向下滑动时，检查最底部行是否已经完全滚出视图
                int bottomRow = CalculateBottomRow();
                if (bottomRow == totalRows) return;
                // 将最顶部的一行格子移动到底部
                OnRemoveRows(true);
                // 分配新的最底部一行格子
                OnAddRows(false);
                //更新层级面板上的顺序
                SyncSlotSiblingIndex();
            }
        }

        /// <summary>
        /// 更新单个格子
        /// </summary>
        /// <param name="index"></param>
        /// <param name="slot"></param>
        private void UpdateSingleGrid(int index, Item slot)
        {
            if (index < 0 || index >= slotDict.Count) return;
            IItemGrid itemGrid = gridManager.GetGridByIndex(index);
            if (itemGrid == null)
            {
                Debug.LogError("InfiniteScrollUI: 未找到对应的 IItemGrid 组件");
                return;
            }
            if (slot == null || slot.itemData == null)
            {
                itemGrid.UpdateItemData(null, 0);
                return;
            }
            itemGrid.UpdateItemData(slot.itemData, slot.amount);
        }

        private void OnItemChanged(E_InventoryUpdateMode mode, List<(Item, int)> items)
        {
            switch (mode)
            {
                case E_InventoryUpdateMode.Single:
                    foreach (var (item, index) in items)
                    {
                        //更新单个格子
                        UpdateSingleGrid(index, item);
                    }
                    break;
                case E_InventoryUpdateMode.Everything:
                    UpdatePageGrid();
                    break;
            }
        }

        #region 辅助函数
        /// <summary>
        /// 通过行列计算索引
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private int CalculateIndex(int row, int col)
        {
            return row * visibleColumns + col;
        }
        /// <summary>
        /// 计算当前显示的最顶部行数
        /// </summary>
        /// <returns></returns>
        private int CalculateTopRow()
        {
            float contentY = parentScroll.content.anchoredPosition.y;
            int topRow = Mathf.FloorToInt(contentY / cellHeight);
            return topRow;
        }
        /// <summary>
        /// 计算当前显示的最底部行数
        /// </summary>
        /// <returns></returns> 
        private int CalculateBottomRow()
        {
            float contentY = parentScroll.content.anchoredPosition.y;
            int bottomRow = Mathf.CeilToInt((contentY + parentScroll.viewport.rect.height) / cellHeight);
            return bottomRow;
        }
        /// <summary>
        /// 同步格子在层级中的顺序
        /// </summary>
        private void SyncSlotSiblingIndex()
        {
            var node = activeSlots.First;
            for (int i = 0; i < activeSlots.Count; i++)
            {
                var obj = node.Value.GridSelfObject;
                obj.transform.SetSiblingIndex(node.Value.Index);
                obj.name = $"Slot_{node.Value.Index}";
                node = node.Next;
            }
        }
        /// <summary>
        /// 移除整行边缘格子
        /// </summary>
        /// <param name="isabove">是否从上方移除</param>
        private void OnRemoveRows(bool isabove)
        {
            int targetRow;
            if (isabove)
            {
                // 从上方移除行
                targetRow = CalculateTopRow() - extraRows;
                if (targetRow <= 0) return;
            }
            else
            {
                // 从下方移除行
                // 回收最底部的一行格子
                targetRow = CalculateBottomRow() + extraRows;
                if (targetRow >= totalRows) return;
            }
            for (int col = 0; col < visibleColumns; col++)
            {
                // 计算从哪个索引回收，放回对象池并置空字典引用
                int fromIndex = CalculateIndex(targetRow, col);
                var grid = gridManager.GetGridByIndex(fromIndex);
                gridManager.RecycleGrid(grid);
            }
        }
        /// <summary>
        /// 添加整行不可见格子
        /// </summary>
        /// <param name="isabove">是否从上方添加</param>
        private void OnAddRows(bool isabove)
        {
            int targetRow;
            if (isabove)
            {
                // 分配新的最顶部一行格子
                targetRow = CalculateTopRow() - extraRows;
                if (targetRow <= 0) return;
            }
            else
            {
                // 分配新的最底部一行格子
                targetRow = CalculateBottomRow() + extraRows;
                if (targetRow >= totalRows) return;
            }
            for (int col = 0; col < visibleColumns; col++)
            {
                //从对象池中取出格子并计算将要分配的索引，然后重新绑定索引和格子接口对象的绑定
                int toIndex = CalculateIndex(targetRow, col);
                IItemGrid itemGrid = gridManager.ActivateGrid(toIndex);
                // 更新格子数据
                var item = continer.GetItem(toIndex);
                itemGrid.UpdateItemData(item?.itemData, item?.amount ?? 0);
            }
            SyncSlotSiblingIndex();
        }
        
        #endregion
    
    }
}
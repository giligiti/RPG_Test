using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem
{
    [RequireComponent(typeof(InventoryContiner))]
    public class InfiniteScrollUI : MonoBehaviour
    {
        public InventoryBoxConfig config;                       //存储箱配置文件
        [Header("UI 绑定")]
        [SerializeField] private Transform content;             // ScrollRect.Content
        [SerializeField] private GameObject slotPrefab;         // 格子预制体
        [SerializeField] private ScrollRect parentScroll;       // ScrollRect 组件

        private int visibleColumns;            // 背包显示的物品列数
        private int visibleRows;               // 背包显示的行数
        private int LeastCount => visibleColumns * visibleRows;//最低格子数量
        private int extraRows = 2;             // 额外缓冲行数
        private float paddingTop;       
        private float paddingLeft;
        private float cellwidth = 100f;
        private float cellHeight = 100f;
        private int TotalRows => Mathf.CeilToInt(continer.GetCount() / (float)visibleColumns);
        private int lastTopRow = 0; // 上一次的顶部行号

        private bool _isScrolling;  //表示是否正在滑动
        private CancellationTokenSource _cts = new();

        private IInventoryContiner continer;                    //物品容器接口
        private GridManager gridManager;                        //格子管理器

        private ItemGridPool pooledSlots = new ItemGridPool(); //格子接口对象池

        #region 初始化
        void Awake()
        {
            // 直接按接口获取容器（不用反射）。
            continer = GetComponent<IInventoryContiner>();
            if (continer == null)
            {
                Debug.LogError("InfiniteScrollUI: 未找到 IInventoryContiner 组件，请确保该组件已挂在同一 GameObject 上");
            }
            gridManager = new GridManager(pooledSlots, slotPrefab, content, continer.Place);

            //格子有关的初始化
            GridInit();

            //调整ScrollRectContent的大小
            AdjustScrollRectCont();
        }
        void OnEnable()
        {
            // 订阅容器变化(单个格子发生变化的时候更新对应格子)
            continer.SubscribeOnChanged(OnItemChanged);
            parentScroll.onValueChanged.AddListener(OnScrollBegin);
        }
        private void OnDisable()
        {
            // 退订容器变化
            continer.UnsubscribeOnChanged(OnItemChanged);
            parentScroll.onValueChanged.RemoveListener(OnScrollBegin);

        }

        private void GridInit()
        {
            // 初始化 
            continer = GetComponent<IInventoryContiner>();
            // 初始化格子尺寸和间距
            paddingLeft = config.paddingLeft;
            paddingTop = config.paddingTop;
            cellwidth = config.cellWidth + config.cellRightLeftSpacing;
            cellHeight = config.cellHeight + config.cellUpDownSpacing;
            visibleColumns = config.columns;
            visibleRows = config.rows;

            int allItemCount = continer.GetCount();
            int pages = allItemCount == 0 ? 1 : Mathf.CeilToInt(allItemCount / (float)LeastCount);//得到网格的页数
            int gridCount = (visibleRows + 2 * extraRows + 1) * visibleColumns;
            //选取最小的作为要实例化的网格数量（实际格子比无限滚动需要的多就会选择无限滚动的，如果是更少，那就是只会实例化一页）
            gridCount = Math.Min(gridCount, pages * LeastCount);
            //实例化格子
            for (int i = 0; i < gridCount; i++)
            {
                gridManager.ActivateGrid(i);
            }
            Debug.Log($"激活格子数量：{gridManager.GetAllActiveGrids().Count()}");
            //让格子回到对应的位置上
            _ = UpdatePageGrid();
        }

        /// <summary>
        /// 更新当前页面的所有格子
        /// </summary>
        private async UniTask UpdatePageGrid()
        {
            //更新格子位置
            int startIndex = int.MaxValue;
            int endIndex = int.MinValue;
            foreach (var itemGrid in gridManager.GetAllActiveGrids())
            {
                startIndex = Math.Min(startIndex, itemGrid.Index);
                endIndex = Math.Max(endIndex, itemGrid.Index);
            }
            Debug.Log(startIndex + " " + endIndex);
            //真正更新位置方法
            RefleshGridPositon(startIndex, endIndex);

            //更新格子内容（遍历目前激活的格子更新）
            foreach (var itemGrid in gridManager.GetAllActiveGrids())
            {
                Item slot = continer.GetItem(itemGrid.Index);
                itemGrid.UpdateItemData(slot?.itemData, slot?.amount ?? 0);

                await UniTask.NextFrame();
            }
            Debug.Log("更新了整个页面");
        }
        
        #endregion

        #region 无限滚动背包实现
        private void OnScrollBegin(Vector2 _)
        {
            if (_isScrolling) return;          // 已经在追帧
            _isScrolling = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            CatchUpVisibility(_cts.Token).Forget();
        }
        /// <summary>
        /// 无限滚动的主逻辑
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTaskVoid CatchUpVisibility(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                UpdateVisibleSlots();
                await UniTask.Yield();          // 等下一帧，不卡主线程
                                                // 如果连续两帧没有变化就认为滑动结束
                if (parentScroll.velocity.magnitude < 0.1f)
                {
                    break;
                }
            }
            _isScrolling = false;
        }
        /// <summary>
        /// 无限滚动的实现逻辑
        /// </summary>
        private void UpdateVisibleSlots()
        {
            int nowRow = CalculateTopRow();
            if (nowRow == lastTopRow) return;
            bool isAbove = nowRow < lastTopRow;
            int lastTop = lastTopRow;
            lastTopRow = nowRow;
            //Debug.Log($"现在行号”{nowRow},历史行号{lastTopRow},是否向上滑动：{isAbove}");
            //理念是：前进方向拥有当前行以及缓冲行还有一行移动行；后退方向只有当前行以及缓冲行
            //也就是说其实就是在分配移动行，在这个脚本中移动行不可配置，固定一行
            //只需要根据公式计算出应该是哪一行，具体是否是有效行，具体执不执行脚本移除和添加行的方法去判断
            for (int offset = Math.Abs(nowRow - lastTop); offset >= 0; offset--)
            {
                int top = lastTop - extraRows - 1;                   //顶部行
                int buttom = lastTop + visibleRows + extraRows + 1;//底部行
                // 计算是向上滑动还是向下滑动
                if (isAbove)
                {
                    // 向上滑动
                    // 回收最底部移动格子
                    Debug.Log($"回收底部格子：第{buttom}行");
                    OnRemoveRows(buttom);

                    // 分配新的最顶部一行格子
                    //Debug.Log($"分配顶部格子：第{top}行");
                    OnAddRows(top);
                }
                else
                {
                    // 向下滑动
                    // 将最顶部的一行格子移除
                    OnRemoveRows(top);
                    //Debug.Log($"回收顶部格子：第{top}行");
                    // 分配新的最底部一行格子
                    OnAddRows(buttom);
                    Debug.Log($"分配底部格子：第{buttom}行");
                }
                lastTop += isAbove ? -1 : 1;
            }
        }
        
        #endregion
        
        /// <summary>
        /// 格子数据发生变化的时候就会事件触发调用
        /// </summary>
        /// <param name="changeIndex"></param>
        private void OnItemChanged(IEnumerable<int> changeIndex)
        {
            
            bool isInstantiate = false;
            int min = int.MaxValue;
            int max = int.MinValue;
            int count = continer.GetCount();
            //检查是否在显示范围内
            foreach (var index in changeIndex)
            {
                if (index < 0 || index > count)
                {
                    Debug.LogError("更新的索引无效，超过了合理范围：" + index);
                    continue;
                }

                //得到对应格子和对应数据，然后进行更新
                IItemGrid itemGrid = gridManager.GetGridByIndex(index);
                if (itemGrid == null)
                {
                    itemGrid = gridManager.ActivateGrid(index);
                    isInstantiate = true;
                }
                Item slot = continer.GetItem(index);
                itemGrid.UpdateItemData(slot?.itemData, slot?.amount ?? 0);
                //选出最大最小值
                min = Mathf.Min(min, index); max = Mathf.Max(max, index);
            }
            //如果更新过程中创建了格子就要更新格子位置
            if (isInstantiate)
            {
                //这里是为了能填满最后一行
                int totalSlotCount = continer.GetCount();
                int remainder = totalSlotCount % visibleColumns;
                int needGrid = remainder == 0 ? 0 : (visibleColumns - remainder);
                for (int i = max + 1; i < max + needGrid + 1; i++)
                {
                    var itemGrid = gridManager.ActivateGrid(i);
                    itemGrid.UpdateItemData(null, 0);
                }
                RefleshGridPositon(min, max + needGrid);
                
            }
            //调整scrollRect的content大小
            AdjustScrollRectCont();
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
            RectTransform contentRect = parentScroll.content;
            if (contentRect == null) return 0;
            // 滚动偏移量：Content向上滑，contentY为正，取绝对值（统一偏移方向为“向上滚动的距离”）
            float scrollOffsetY = Mathf.Abs(contentRect.anchoredPosition.y);

            // 实际有效偏移：减去顶部内边距（格子从paddingTop后开始排列，这部分偏移不算行号）
            float effectiveOffsetY = scrollOffsetY - paddingTop;

            // 防止effectiveOffsetY为负（还没滚动到paddingTop的位置，行号为0）
            effectiveOffsetY = Mathf.Max(effectiveOffsetY, 0);

            // 顶部行号 = 有效偏移 ÷ 每行高度 → 向下取整（确保行号连续）
            int topRow = Mathf.FloorToInt(effectiveOffsetY / cellHeight);

            // 边界修正：不能小于0
            return Mathf.Max(topRow, 0);
        }
        
        /// <summary>
        /// 更新指定范围的格子的位置
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        private void RefleshGridPositon(int startIndex, int endIndex)
        {
            if (startIndex > endIndex) Debug.LogWarning("RefleshGridPositon中最小值比最大值还大: " + startIndex + " | " + endIndex);
            for (int i = startIndex; i < endIndex + 1; i++)
            {
                int row = i / visibleColumns;
                int col = i % visibleColumns;
                var g = gridManager.GetGridByIndex(i);
                if (g == null)
                {
                    Debug.Log("没找到对应的格子: "+ i);
                    continue;
                }
                Transform grid = g.GridSelfObject.transform;
                SetGridPosition(grid,row,col);
            }
            void SetGridPosition(Transform grid, int row, int col)
            {
                var target = grid.transform as RectTransform;
                float posX = paddingLeft + col * cellwidth;
                float posY = -(paddingTop + row * cellHeight);
                target.anchoredPosition = new Vector2(posX, posY);
            }
        }
        
        /// <summary>
        /// 移除整行顶部或者底部的格子
        /// </summary>
        /// <param name="isabove">是否从上方移除</param>
        private void OnRemoveRows(int targetRow)
        {
            if (targetRow < 0 || targetRow > TotalRows) return;
            //Debug.Log($"当前底部行号为“{CalculateBottomRow()}要移除的行号为： " + targetRow);
            for (int col = 0; col < visibleColumns; col++)
            {
                // 计算从哪个索引回收，放回对象池并置空字典引用
                int fromIndex = CalculateIndex(targetRow, col);
                var grid = gridManager.GetGridByIndex(fromIndex);
                if (grid == null) continue;
                //Debug.Log($"移除了格子,索引： {fromIndex} ");
                gridManager.RecycleGrid(grid);
            }
        }
        /// <summary>
        /// 添加整行不可见格子
        /// </summary>
        /// <param name="isabove">是否从上方添加</param>
        private void OnAddRows(int targetRow)
        {
            if (targetRow < 0 || targetRow > TotalRows) return;
            //Debug.Log($"是否在上方添加格子{isabove}，检测到顶部/底部的行号为：{nowRow} 要添加的行号为： " + targetRow);
            Debug.Log($"添加了格子，行号为{targetRow}");
            for (int col = 0; col < visibleColumns; col++)
            {
                //从对象池中取出格子并计算将要分配的索引，然后重新绑定索引和格子接口对象的绑定
                int toIndex = CalculateIndex(targetRow, col);
                //如果要添加的格子之前并不存在，那就从对象池中取出
                IItemGrid itemGrid = gridManager.GetGridByIndex(toIndex);
                if (itemGrid == null) itemGrid = gridManager.ActivateGrid(toIndex);

                // 更新格子数据
                var item = continer.GetItem(toIndex);
                itemGrid.UpdateItemData(item?.itemData, item?.amount ?? 0);

            }
            var a = CalculateIndex(targetRow, 0);
            RefleshGridPositon(a, a + visibleColumns - 1);
        }
        
        private void AdjustScrollRectCont()
        {
            int row = Mathf.Max(TotalRows, visibleRows);
            float length = row * cellHeight + paddingTop;
            parentScroll.content.sizeDelta = new Vector2(parentScroll.content.sizeDelta.x, length);
        }
        #endregion
    
    }
}
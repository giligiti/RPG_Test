namespace InventorySystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public class GridManager
    {
        private Dictionary<int, IItemGrid> slotDict = new Dictionary<int, IItemGrid>();
        private LinkedList<IItemGrid> activeSlots = new LinkedList<IItemGrid>();
        private GameObject prefabObject;// 格子预制体
        private RectTransform content;     // 格子父物体
        private IGridPool gridPool;     // 你的对象池接口

        // 初始化（只调用一次）
        public GridManager(IGridPool gridPool, GameObject gameObject, RectTransform content)
        {
            this.gridPool = gridPool; // 注入对象池，解耦
            this.prefabObject = gameObject;
            this.content = content;
        }

        #region 对外暴露的核心方法
        /// <summary>
        /// 激活格子：自动同步接口Index、字典、链表
        /// </summary>
        /// <param name="targetIndex">目标索引</param>
        public IItemGrid ActivateGrid(int targetIndex)
        {
            // 1. 从对象池取格子（复用你的对象池逻辑）
            IItemGrid idleGrid = gridPool.GetIdleGrid();
            if (idleGrid == null)
            {
                var obj = GameObject.Instantiate(prefabObject, content);
                idleGrid = obj.GetComponent<IItemGrid>();
            }

            // 2. 自动同步接口Index（格子自记录）
            idleGrid.Index = targetIndex;

            // 3. 自动同步字典（索引→格子映射）
            if (slotDict.ContainsKey(targetIndex))
                slotDict[targetIndex] = idleGrid;
            else
                slotDict.Add(targetIndex, idleGrid);

            // 4. 自动同步链表（激活格子列表）
            activeSlots.AddLast(idleGrid);

            return idleGrid;
        }

        /// <summary>
        /// 回收格子：自动同步接口Index、字典、链表
        /// </summary>
        /// <param name="grid">要回收的格子</param>
        public void RecycleGrid(IItemGrid grid)
        {
            if (grid == null) return;

            int targetIndex = grid.Index;

            // 1. 自动同步链表（从激活列表移除）
            activeSlots.Remove(grid);

            // 2. 自动同步接口Index（清空格子索引）
            grid.Index = -1;
            grid.ResetGrid(); // 重置UI

            // 3. 自动同步字典（置空映射）
            if (slotDict.ContainsKey(targetIndex))
                slotDict[targetIndex] = null;

            // 4. 放回对象池（复用你的对象池逻辑）
            gridPool.ReturnGrid(grid);
        }
        #endregion

        #region 对外暴露的查询/更新方法（按需调用）
        /// <summary>
        /// 页面全量更新：遍历激活链表（小范围，高性能）
        /// </summary>
        public IEnumerable<IItemGrid> GetAllActiveGrids()
        {
            return activeSlots; // 直接返回链表，外部遍历更新
        }

        /// <summary>
        /// 单点数据更新：字典O(1)查找（不用遍历）
        /// </summary>
        public IItemGrid GetGridByIndex(int targetIndex)
        {
            slotDict.TryGetValue(targetIndex, out var grid);
            return grid;
        }
        #endregion
    }

    // 辅助接口：对象池解耦（你的对象池实现这个接口即可）
    public interface IGridPool
    {
        IItemGrid GetIdleGrid(); // 取闲置格子
        void ReturnGrid(IItemGrid grid); // 放回格子
    }
}
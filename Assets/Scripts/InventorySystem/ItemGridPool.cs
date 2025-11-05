using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

public class ItemGridPool : IGridPool
{
    private readonly Queue<IItemGrid> poolQueue = new Queue<IItemGrid>();

    public int Count => poolQueue.Count;
    public IItemGrid GetIdleGrid()
    {
        if (poolQueue.Count > 0)
        {
            var obj = poolQueue.Dequeue();
            if (obj != null)
                obj.GridSelfObject.SetActive(true);
            return obj;
        }
        return null;
    }

    public void ReturnGrid(IItemGrid obj)
    {
        if (obj == null) return;
        obj.GridSelfObject.SetActive(false);
        poolQueue.Enqueue(obj);
    }
}
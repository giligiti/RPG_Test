using System.Collections.Generic;
using UnityEngine;

public abstract class PoolData
{
    //代表每一个池子对象类型的父对象
    public GameObject Father;

    protected int least = 5;    
    public virtual int Least { get { return least ; } set { least = value; } }//用来控制最小保留值//重写可以实现改变默认最小值
    private int getSetCount = 0;
    /// <summary>
    /// 频率
    /// </summary>
    public virtual int GetSetCount {  get { return getSetCount; } set { getSetCount = Mathf.Max(value,0); } }

    /// <summary>
    /// 对象池数量
    /// </summary>
    public int PoolListCount => poolList.Count;

    private Queue<GameObject> poolList;
    //显示声明无参构造函数,以防万一
    public PoolData() { }
    //传入的是要存储的对象（用来取名）；场景上创建的对象池（用来设置父对象）
    public void InitPoolData(string name, GameObject poolObj)
    {
        Father = new GameObject(name);
        Father.transform.parent = poolObj.transform;
        poolList = new Queue<GameObject>();
    }
    //提前加载多个对象//异步加载//非使用者调用，一般加载过场景的时候调用
    /// <summary>
    /// 一般加载过场景的时候调用
    /// </summary>
    /// <param name="name">对象名</param>
    /// <param name="poolObj">对象池的父对象</param>
    /// <param name="objNum">需要提前加载的对象数量</param>
    /// <param name="path">对象的路径</param>
    public void InitPoolData(string name, GameObject poolObj, int objNum, string path = null)
    {
        InitPoolData(name, poolObj);
        for (int i = 0; i < objNum; i++)
        {
            //实例化相应数量的对象并进行存储
            SetValue(ObjectInstante(Father.name,path));
            getSetCount++;
        }
    }


    //取
    public virtual GameObject GetValue(string path = null)
    {
        getSetCount++;
        GameObject obj = null;
        //有对象
        if (poolList.Count > 0)
        {
            obj = poolList.Dequeue();
        }
        //没对象
        else
        {
            //实例化
            obj = ObjectInstante(Father.name, path);
        }
        // //取消父对象，直观地显示在层级面板上
        // obj.transform.parent = null;
        obj.SetActive(true);
        return obj;
    }
    //存
    public virtual void SetValue(GameObject obj)//虚函数让子类可以重写回收逻辑，用于特效粒子延时回收等特殊情况
    {
        getSetCount++;
        obj.transform.parent = Father.transform;
        obj.name = Father.name;
        //隐藏
        obj.SetActive(false);
        poolList.Enqueue(obj);
    }
    //实例化方法，让子类可以重写实例化方法，实现实例化的多样性
    protected abstract GameObject ObjectInstante(string key,string path = null);
    ////异步实例化方法 
    //protected GameObject ObjecsInstanteAsync(string key)
    //{

    //}
    
}
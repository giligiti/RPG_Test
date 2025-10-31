using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


//对象池管理员
//负责把对象加入对象池，以及从对应的对象池中取出
//Y是对象池管理员管理的具体对象池类型，如MonsterPoolData  如果是取出或者存入Monster类型(BaseMonster)则Y就是MonsterPoolData
//调用对象池需要指明用哪种对象池。不同的对象池的实例化方法不同
//当执行类似游戏暂停操作的时候，需要调用停止定期清理的方法
public class PoolMgr : BaseManager<PoolMgr> 
{
    //对象池物体，用作父物体
    private GameObject Pool;
    private PoolMgr()
    {
        Pool = new GameObject("PoolMgr");
    }
    private Coroutine clearCheckCoroutine;

    private int clearNum = 5;            //每次清理需要清理的对象池成员数量
    private int clearFrequency = 10;     //每次清理需要减少频率的值
    private int clearThresholds = 10;    //清理阈值（频率），低于该值则会被清理
    private int clearDelayTime = 300;    //对象池定期清理的时间，单位：秒；
    /// <summary>
    /// 对象池定期清理的时间
    /// </summary>
    public int ClearDelayTime { get { return clearDelayTime; }set { clearDelayTime = Mathf.Max(value, 5); } }
    //缓存池容器
    private Dictionary<string, PoolData> PoolDic = new Dictionary<string, PoolData>();
    #region 取出对象相关

    public GameObject GetObject<X, Y>(string path = null) where Y : PoolData, new()
    {
        string name = nameof(X);
        return GetObj<Y>(name, path);
    }

    /// <summary>
    /// 取出对象的方法重载
    /// </summary>
    /// <param name="key">对象池的名字</param>
    /// <returns></returns>
    public GameObject GetObject<Y>(string key, string path = null) where Y : PoolData, new()
    {
        return GetObj<Y>(key, path);
    }
    //真正取出的方法
    private GameObject GetObj<Y>(string key, string path = null) where Y : PoolData, new()
    {
        FirstCheck();

        GameObject obj = null;
        //如果不存在键
        if (!PoolDic.ContainsKey(key))
        {
            Y pool = new Y();
            pool.InitPoolData(key, Pool);
            PoolDic.Add(key, pool);
        }

        obj = PoolDic[key].GetValue(path);
        obj.name = key;
        return obj;
    }

    //提前实例化多个物体（不取出，自动失活）
    /// <summary>
    /// 过场景时提前加载多个对象
    /// </summary>
    /// <param name="key">对象名</param>
    /// <param name="objNum">需要生成的数量</param>
    /// <param name="path">资源路径</param>
    public void AdvanceInstantite<Y>(string key, int objNum, string path = null) where Y : PoolData, new()
    {
        AdvanceInit<Y>(key, objNum, path);
    }
    /// <summary>
    /// 过场景时提前加载多个对象的泛型方法，T为对象池名
    /// </summary>
    /// <param name="objNum">需要生成的数量</param>
    /// <param name="path">资源路径</param>
    public void AdvanceInstantite<T,Y>(int objNum, string path = null) where Y : PoolData, new()
    {
        string key = typeof(T).Name;
        AdvanceInit<Y>(key, objNum, path);
    }
    //跳过对象池管理自身的取出方法，直接调用对象池内部的实例化多个对象的方法
    private void AdvanceInit<Y>(string key, int objNum, string path = null) where Y : PoolData, new()
    {
        FirstCheck();
        //如果不存在键
        if (!PoolDic.ContainsKey(key))
        {
            Y pool = new Y();
            //实例化对应数量的对象并自动进行存储
            pool.InitPoolData(key, Pool, objNum, path);
        }
        else
        {
            PoolDic[key].InitPoolData(key, Pool, objNum, path);
        }
    }

    #endregion

    #region 存储对象相关

    //存
    public void PushObject<T,Y>(GameObject obj) where Y : PoolData, new()
    {
        FirstCheck();
        string key = typeof(T).Name;
        //如果不存在键
        if (!PoolDic.ContainsKey(key))
        {
            Y pool = new Y();               //创建新对象池
            pool.InitPoolData(key, Pool);   //初始化该对象池
            PoolDic.Add(key, pool);         //把对象加入对象池
        }

        PoolDic[key].SetValue(obj);
    }
    public void PushObject<Y>(string name, GameObject obj) where Y : PoolData, new()
    {
        FirstCheck();
        //如果不存在键
        if (!PoolDic.ContainsKey(name))
        {
            Y pool = new Y();               //创建新对象池
            pool.InitPoolData(name, Pool);   //初始化该对象池
            PoolDic.Add(name, pool);         //把对象加入对象池
        }

        PoolDic[name].SetValue(obj);

    }

    #endregion

    #region 清理对象池相关

    //清空缓存池的操作
    public void ClearPool()
    {
        PoolDic.Clear();
        PoolDic = null;
        Pool = null;
    }

    //定期检查清理对象池
    IEnumerator IEWaitForDelyTime()
    {
        yield return new WaitForSecondsRealtime(clearDelayTime);
        MonoMgr.Instance.StartCoroutine(IEClearPool());
        clearCheckCoroutine = MonoMgr.Instance.StartCoroutine(IEWaitForDelyTime());//反复开启自身，实现每次定期检查
    }
    IEnumerator IEClearPool()
    {
        Debug.Log("开始清理");
        //查找出需要删除的对象池在再一起删除
        List<string> keyList = new List<string>();  
        foreach (var pool in PoolDic)
        {
            //频率低于阈值的且数量大于最小保持数量的对象池进行记录清理
            if (pool.Value.GetSetCount < clearThresholds && pool.Value.PoolListCount > pool.Value.Least)
            {
                keyList.Add(pool.Key);
            }
            else
            {
                //每次执行定期清理就会减少频率;
                pool.Value.GetSetCount -= clearFrequency; 
            }
            yield return null;
        }
        //确定要清理的才会进入这个循环
        foreach (var key in keyList)
        {
            Debug.Log(key);
            //池子中成员数量少于阈值则销毁后直接全部清理
            int i = PoolDic[key].PoolListCount <= clearNum? PoolDic[key].Least: PoolDic[key].PoolListCount - clearNum;

            //while循环条件确保了即使确定要删除对象池后中途哪怕有新的存入对象和取出对象也会正常运作
            //删除后存取的逻辑由对象池管理器脚本的get和push方法保障会创建新键
            //对象池会清理直到数量减少到最小值
            while (PoolDic[key].PoolListCount > i )
            {
                GameObject obj = PoolDic[key].GetValue();
                obj.SetActive(false);
                Object.Destroy(obj);
                yield return null;
            }

        }
    }
    /// <summary>
    /// 停止定期清理检查
    /// </summary>
    public void StopClearCheck()
    {
        if (clearCheckCoroutine != null)    
        MonoMgr.Instance.StopCoroutine(clearCheckCoroutine);
    }
    /// <summary>
    /// 开启定期清理检查
    /// 默认第一次创建对象池的时候开始才开始定期检查
    /// </summary>
    public void StartClearCheck()
    {
        clearCheckCoroutine = MonoMgr.Instance.StartCoroutine(IEWaitForDelyTime());
    }

    #endregion

    private void FirstCheck()
    {
        if (clearCheckCoroutine == null)
            StartClearCheck();
    }


}

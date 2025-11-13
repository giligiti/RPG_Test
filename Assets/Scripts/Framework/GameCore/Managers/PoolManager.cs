using System.Collections.Generic;
using UnityEngine;

namespace Framework.Pool
{
    public class PoolManager : IModule
    {
        //对象池物体，用作父物体
        private GameObject Pool;
        private PoolManager()
        {
            Pool = new GameObject("PoolMgr");
        }
        //缓存池容器
        private Dictionary<string, PoolData> PoolDic = new Dictionary<string, PoolData>();
        #region 核心操作

//———————————————————————————————————取出对象——————————————————————————————————————————————————————
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

//———————————————————————————————————提前生成对象——————————————————————————————————————————————————————
        
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
        public void AdvanceInstantite<T, Y>(int objNum, string path = null) where Y : PoolData, new()
        {
            string key = typeof(T).Name;
            AdvanceInit<Y>(key, objNum, path);
        }
        //跳过对象池管理自身的取出方法，直接调用对象池内部的实例化多个对象的方法
        private void AdvanceInit<Y>(string key, int objNum, string path = null) where Y : PoolData, new()
        {
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


        #region 存储对象相关

        //存
        public void PushObject<T, Y>(GameObject obj) where Y : PoolData, new()
        {
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
        #endregion

        #region IModule接口实现
        public int Priority => throw new System.NotImplementedException();

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Run(float deltaTime)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }

    /// <summary>
    /// 可池化物体接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 当被取出的时候
        /// </summary>
        void OnTakeOut();
        /// <summary>
        /// 当被返回对象池的时候
        /// </summary>
        void OnReturn();
    }

}
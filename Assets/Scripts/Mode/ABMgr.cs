using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class ABMgr : BaseManager<ABMgr>
{
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
    private ABMgr()
    {

    }

    private AssetBundle mainBundle;             //存储主包
    private AssetBundleManifest manifest;       //存储主包的依赖关系
    //流文件地址字符串
    private string streamingPath
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    //主包名
    private string MainBundleName
    {
        get
        {
#if UNITY_ANDROID
            return "ANDROID";
#elif UNITY_IOS
            return "IOS";
#else
            return "PC";
#endif
        }
    }

    private void LoadBundle(string abName)
    {
        if (mainBundle == null)
        {
            mainBundle = AssetBundle.LoadFromFile(streamingPath + MainBundleName);
            manifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        if (manifest == null||mainBundle ==null)
        {
            Debug.Log("nono");
        }
        string[] bundles = manifest.GetAllDependencies(abName);//根据传入的名字得到包的依赖包的名字
        AssetBundle build;
        //加载所有依赖包
        for (int i = 0; i < bundles.Length; i++)
        {
            //判断是否加载过
            if (abDic.ContainsKey(bundles[i]))
                continue;
            build = AssetBundle.LoadFromFile(streamingPath + bundles[i]);
            abDic.Add(bundles[i], build);//存储

        }

        //加载包
        if (!abDic.ContainsKey(abName))//判断是否加载过
        {
            build = AssetBundle.LoadFromFile(abName);
            abDic.Add(abName, build);
        }
    }
    #region 同步加载
    //指定类型方法
    public Object LoadRes(string abName, string resName,Type types)
    {
        LoadBundle(abName);
        Object obj = abDic[abName].LoadAsset(resName , types);
        return obj;
    }
    //泛型方法
    public T LoadRes<T>(string abName, string resName) where T : Object
    {
        LoadBundle(abName);
        T obj = abDic[abName].LoadAsset<T>(resName);
        return obj;
    }

    #endregion

    #region 异步加载
    //异步加载传入ab包名，资源名，类型名，回调事件
    public void LoadResAsync(string abName, string resName, Type types, UnityAction<Object> events = null)
    {
        MonoMgr.Instance.StartCoroutine(RealLoadRes(abName, resName, types, events));
    }
    //泛型异步
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> events = null)where T : Object
    {
        MonoMgr.Instance.StartCoroutine(RealLoadRes<T>(abName, resName, events));
    }
    //协程
    private IEnumerator RealLoadRes(string name, string resName, Type type, UnityAction<Object> events = null)
    {
        LoadBundle(name);
        AssetBundleRequest abq = abDic[name].LoadAssetAsync(resName,type);
        yield return abq;
        //调用回调
        events?.Invoke(abq.asset);
    }
    //泛型协程
    private IEnumerator RealLoadRes<T>(string name, string resName, UnityAction<T> events = null) where T : Object
    {
        LoadBundle(name);
        AssetBundleRequest abq = abDic[name].LoadAssetAsync<T>(resName);
        yield return abq;
        //调用回调
        T b = abq.asset as T;
        events?.Invoke(b);
    }
    #endregion

    #region 资源包卸载
    //卸载单个包
    public void UnloadPack(string name)
    {
        if (!abDic.ContainsKey(name))
            return;
        abDic[name].Unload(false);//卸载
        abDic.Remove(name);//在字典中移除
    }

    //卸载所有包
    public void UnloadAllPack(string name)
    {
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();
    }

    #endregion



}


using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Framework.ToolSpace;
using UnityEngine;
using System.Threading;



namespace Framework.Resources
{
    public class ResManager : IModule
    {
        public int Priority => 10;
        Dictionary<string, Object> keepDic;
        LRUCache<string, Object> resCache;
        private string AssetPath => Application.dataPath.Replace("Assets/", "");
        CancellationTokenSource tokenSource;

        private ResManager(int capacity)
        {
            tokenSource = new();
            resCache = new(capacity);
            keepDic = new();
            //监听事件，当内部淘汰发生时，释放该资源
            resCache.eliminateEvent += (key, value) =>
            {
                Addressables.Release(value);
            };
        
        }
        public async UniTask<T> LoadRes<T>(string resKey, ResCacheType keepType = ResCacheType.Cache, System.Action<T> onSuccess = null) where T : Object
        {
            resKey = AssetPath + resKey;
            //如果曾经缓存过
            if (TryGetValue(resKey, out T value)) return value;
            //如果没有缓存过，就进行加载
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(resKey);

            try
            {
                await handle.ToUniTask(cancellationToken: tokenSource.Token);
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("加载任务已被取消");
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("资源管理器加载资源时出错" + ex.Message);
                throw;
            }
            finally
            {
                Addressables.Release(handle);
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"!!!警告，资源加载失败！具体情况： {handle.Status}");
                return null;
            }

            HandleResType(resKey, handle.Result, keepType);

            onSuccess?.Invoke(handle.Result);
            return handle.Result;
            }
        public void ReleaseRes(string resKey)
        {
            resKey = AssetPath + resKey;
            Object obj = default;
            if (keepDic.Remove(resKey, out obj) || resCache.RemoveWithoutChangeSort(resKey, out obj))
            {
                Addressables.Release(obj);
            }
        }

        public async UniTask LoadScene(string sceneKey, System.Action<float> onProcess,System.Action<Scene> onSuccess)
        {
            //是否已经加载？防止重复加载
            if (IsSceneLoaded(sceneKey))
            {
                var scene = SceneManager.GetSceneByPath(sceneKey);
                SceneManager.SetActiveScene(scene);
                onSuccess?.Invoke(scene);
                return;
            }
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneKey);
            CancellationToken token = tokenSource.Token;

            try
            {
                if (onProcess != null)
                {
                    // 每帧报告
                    while (!handle.IsDone && !token.IsCancellationRequested)
                    {
                        float percent = handle.PercentComplete;
                        onProcess?.Invoke(percent);
                        await UniTask.NextFrame();
                    }
                }
                else
                {
                    await handle.ToUniTask(cancellationToken: token);
                }

            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("加载任务已被取消");
                return;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("资源管理器加载场景时出现错误" + ex.Message);
            }
            finally
            {
                Addressables.Release(handle);
            }



            onProcess?.Invoke(1);                       //确保进度报告为100%

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"!!!警告，资源加载失败！具体情况： {handle.Status}");
                return;
            }

            onSuccess?.Invoke(handle.Result.Scene);
            Addressables.Release(handle);               //直接释放资源加载句柄
            }

        
        #region 辅助函数
        private bool TryGetValue<T>(string key, out T value)
        {
            Object item = default;
            if (resCache.TryGetValue(key, out item) || keepDic.TryGetValue(key, out item))
            {
                if (item is T target)
                {
                    value = target;
                    return true;
                }
            }
            value = default;
            return false;
        }

        private void HandleResType<T>(string key, T res, ResCacheType keepType) where T : Object
        {
            switch (keepType)
            {
                case ResCacheType.Cache:
                    resCache.Put(key, res);
                    break;
                case ResCacheType.Release:
                    break;
                case ResCacheType.Permanent:
                    keepDic.Add(key, res);
                    break;
                default:
                    Debug.LogError("出现了没有定于的枚举类型");
                    break;
            }
        }

        /// <summary>
        /// 判断场景是否已加载
        /// </summary>
        private bool IsSceneLoaded(string scenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.path == scenePath && scene.isLoaded)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
        
        #region IModule接口实现

        public void Dispose()
        {
            resCache.Clear();
            keepDic.Clear();
        }

        public void Init()
        {
        }
        #endregion
    }
    public enum ResCacheType
    {
        Cache,      //缓存
        Release,    //加载后不缓存
        Permanent   //永久持有
    }
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableTest : MonoBehaviour
{
    public AssetReferenceGameObject obj;
    public LazyLoadReference<GameObject> lazyLoad;
    public AssetReferenceGameObject sharphere;
    public List<Transform> posList;
    private IEnumerator<Transform> PosIE;

    void Awake()
    {
        PosIE = GetPos();
        AsyncOperationHandle<GameObject> operation = obj.InstantiateAsync();
        operation.Completed += InstanLoadSuccess;
        AsyncOperationHandle<GameObject> sphereHandle = sharphere.InstantiateAsync();
        sphereHandle.Completed += InstanLoadSuccess;

        // AsyncOperationHandle<GameObject> objHandle = obj.LoadAssetAsync<GameObject>();
        // objHandle.Completed += LoadSuccess;
        // AsyncOperationHandle<GameObject> sharpherehandle = sharphere.LoadAssetAsync<GameObject>();
        // sharpherehandle.Completed += LoadSuccess;


        // AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(obj);
        // handle.Completed += LoadSuccess;
        // AsyncOperationHandle<GameObject> handleSh = Addressables.LoadAssetAsync<GameObject>(sharphere);
        // handleSh.Completed += LoadSuccess;

        //动态加载
        // AsyncOperationHandle<GameObject> cubeHandle = Addressables.LoadAssetAsync<GameObject>("MainTestCube");
        // cubeHandle.Completed += LoadSuccess;
        // AsyncOperationHandle<GameObject> sphereHandle = Addressables.LoadAssetAsync<GameObject>("Sphere");
        // sphereHandle.Completed += LoadSuccess;


    }

    private void InstanLoadSuccess(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("物体实例化成功 :" + handle.Result.GetInstanceID());
            UniTask.Void(async () =>
            {
                await UniTask.Delay(5000);
                Debug.Log("一秒后打印");
                obj.ReleaseInstance(handle.Result);
            });
        }
    }

    private void LoadSuccess(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var objinstance = Instantiate(handle.Result, GetIE());
            Debug.Log($"物体: {objinstance.name}加载成功");
            Addressables.Release(handle);
        }
    }
    IEnumerator<Transform> GetPos()
    {
        foreach (var item in posList)
        {
            yield return item;
        }
    }
    Transform GetIE()
    {
        PosIE.MoveNext();
        return PosIE.Current;
    }
}
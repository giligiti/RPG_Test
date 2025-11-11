using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
//集中在此处处理场景加载相关方法和事件
public class ScenesLoadMgr
{
    public UnityEvent FrontEvent = new UnityEvent();
    public UnityEvent BackEvent = new UnityEvent();
    public UnityEvent<float> LoadingEvent = new UnityEvent<float>();
    private ScenesLoadMgr()
    {
        Debug.Log("——————创建了ScenesLoadMgr————————");
    }

    //提供给外部进行场景加载
    public void LoadScene(string name, UnityAction fun = null)
    {
        FrontEvent?.Invoke();

        SceneManager.LoadScene(name);

        BackEvent?.Invoke();

        //触发传入的方法
        fun?.Invoke();
    }
    //场景异步加载方法
    public void LoadSceneAsync(string name, UnityAction fun)
    {
        FrontEvent?.Invoke();
        MonoMgr.Instance.StartCoroutine(AsyncFun(name, fun));
    }
    private IEnumerator AsyncFun(string name, UnityAction fun)
    {
        //AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        //yield return ao;
        //fun?.Invoke();
        //也可以是下面这样
        AsyncOperation ao = SceneManager.LoadSceneAsync(name);
        ////表示异步加载未完成就会进入这个循环，和上面的yield return ao 一个效果
        while (!ao.isDone)
        {
            //进行进度条更新等操作
            LoadingEvent?.Invoke(ao.progress);
            yield return ao.progress;
        }
        yield return ao;
        BackEvent?.Invoke();
        fun?.Invoke();
        //RemoveListenerFunction();
    }
    // private void RemoveListenerFunction()
    // {
    //     FrontEvent.RemoveAllListeners();
    //     BackEvent.RemoveAllListeners();
    //     LoadingEvent.RemoveAllListeners();
    // }
}

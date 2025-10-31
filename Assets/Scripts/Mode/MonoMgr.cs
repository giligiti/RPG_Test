using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr :BaseManager<MonoMgr>
{
    private MonoController monocontroller;
    //私有构造函数
    private MonoMgr()
    {
        GameObject obj = new GameObject("Mono");
        monocontroller = obj.AddComponent<MonoController>();
    }
    public void AddEventListener(UnityAction listener, E_LifeFunction function = E_LifeFunction.update)
    {
        monocontroller.AddEventListener(listener, function);
    }
    public void RemoveEventListener(UnityAction listener, E_LifeFunction function = E_LifeFunction.update)
    {
        monocontroller.RemoveEventListener(listener,function);
    }
    //实现协程的启用和关闭
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return monocontroller.StartCoroutine(routine);
    }
    public void StopCoroutine(IEnumerator routine)
    {
        monocontroller.StopCoroutine(routine);
    }
    //通过Coroutine来停止
    public void StopCoroutine(Coroutine routine)
    {
        monocontroller.StopCoroutine(routine);
    }
}

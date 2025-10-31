using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//添加事件监听：AddEventListener
//注册事件：调用方法需要传入带有一个obj参数引用的函数，obj是事件触发者，用于判断事件触发者的身份
//事件触发：TriggerEventListener
//事件发生：触发事件需要传入事件触发者本身或者相应脚本
public class EventCenter :BaseManager<EventCenter>
{
    private EventCenter()
    {

    }
    //事件容器
    //需要传入事件触发者本身
    private Dictionary<string, UnityAction<Object>> eventDic = new Dictionary<string, UnityAction<Object>>();
    //不需要传入事件触发者本身
    private Dictionary<string,UnityAction> eventNop = new Dictionary<string, UnityAction>();

    #region 需要传入事件触发者本身的监听和注销方法

    //事件注册
    public void AddEventListener(string name, UnityAction<Object> events)
    {
        if (eventDic.ContainsKey(name))
        {
            eventDic[name] += events;
        }
        else
        {
            eventDic.Add(name, events);
        }
    }

    //事件触发
    public void TriggerEventListener(string name , Object obj = null)
    {
        if (!eventDic.ContainsKey(name))
            return;
        eventDic[name].Invoke(obj);
    }

    #endregion

    #region 不需要传入事件触发者本身的监听注销方法重载

    //事件监听触发方法重载

    //事件注册
    public void AddEventListener(string name, UnityAction events)
    {
        if (eventDic.ContainsKey(name))
        {
            eventNop[name] += events;
        }
        else
        {
            eventNop.Add(name, events);
        }
    }
    //事件触发
    public void TriggerEventListener(string name)
    {
        if (!eventNop.ContainsKey(name))
            return;
        eventNop[name].Invoke();
    }

    #endregion
    //用于加载场景时清空事件中心，防止内存泄露
    public void ClearEventCenter()
    {
        eventDic.Clear();
        eventNop.Clear();
    }

}

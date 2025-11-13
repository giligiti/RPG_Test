using System;
using System.Collections.Generic;

namespace Framework.Event
{
    /// <summary>
    /// 管理整个项目大部分的事件触发逻辑（非线程安全）
    /// </summary>
    public class EventBus : IModule
    {
        public int EventCount => eventDic.Count;
        private static int idDistributor = 0;
        //监听者容器
        Dictionary<Type, List<(int,IEventHandle)>> eventDic;
        private EventBus()
        {
            eventDic = new();
        }

        //分发监听者持有对象的ID
        private int GetHandleID()
        {
            return idDistributor++;
        }
//—————————————————————————————————————事件发布—————————————————————————————————————————————————————————
        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="eventdata"></param>
        public void PublishEvent(Type msgType, IEvent eventdata)
        {
            // 查询字典中是否有监听者，如有则遍历监听者列表，取出其中对应的IEventHandle对象
            // 通过其内部方法调用触发存储于其中的委托
            if (eventDic.TryGetValue(msgType, out var handler))
            {
                foreach (var hand in handler)
                    hand.Item2.HandleEvnet(eventdata);
            }
        }
        /// <summary>
        /// 触发事件重载
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="eventdata"></param>
        public void PublishEvent(IEvent eventdata)
        {
            Type msgType = eventdata.GetType();
            PublishEvent(msgType, eventdata);
        }

        //—————————————————————————————————————事件订阅—————————————————————————————————————————————————————————


        /// <summary>
        /// 订阅
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="action"></param>
        /// <return>用于取消订阅的接口<return>
        public IDisposable SubscribeEvent<T>(Action<T> action) where T : class, IEvent
        {
            Type msgType = typeof(T);
            if (!eventDic.ContainsKey(msgType))
            {
                eventDic[msgType] = new List<(int, IEventHandle)>();
            }
            int handleID = GetHandleID();
            FunctionHandle<T> handle = new FunctionHandle<T>(handleID, action);
            eventDic[msgType].Add((handleID, handle));
            return handle;
        }
        //—————————————————————————————————————事件订阅取消—————————————————————————————————————————————————————————
        public void UnSubscribeEvent<T>(int id) where T : IEvent
        {
            Type t = typeof(T);
            //如果字典中有这个事件类型
            if (eventDic.TryGetValue(t, out var handleList))
            {
                //遍历监听者列表，寻找id对应的对象
                for (int i = handleList.Count - 1; i >= 0; i--)
                {
                    if (handleList[i].Item1 == id) handleList.RemoveAt(i);
                }
                //如果没人订阅了，那就释放这个事件的列表
                if (handleList.Count == 0) eventDic.Remove(t, out _);
            }
        }

        #region IModule接口实现

        public int Priority => 10;

        public void Dispose() => eventDic.Clear();

        public void Init() { }

        public void Run(float deltaTime) { }

        #endregion
    }
    /// <summary>
    /// 类对象缓存注册的委托函数
    /// 在消息发布的时候调用对应的对象执行HandleEven方法
    /// 该方法会调用缓存的委托，注入参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FunctionHandle<T> : IEventHandle, IDisposable where T : class, IEvent
    {
        private WeakReference<Action<T>> action;
        private int subscriberID;
        public FunctionHandle(int id,Action<T> action)
        {
            this.action = new WeakReference<Action<T>>(action);
            subscriberID = id;  
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <typeparam name="T1"></typeparam>
        public void HandleEvnet<T1>(T1 eventData) where T1 : IEvent
        {
            //类型检查
            if (eventData is T typedEvent && action.TryGetTarget(out var target))
            {
                target.Invoke(typedEvent);
            }
            else
            {
                UnityEngine.Debug.LogError("消息类型不匹配");
            }
        }
        /// <summary>
        /// 订阅者通过该方法取消订阅事件
        /// </summary>
        public void Dispose()
        {
            GameCore.EventBus.UnSubscribeEvent<T>(subscriberID);
        }
        public void Reset()
        {
            action = null;
            subscriberID = -1;
        }
    }
    /// <summary>
    /// 事件接口
    /// </summary>
    public interface IEvent { }

    public interface IEventHandle
    {
        public void HandleEvnet<T>(T eventdata) where T : IEvent;
    }
}

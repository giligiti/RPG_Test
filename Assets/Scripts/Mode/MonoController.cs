using UnityEngine;
using UnityEngine.Events;

public class MonoController : MonoBehaviour
{
    private UnityEvent MonoAwakeEvent;
    private UnityEvent MonoOnEnableEvent;
    private UnityEvent MonoStartEvent;
    private UnityEvent MonoFixUpdateEvent;
    private UnityEvent MonoUpdateEvent;
    private UnityEvent MonoLaterUpdateEvent;
    private UnityEvent MonoOnDisableEvent;

    #region 生命周期函数

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        MonoAwakeEvent?.Invoke();
    }
    private void OnEnable()
    {
        MonoOnEnableEvent?.Invoke();
    }
    private void Start()
    {
        MonoStartEvent?.Invoke();
    }
    private void FixedUpdate()
    {
        MonoFixUpdateEvent?.Invoke();
    }
    // Update is called once per frame
    void Update()
    {
        MonoUpdateEvent?.Invoke();
    }
    private void LateUpdate()
    {
        MonoLaterUpdateEvent?.Invoke();
    }
    private void OnDisable()
    {
        MonoOnDisableEvent?.Invoke();
    }

    #endregion

    /// <summary>
    /// 添加事件监听，选择注册在哪个生命周期函数中
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="function">枚举表示生命周期</param>
    public void AddEventListener(UnityAction listener, E_LifeFunction function)
    {
        UnityEvent MonoEvent = DecideFunction(function);
        MonoEvent.AddListener(listener);
    }

    public void RemoveEventListener(UnityAction listener, E_LifeFunction function)
    {
        //要注销必先注册，如果出问题说明没有注册
        UnityEvent MonoEvent = DecideFunction(function);
        MonoEvent.RemoveListener(listener);
    }

    private UnityEvent DecideFunction(E_LifeFunction function)
    {
        switch (function)
        {
            case E_LifeFunction.awake: return MonoAwakeEvent ??= new UnityEvent();
            case E_LifeFunction.onenable: return MonoOnEnableEvent ??= new UnityEvent();
            case E_LifeFunction.start: return MonoStartEvent ??= new UnityEvent();
            case E_LifeFunction.fixUpdate: return MonoFixUpdateEvent ??= new UnityEvent();
            case E_LifeFunction.update: return MonoUpdateEvent ??= new UnityEvent();
            case E_LifeFunction.laterUpdate: return MonoLaterUpdateEvent ??= new UnityEvent();
            case E_LifeFunction.ondisable: return MonoOnDisableEvent ??= new UnityEvent();
            default: return MonoUpdateEvent ??= new UnityEvent();
        }
    }
}
public enum E_LifeFunction
{
    awake,
    onenable,
    start,
    fixUpdate,
    update,
    laterUpdate,
    ondisable,
}
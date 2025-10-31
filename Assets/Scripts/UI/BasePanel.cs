using UnityEngine;
using UnityEngine.Events;



[RequireComponent(typeof(CanvasGroup))]
public abstract class BasePanel : MonoBehaviour
{
    CanvasGroup canvasGroup;
    float alphaSpeed = 10f;
    bool isShow = false;
    UnityAction Delte;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        Init();
    }
    /// <summary>
    /// 注册事件，初始化等需要一开始就执行的命令就放在这里
    /// </summary>
    protected abstract void Init();

    //实现淡入
    public virtual void ShowMe()
    {
        isShow = true;
        canvasGroup.alpha = 0;
    }
    public virtual void HideMe(UnityAction del)
    {
        isShow = false;
        Delte = del;
    }

    /// <summary>
    /// 调整面板透明度
    /// </summary>
    private void AlphaUpdate()
    {
        if (isShow && canvasGroup.alpha != 1)
        {
            canvasGroup.alpha = Mathf.Min(canvasGroup.alpha + Time.deltaTime * alphaSpeed, 1);
        }
        else if (!isShow && canvasGroup.alpha != 0)
        {
            canvasGroup.alpha = Mathf.Max(canvasGroup.alpha - Time.deltaTime * alphaSpeed, 0);
            //淡出完成后调用
            if (canvasGroup.alpha == 0)
            {
                Delte?.Invoke();
            }
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
        AlphaUpdate();
    }
}
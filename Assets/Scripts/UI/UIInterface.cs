using System;
using UnityEngine;

public interface UIView
{
    /// <summary>
    /// 展示UI
    /// </summary>
    /// <param name="action">UI展示完成后要做的事情</param>
    public void ShowUI(Action action = null);
    /// <summary>
    /// 关闭UI
    /// </summary>
    /// <param name="action">UI关闭完成后要做的事情</param>
    public void HideUI(Action action = null);
}
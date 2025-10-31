using System.Collections.Generic;
using UnityEngine;

public class UIManager : BaseManager<UIManager>
{

    //存储当前显示的面板
    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();
    private GameObject canvas;
    public GameObject Canvas => canvas;

    private UIManager()
    {
        if (canvas == null)
        {
            canvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        }
    }

    //显示面板//创建面板并且返回面板
    public T ShowPanel<T>() where T : BasePanel
    {

        string name = typeof(T).Name;
        //检查是否已经存在
        if (panelDic.TryGetValue(typeof(T).Name, out var value))
            return value as T;
        //实例化面板
        GameObject panelobj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + name));
        panelobj.transform.SetParent(canvas.transform, false);

        //得到的面板存起来
        T panel = panelobj.GetComponent<T>();
        panelDic.Add(name, panel);

        panel.ShowMe();

        return panel;

    }

    //隐藏面板//是否淡出面板，隐藏面板，传入UnityAction销毁面板
    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
        if (isFade)
        {
            if (panelDic[typeof(T).Name])
            {
                panelDic[typeof(T).Name].HideMe(() =>
                {
                    GameObject.Destroy(panelDic[typeof(T).Name].gameObject);
                    panelDic.Remove(typeof(T).Name);
                });

            }
        }
        else
        {
            GameObject.Destroy(panelDic[typeof(T).Name].gameObject);
            panelDic.Remove(typeof(T).Name);
        }
    }


    //得到面板
    public T GetPanel<T>() where T : BasePanel
    {
        if (panelDic.TryGetValue(typeof(T).Name, out var value))
            return value as T;
        return null;
    }
}

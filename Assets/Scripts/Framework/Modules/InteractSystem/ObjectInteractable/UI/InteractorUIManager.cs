using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    /// <summary>
    /// 交互UI管理器
    /// </summary>
    public class InteractorUIManager
    {
        //存储所有的实例化的提示UI物体
        Dictionary<E_InteractUIType, (GameObject, IInteractUIStand)> objDic;
        Dictionary<GameObject, UIView> uiDic;
        public InteractorUIManager()
        {
            objDic = new(4);
            uiDic = new(4);
        }

        private GameObject activeObj;           //表示当前已经显示的提示UI
        //展示
        public GameObject ShowUI(InteractTipSO config, Action action = null)
        {
            if (config == null) return null;

            GameObject obj;
            (GameObject, IInteractUIStand) kv;
            #region  创建UI
            //如果ui没被创建，则调用配置文件中的逻辑进行创建
            if (!objDic.TryGetValue(config.uiType, out kv))
            {
                kv = config.CreatObj();
                objDic.Add(config.uiType, kv);
            }
            //如果已经被创建，则根据配置文件进行更新
            else
            {
                config.InitUI(kv.Item2);
            }
            obj = kv.Item1;
            #endregion

            #region UI效果
            //如果是第一个提示UI,尝试获取ui展示接口,展示UI
            if (activeObj == null || !activeObj.activeSelf)
            {
                //尝试检测该view接口是否有提前存储过，如果有则无需getcomponent
                UIView view;
                if (uiDic.TryGetValue(obj, out view))
                {
                    view.ShowUI(action);
                }
                else if (obj.TryGetComponent(out view))
                {
                    uiDic.Add(obj, view);
                    view.ShowUI(action);
                }
                obj.SetActive(true);
            }
            //如果物体存在且已经激活，判断是否和当前需要的展示的UI是同一个物体，如果不是就隐藏
            else if (activeObj != obj)
            {
                activeObj.SetActive(false);
                obj.SetActive(true);
                activeObj = obj;
            }
            #endregion

            // 临时
            var panel = UIManager.Instance.ShowPanel<InteractPanel>();
            // 设置父对象和位置
            obj.transform.SetParent(panel.interactPoint.transform, false);
            obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return obj;
        }
        //隐藏
        public void HideUI(InteractTipSO config,Action action = null)
        {
            //检查是否存在这个交互UI类型的物体，并检查是否有UIview来触发UI的隐藏动画
            if (objDic.TryGetValue(config.uiType, out var kv))
            {
                GameObject obj = kv.Item1;
                UIView view;
                if (uiDic.TryGetValue(obj, out view))
                {
                    action += () => obj.SetActive(false);
                    view.HideUI(action);
                }
                // //如果有物体不通过这个脚本进行创建，但是通过这个脚本进行隐藏
                // else if (obj.TryGetComponent(out view))
                // {
                //     Debug.LogWarning("物体不通过InteractorUIManager脚本进行创建，但是通过这个脚本进行隐藏,可能会存储不该缓存的接口对象");
                //     uiDic.Add(obj, view);
                //     action += () => obj.SetActive(false);
                //     view.HideUI(action);
                // }

                //如果物体身上没有找到注册过的UIView接口就会直接暴力触发委托并隐藏UI
                else
                {
                    action?.Invoke();
                    obj.SetActive(false);
                }
            }
        }
    }
}

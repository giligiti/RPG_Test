using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    public class InteractorFactory
    {
        //存储所有的实例化的提示UI物体
        Dictionary<Type, GameObject> objDic;
        public InteractorFactory() => objDic = new(4);

        private GameObject activeObj;           //表示当前已经显示的提示UI

        public GameObject ShowUI(InteractTipSO config, Action action = null)
        {
            if (config == null) return null;
            GameObject obj;
            //如果ui没被创建，则调用配置文件中的逻辑进行创建
            if (!objDic.TryGetValue(config.GetType(), out obj))
            {
                obj = config.CreatObj();
                objDic.Add(config.GetType(), obj);
            }
            //如果已经被创建，则根据配置文件进行更新
            else obj = config.InitObj(obj);
            //如果是第一个提示UI,尝试获取ui展示接口,展示UI
            if (activeObj == null || !activeObj.activeSelf)
            {
                if (obj.TryGetComponent<UIView>(out var view))
                    view.ShowUI(action);
                obj.SetActive(true);
            }
            //如果物体存在且已经激活，判断是否和当前需要的展示的UI是同一个物体，如果不是就隐藏
            else if (activeObj != obj)
            {
                activeObj.SetActive(false);
                obj.SetActive(true);
                activeObj = obj;
            }
            // 临时
            var panel = UIManager.Instance.ShowPanel<InteractPanel>();
            // 设置父对象和位置
            obj.transform.SetParent(panel.interactPoint.transform, false);
            obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return obj;
        }
        public void HideUI(InteractTipSO config)
        {
            if (objDic.TryGetValue(config.GetType(), out GameObject obj))
            {
                obj.SetActive(false);
            }
        }
        /// <summary>
        /// 隐藏当前显示的物体
        /// </summary>
        public void HideUI()
        {
            activeObj?.SetActive(false);
            Debug.Log("隐藏了交互UI");
        }
    }
}

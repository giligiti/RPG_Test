using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace InteractSystem
{
    /// <summary>
    /// 表示各种提示UI的配置文件
    /// </summary>
    [CreateAssetMenu(fileName = "InteractUIConfig", menuName = "交互系统/交互UI配置")]
    public class InteractTipSO : ScriptableObject
    {
        [Header("交互UI类型")]
        [Tooltip("不同结构的交互UI预制体，应该是不同的类型，是根据此类型进行对象池复用的")]
        public E_InteractUIType uiType;
        [Header("输入动作配置")]
        [Tooltip("把 Input Actions 资产里对应的 Action 拖进来")]
        public InputActionReference inputKeyAction;
        public GameObject interactUI;       //该提示配置的预制体
        [Header("配置对应的图片和文本")]
        public List<InteractPictureConfig> imgComponentConfigs;
        public List<InteractTextConfig> textComponentConfigs;
        [Header("布局配置")]
        public float posX;
        public float posY;
        /// <summary>
        /// 负责实例化，并初始化该配置的预制体
        /// </summary>
        /// <returns></returns>
        public (GameObject, IInteractUIStand) CreatObj()
        {
            var obj = GameObject.Instantiate(interactUI);
            var itf = obj.GetComponent<IInteractUIStand>();
            InitUI(itf);
            return (obj, itf);
        }

        public void InitUI(IInteractUIStand uiStand)
        {
            // 图片组件的初始化
            foreach (var item in imgComponentConfigs)
            {
                if (uiStand.TryGetComponent(item.componentType, out var uiGraphic))
                {
                    if (uiGraphic is Image image)
                    {
                        image.sprite = item.value;
                    }
                    else Debug.LogWarning("发生类类型转换错误，配置的枚举和组件上的组件不一致");
                }
                else Debug.LogWarning("该UI交互物体上没有找到对应的组件，请检查预制体的配置");
            }
            // 文字组件的初始化
            foreach (var item in textComponentConfigs)
            {
                if (uiStand.TryGetComponent(item.componentType, out var uiGraphic))
                {
                    if (uiGraphic is TextMeshProUGUI image)
                    {
                        image.text = item.value;
                    }
                    else Debug.LogWarning("发生类类型转换错误，配置的枚举和组件上的组件不一致");
                }
                else Debug.LogWarning("该UI交互物体上没有找到对应的组件，请检查预制体的配置");
            }
        }
    }
    //交互类型
    //决定监听的按键
    public enum E_InteractUIType
    {
        Normal,     //普通交互类型，点击即可
        Special,    //特殊交互类型，需要长按
    }
}
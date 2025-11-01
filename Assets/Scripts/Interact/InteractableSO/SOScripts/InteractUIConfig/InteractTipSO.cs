using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractSystem
{
    /// <summary>
    /// 表示各种提示UI的配置文件
    /// </summary>
    public abstract class InteractTipSO : ScriptableObject
    {
        [Header("输入动作配置")]
        [Tooltip("把 Input Actions 资产里对应的 Action 拖进来")]
        public InputActionReference inputKeyAction;
        public GameObject interactUI;       //该提示配置的预制体
        [Header("布局配置")]
        public float posX;
        public float posY;
        /// <summary>
        /// 负责实例化，并初始化该配置的预制体
        /// </summary>
        /// <returns></returns>
        public abstract GameObject CreatObj();
        public abstract GameObject InitObj(GameObject obj);

    }
    //交互类型
    //决定监听的按键
    public enum E_InteractType
    {
        Normal,     //普通交互类型，点击即可
        Special,    //特殊交互类型，需要长按
    }
}
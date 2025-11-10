using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
namespace InteractSystem
{
    /// <summary>
    /// 负责与可交互物体交互
    /// </summary>
    [RequireComponent(typeof(InteractObjFind))]
    public class MainInteractor : MonoBehaviour
    {
        //输入相关
        private InputSystem inputAction;

        private InteractObjFind interactObjFind;                    //寻找交互接口的脚本对象
        private InteractInterfaceHandle interfaceHandle;            //当前选中的的交互接口 
        private InteractorUIManager interUIFactory;                   //创建提示UI的对象
        //玩家是否处于可交互状态
        private bool isInteract = true;
        public bool IsInteract => isInteract;

        #region 初始化
        void Awake()
        {
            inputAction = new();
            inputAction.Enable();
            interUIFactory = new();
            interactObjFind = GetComponent<InteractObjFind>();
        }
        void OnEnable()
        {
            inputAction.Enable();
            inputAction.Player.Interact.started += InteractInputStart;
            inputAction.Player.Interact.canceled += InteractInputStop;
        }
        void OnDisable()
        {
            inputAction.Player.Interact.started -= InteractInputStart;
            inputAction.Player.Interact.canceled -= InteractInputStop;
            inputAction.Disable();
        }
        void Start()
        {
            //开始检测
            interactObjFind.StartCheck(ReceiveHandle, null);//使用默认排序方法（根据距离）
        }

        #endregion

        #region 可交互对象的进入与离开
        /// <summary>
        /// 检测到交互接口对象
        /// </summary>
        /// <param name="arg0"></param>
        /// 检测到则触发接口：IOnInteractable(弹出交互UI);ISelectable(选中物体);
        private void ReceiveHandle(InteractInterfaceHandle arg0)
        {
            Debug.Log("主交互获取到有效的可交互物体");
            //如果没有检测到可交互物体
            if (arg0 == null) InteractableExit();
            else InteractableEnter(arg0);
            interfaceHandle = arg0;
        }
        /// <summary>
        /// 检测到交互物体
        /// </summary>
        /// <param name="arg0"></param>
        private void InteractableEnter(InteractInterfaceHandle arg0)
        {
            //根据获取的配置文件展示交互UI（自动处理已经存在的Ui图标的展示逻辑）
            if (arg0.TryGetInterface<IInteractUIable>(out var itf))
                interUIFactory.ShowUI(itf.ProvideConfig());

            //进行选中交互
            if (arg0.TryGetInterface<ISelectable>(out var selectable))
                selectable.OnSelect();

        }
        /// <summary>
        /// 未检测到交互物体
        /// </summary>
        /// <param name="arg0"></param>
        private void InteractableExit()
        {
            //这里interfaceHandle表示的是上一个可交互物体
            //Debug.Log("失去可交互物体");
            if (interfaceHandle == null) return;

            if (interfaceHandle.TryGetInterface<IInteractUIable>(out var itf))
                interUIFactory.HideUI(itf.ProvideConfig());

            if (interfaceHandle.TryGetInterface<ISelectable>(out var selectable))
                selectable.OnLoseSelect();
        }
        #endregion

        #region 进行交互
        private bool CanInteract()
        {
            //玩家当前处于不可交互状态则返回
            if (!isInteract) return false;
            //没有找到可交互物体则返回 物体处于不可交互状态则返回
            if (interfaceHandle == null) return false;
            return true;
        }
        //交互输入
        private void InteractInputStart(InputAction.CallbackContext context)
        {
            if (!CanInteract()) return;
            Debug.Log("按下了按键，开始进行交互");
            //触发交互开始接口
            if (interfaceHandle.TryGetInterface<IOnInteractStart>(out var itf))
            {
                if (itf.CanInteract && itf.ValidateInput(context)) itf.OnInteractStart(this);
            }
        }
        private void InteractInputStop(InputAction.CallbackContext context)
        {
            if (!CanInteract()) return;
            Debug.Log("松开了按键，停止交互");
            //触发交互结束接口
            if (interfaceHandle.TryGetInterface<IOnInteractStop>(out var itf))
            {
                itf.OnInteractStop(this);
            }
        }
        #endregion
    }
}


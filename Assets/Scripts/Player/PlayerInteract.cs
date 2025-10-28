using UnityEngine;
using UnityEngine.InputSystem;
namespace Test
{
    public class PlayerInteract : MonoBehaviour
    {
        //输入相关
        private InputSystem inputAction;


        //是否处于可交互状态
        public bool isInteract { get; }
        [SerializeField] private Transform interactPointPos;
        //交互的UI

        void OnEnable()
        {
            inputAction?.Enable();
        }
        void OnDisable()
        {
            inputAction.Disable();
        }
        // Start is called before the first frame update
        void Start()
        {
            inputAction = new();
            inputAction?.Enable();
        }

        // Update is called once per frame
        void Update()
        {

        }
        //创建交互
        private void CreatInteractUI()
        {
            
        }
    }
    /// <summary>
    /// 交互行为的规范接口
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 检查当前交互对象是否可交互（如是否在冷却、是否满足条件）
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// 执行交互逻辑
        /// </summary>
        /// <param name="interactor"></param>
        void OnInteract(PlayerInteract interactor);

        /// <summary>
        /// 获取交互提示文本（如“按F键打开”）
        /// </summary>
        /// <returns></returns>
        string GetInteractPrompt();

        /// <summary>
        /// 获取交互配置
        /// </summary>
        /// <returns></returns>
        InteractTipSO GetInteractConfig();
        

        // /// <summary>
        // /// 玩家看向物体时的反馈（显示UI提示）
        // /// </summary>
        // void OnFocus();

        // /// <summary>
        // /// 玩家移开视线时的清理（隐藏UI提示）
        // /// </summary>
        // void OnLoseFocus();
    }
}

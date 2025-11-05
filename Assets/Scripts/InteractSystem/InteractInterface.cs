using UnityEngine.UI;

namespace InteractSystem
{
    /// <summary>
    /// 交互物体接口的基类接口
    /// </summary>
    public interface IInteractableBase { }

    /// <summary>
    /// 会弹出交互UI提示的物体（能被检测到但是尚未开始交互的状态）
    /// </summary>
    public interface IInteractUIable : IInteractableBase
    {
        /// <summary>
        /// 检查当前交互对象是否可交互（如是否在冷却、是否满足条件）
        /// </summary>
        bool CanInteract { get; }
        /// <summary>
        /// 提供提示UI配置文件
        /// </summary>
        /// <returns></returns>
        InteractTipSO ProvideConfig();
    }
    /// <summary>
    /// 可交互物体的开始交互接口
    /// </summary>
    public interface IOnInteractStart : IInteractableBase
    {
        /// <summary>
        /// 检查当前交互对象是否可交互（如是否在冷却、是否满足条件）
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// 执行开始交互逻辑
        /// </summary>
        /// <param name="interactor"></param>
        void OnInteractStart(MainInteractor interactor);
        /// <summary>
        /// 内部验证（可选）：接收输入上下文，由物体自己判断是否符合触发条件（如某些物体需要长按，可在此处判断按压时长）
        /// </summary>
        /// <param name="context">输入上下文（包含按压状态、时长等）</param>
        /// <returns>是否满足内部输入条件</returns>
        bool ValidateInput(UnityEngine.InputSystem.InputAction.CallbackContext context); // 默认返回true
    }
    /// <summary>
    /// 可交互物体的停止交互接口
    /// </summary>
    public interface IOnInteractStop : IInteractableBase
    {
        /// <summary>
        /// 执行停止交互逻辑
        /// </summary>
        /// <param name="interactor"></param>
        void OnInteractStop(MainInteractor interactor);

    }
    /// <summary>
    /// 可以被选中的物体的接口
    /// </summary>
    public interface ISelectable : IInteractableBase
    {
        //选中的时候
        void OnSelect();
        //未选中的时候
        void OnLoseSelect();
    }
    /// <summary>
    /// 交互UI数据互通之间的标准，让交互UI预制体的脚本和配置能够联动，同时具有拓展性
    /// </summary>
    /// 这里是使用里氏替换，调用者需要把组件转换成需要的具体组件类型
    public interface IInteractUIStand
    {
        bool TryGetComponent(E_InteractUIComponentType uiType, out MaskableGraphic uiGraphic);
    }
}
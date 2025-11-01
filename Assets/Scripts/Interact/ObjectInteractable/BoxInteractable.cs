using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractSystem
{
    public class BoxInteractable : InteractableObject, IOnInteractStart, ISelectable, IOnInteractable, IOnInteractStop
    {
        [SerializeField]
        private InteractTipSO interactTipSO;
        public bool CanInteract => true;            //暂时设置，用于可以交互

        
        public InteractTipSO ProvideConfig() => interactTipSO;

        public void OnInteractStart(MainInteractor interactor)
        {
            Debug.Log($"!!!打开了宝箱{gameObject.name}");
        }
        public void OnInteractStop(MainInteractor interactor)
        {
            Debug.Log($"结束宝箱交互!!!{gameObject.name}");
        }

        public void OnSelect()
        {
            Debug.Log($"选中了宝箱{gameObject.name}");
        }
        public void OnLoseSelect()
        {
            Debug.Log($"取消选中宝箱{gameObject.name}");
        }

        public bool ValidateInput(InputAction.CallbackContext context)
        {
            //Debug.Log($"输入的动作是: {context.action.name};\n能够进行交互的动作是: {interactTipSO.inputKeyAction.action.name}");
            return context.action == interactTipSO.inputKeyAction.action;
        }
    }
}
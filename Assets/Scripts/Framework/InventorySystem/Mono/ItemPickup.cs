using InteractSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InventorySystem
{
    public class ItemPickup : InteractableObject, IInteractUIable, IOnInteractStart, ISelectable
    {
        public bool CanInteract => throw new System.NotImplementedException();
        
        #region ISelectable接口
        public void OnLoseSelect()
        {
            throw new System.NotImplementedException();
        }

        public void OnSelect()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region IOnInteractStart接口
        public void OnInteractStart(MainInteractor interactor)
        {
            throw new System.NotImplementedException();
        }
        public InteractTipSO ProvideConfig()
        {
            throw new System.NotImplementedException();
        }

        public bool ValidateInput(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
        }
        #endregion
        
    }

}
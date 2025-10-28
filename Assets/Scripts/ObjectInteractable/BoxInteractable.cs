using UnityEngine;

namespace Test
{
    public class BoxInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private InteractTipSO interactTipSO;
        public bool CanInteract => throw new System.NotImplementedException();

        public InteractTipSO GetInteractConfig() => interactTipSO;

        public string GetInteractPrompt() => "打开";
        public void OnInteract(PlayerInteract interactor)
        {


        }

        #region 无用
        
        #endregion

    }
}
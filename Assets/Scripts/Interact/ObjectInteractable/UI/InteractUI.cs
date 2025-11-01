using System;
using UnityEngine;

namespace InteractSystem
{
    public class InteractUI : MonoBehaviour, UIView
    {
        public void HideUI(Action action = null)
        {
            action?.Invoke();
        }

        public void ShowUI(Action action = null)
        {
            action?.Invoke();
        }
    }
}

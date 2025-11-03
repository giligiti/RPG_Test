using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InteractSystem
{
    public class InteractUI : MonoBehaviour, UIView, IInteractUIStand
    {
        [Header("相关UI组件")]
        [Tooltip("拖拽需要配置的ui组件进来，需要和对应的InteractTipSO配置文件一致")]
        [SerializeField]
        public List<InteractUIComponentHandle> ui_components;

        public void HideUI(Action action = null)
        {
            action?.Invoke();
        }

        public void ShowUI(Action action = null)
        {
            action?.Invoke();
        }
        public bool TryGetComponent(E_InteractUIComponentType uiType, out MaskableGraphic uiGraphic)
        {
            for (int i = 0; i < ui_components.Count; i++)
            {
                if (ui_components[i].componentType == uiType)
                {
                    uiGraphic = ui_components[i].component;
                    return true;
                }
            }
            uiGraphic = null;
            return false;
            // uiGraphic = ui_components.Where(p=>p.componentType == uiType).Select(p=>p.component).FirstOrDefault();
            // return uiGraphic;
        }
    }
    [Serializable]
    public class InteractUIComponentHandle
    {
        public E_InteractUIComponentType componentType;
        [SerializeField]
        public MaskableGraphic component;
    }
    public enum E_InteractUIComponentType
    {
        image1,
        image2,
        image3,
        text1,
        text2,
        text3,
    }
}

using UnityEngine;

namespace InteractSystem
{
    [System.Serializable]
    public class UIComponentConfig
    {
        [Header("组件类型")]
        public E_InteractUIComponentType componentType; // 枚举选择组件类型

        [Header("资源配置（根据类型显示）")]
        [SerializeField] private Sprite imageValue; // 当类型为Image时生效
        [SerializeField] private string textValue;  // 当类型为Text时生效
                                                    // 扩展：其他类型的资源字段（如Button的点击事件名称）

        // 提供类型安全的取值方法
        public T GetValue<T>()
        {
            switch (componentType)
            {
                case E_InteractUIComponentType.image1:
                case E_InteractUIComponentType.image2:
                case E_InteractUIComponentType.image3:
                    if (typeof(T) == typeof(Sprite))
                        return (T)(object)imageValue;
                    break;
                case E_InteractUIComponentType.text1:
                case E_InteractUIComponentType.text2:
                case E_InteractUIComponentType.text3:
                    if (typeof(T) == typeof(string))
                        return (T)(object)textValue;
                    break;
            }
            throw new System.InvalidCastException($"组件类型{componentType}无法转换为{typeof(T)}");
        }
    }
    [System.Serializable]
    public class InteractPictureConfig
    {
        [Header("组件类型")]
        public E_InteractUIComponentType componentType; // 枚举选择组件类型

        [Header("资源配置")]
        [SerializeField] public Sprite value;
    }
    [System.Serializable]
    public class InteractTextConfig
    {
        [Header("组件类型")]
        public E_InteractUIComponentType componentType; // 枚举选择组件类型

        [Header("资源配置")]
        [SerializeField] public string value;
    }

}
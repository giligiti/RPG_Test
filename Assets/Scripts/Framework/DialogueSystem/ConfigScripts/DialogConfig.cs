using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DialogueSystem
{
    [CreateAssetMenu(fileName = "对话配置", menuName = "我的框架/对话系统/对话配置")]
    public class DialogConfig : SerializedScriptableObject
    {
        [ListDrawerSettings(ShowIndexLabels = true,ShowPaging = false)]
        public List<DialogStepConfig> stepConfigs = new List<DialogStepConfig>();
    }
}

using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DialogueSystem
{
    [System.Serializable]
    public class DialogStepConfig
    {
        public bool isPlayer;
        [HideLabel, Multiline(2)]
        public string content;
        public List<IDialogEvent> onStartEvent = new List<IDialogEvent>();
        public List<IDialogEvent> onEndEvent = new List<IDialogEvent>();
    }
}
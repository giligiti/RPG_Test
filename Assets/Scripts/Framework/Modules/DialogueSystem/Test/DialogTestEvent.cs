using UnityEngine;

namespace DialogueSystem
{
    public class DialogTestEvent : IDialogEvent
    {
        public string eventName = "DialogEvent";
        public void ConverString(string str)
        {
            eventName = str;
            Debug.Log("DialogTestEvent：收到事件字符串：" + str);
        }

        public void Execute()
        {
            Debug.Log("事件名字为："+ eventName);
        }
    }
}
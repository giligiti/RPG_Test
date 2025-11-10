using System.Collections;
using UnityEngine;

namespace DialogueSystem
{
    public interface IDialogEvent
    {
        public void Execute();
        public IEnumerator ExcuteBLocking() => null;
        public void ConverString(string str);
    }
}
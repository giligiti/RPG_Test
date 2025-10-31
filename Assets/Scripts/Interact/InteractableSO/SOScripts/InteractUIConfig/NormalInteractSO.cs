using UnityEngine;

namespace InteractSystem
{
    [CreateAssetMenu(fileName = "NormalInteractSO", menuName = "交互系统/交互UI配置/普通交互")]
    public class NormalInteractSO : InteractTipSO
    {
        public Sprite icon_BG;

        public override GameObject CreatObj()
        {
            return GameObject.Instantiate(interactUI);
        }

        public override GameObject InitObj(GameObject obj)
        {
            return obj;
        }
    }
}
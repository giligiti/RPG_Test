using UnityEngine;

namespace InteractSystem
{
    [CreateAssetMenu(fileName = "SpecialInteractSO", menuName = "交互系统/交互UI配置/特殊交互")]
    public class SpecialInteractSO : InteractTipSO
    {
        public Sprite icon_BG;
        public Sprite icon_Item;

        public override GameObject CreatObj()
        {
            throw new System.NotImplementedException();
        }

        public override GameObject InitObj(GameObject obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
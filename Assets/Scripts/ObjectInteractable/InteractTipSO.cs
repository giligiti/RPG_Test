using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "交互", menuName = "我的SO")]
public class InteractTipSO : ScriptableObject
{
    [Header("输入动作配置")]
    public string inputActionName; // 关联的输入动作名称（如"Interact_Special"）
    [SerializeField] private GameObject interactUI;
    [SerializeField] private Sprite icon_BG;
    [SerializeField] private Sprite icon_Item;
}

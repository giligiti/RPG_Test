using InteractSystem;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float redius;
    public GameObject uGUI;
    void Start()
    {
        var obj = Instantiate(uGUI);
        var panel = UIManager.Instance.ShowPanel<InteractPanel>();
        obj.transform.SetParent(panel.interactPoint.transform, false);
        obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        Destroy(obj);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(transform.position,redius);
    }
}
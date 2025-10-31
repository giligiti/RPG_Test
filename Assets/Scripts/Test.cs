using InteractSystem;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float redius;
    void Start()
    {
        // if (gameObject.GetComponentInParent<InteractableObject>())
        // {
        //     Debug.Log("物体身上存在InteractableObject脚本");
        // }
        // else
        // {
        //     Debug.Log("物体身上不存在InteractableObject脚本");
        // }

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position,redius);
    }
}
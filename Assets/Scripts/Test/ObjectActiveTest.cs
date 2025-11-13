using UnityEngine;

public class ObjectActiveTest : MonoBehaviour {
    private void OnDestroy() {
        Debug.Log(gameObject.name + "被销毁！！！");
    }
}
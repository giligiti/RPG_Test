using UnityEngine;

public class TurretPoolData : PoolData
{
    //炮塔的实例化方法
    protected override GameObject ObjectInstante(string key, string path = null)
    {
        key = path == null ? "Turret/" + key : path;
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(key));
        return obj;
    }

}

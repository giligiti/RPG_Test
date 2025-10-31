using UnityEngine;

public class BulletPoolData : PoolData
{
    protected override GameObject ObjectInstante(string key, string path = null)
    {
        key = path == null? "Bullet/" + key : path;
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(key));
        return obj;    
    }

    
}

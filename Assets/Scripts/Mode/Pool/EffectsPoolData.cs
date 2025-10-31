using UnityEngine;

public class EffectsPoolData : PoolData
{
    protected override GameObject ObjectInstante(string key, string path = null)
    {
        key = path == null ? "Eff/" + key : path;
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(key));
        return obj;
    }

}

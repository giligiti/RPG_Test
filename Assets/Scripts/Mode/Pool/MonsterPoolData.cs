using System.Collections.Generic;
using UnityEngine;

public class MonsterPoolData : PoolData
{
    protected override GameObject ObjectInstante(string key, string path = null)
    {
        key = path == null ? "Monster/Zombies/" + key : path;
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(key));
        return obj;
    }


}

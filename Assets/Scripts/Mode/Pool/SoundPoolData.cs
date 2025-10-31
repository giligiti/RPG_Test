

using UnityEngine;

public class SoundPoolData : PoolData
{
    protected override GameObject ObjectInstante(string key, string path = null)
    {
        GameObject obj = new GameObject(key);
        AudioSource ad = obj.AddComponent<AudioSource>();
        AutoRecycleSound autosound = obj.AddComponent<AutoRecycleSound>();
        autosound.GiveAudio(ad);//传入组件
        //需要在调用者处手动进行播放
        ad.playOnAwake = false;
        return obj;
    }
}
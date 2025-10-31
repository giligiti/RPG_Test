using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    //public 
    private SoundMgr soundMgr = SoundMgr.Instance;

    void Awake()
    {
        //注册到声音管理器中
        audioSource = gameObject.AddComponent<AudioSource>();
        soundMgr.RegisterAudioSource(audioSource);
        //播放器设置为3d播放
        audioSource.spatialBlend = 1;
        audioSource.loop = true;                //修改
    }

    public void PlaySound(E_SoundClip type)
    {
        soundMgr.PlaySound(audioSource, type);
        Debug.Log("正在播放");
    }
    public void StopSound()
    {
        audioSource.Stop();
        Debug.Log("停止播放");
    }


    void OnDestroy()
    {
        //在管理器中注销自己
        soundMgr.CleanAudioSource(audioSource);
    }
}
using System.Collections;
using UnityEngine;

public class AutoRecycleSound : MonoBehaviour
{
    public AudioSource ad;
    private const float offset = 0.1f;
    private const float soundOffset = 0;
    private 
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        
    }
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        
    }
    void Update()
    {
    }
    //依赖注入，对象池实例化的时候赋予Audiosource组件
    public void GiveAudio(AudioSource ad)
    {
        this.ad = ad;
        //设置为3D音效组件
        this.ad.spatialBlend = 1;
        //保证音频不循环
        this.ad.loop = false;
    }

    /// <summary>
    /// 扰动声音，使其有起伏
    /// </summary>
    /// 在管理器中加载初始化后调用
    public void DisturbanceVoice(float pitch = 0)
    {
        //让声音有所变化
        //因为每次重新加载都会同步，所以不需要回归状态
        float value = pitch + soundOffset;
        ad.pitch = Random.Range(ad.pitch - value, ad.pitch + value);
        ad.volume = Random.Range(ad.pitch - value, ad.pitch + value);
    }

    public void PlaySoundOnObj(float volume, bool mute, AudioClip clip)
    {
        ad.volume = volume;
        ad.mute = mute;
        ad.PlayOneShot(clip);
        StartCoroutine(WaitForSoundPlayOver());
    }

    IEnumerator WaitForSoundPlayOver()
    {
        yield return new WaitWhile(() => ad.isPlaying);
        yield return new WaitForSeconds(offset);
        //播放完成后启动自动回收到对象池中
        RecycleSound();
    }

    public void RecycleSound()
    {
        PoolMgr.Instance.PushObject<SoundPoolData>("SoundObj", this.gameObject);
    }
}
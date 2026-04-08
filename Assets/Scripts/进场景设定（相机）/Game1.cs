using UnityEngine;

public class Game1 : MonoBehaviour
{
    [Header("BGM设置")]
    public AudioClip bgmClip; // 指定要播放的BGM文件

    public ClearAllBullet clearAllBullet;// 清除所有子弹组件
    
    void OnEnable()
    {
        
        // 播放指定的BGM
        if (Global_AudioManager.Instance != null && bgmClip != null 
        && Global_AudioManager.Instance.GetCurrentBGMName() != "Game1")
        {
            Debug.Log("摄像头开始播放BGM");
            Global_AudioManager.Instance.PlaySFX(bgmClip,false,0.3f);
        }

        Invoke(nameof(FadeOutMusic), 118f);// 118秒后淡出BGM
    }

    void OnDisable()
    {
        clearAllBullet.ClearScreenBullet(false);
    }

    void FadeOutMusic()
    {
        Global_AudioManager.Instance.FadeOutMusic(2f);
    }
}

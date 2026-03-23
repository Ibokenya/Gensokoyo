using UnityEngine;

public class Game1 : MonoBehaviour
{
    [Header("BGM设置")]
    public AudioClip bgmClip; // 指定要播放的BGM文件
    
    void OnEnable()
    {
        Global_GameManager.Instance.state = State.Gaming;
        
        // 播放指定的BGM
        if (Global_AudioManager.Instance != null && bgmClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(bgmClip);
        }
    }
}

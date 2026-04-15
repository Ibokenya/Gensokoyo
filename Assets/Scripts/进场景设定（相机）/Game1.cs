using UnityEngine;

public class Game1 : MonoBehaviour
{
    [Header("BGM设置")]
    public AudioClip bgmClip; // 指定要播放的BGM文件

    public ClearAllBullet clearAllBullet;// 清除所有子弹组件

    [Header("时间查看器")]
    public float currentTime;
    [Header("进入对话相关")]
    public GameObject DialogBox;

    void Update()
    {
        // 更新当前音乐时间
        //currentTime = Global_AudioManager.Instance.CurrentBGMTime;
        currentTime += Time.deltaTime;
        if(currentTime >= 118f)
        {
            Debug.Log("摄像头淡出BGM");
            Global_AudioManager.Instance.FadeOutMusic(10f);
            DialogBox.SetActive(true);
        }
    }
    
    void OnEnable()
    {
        //播放指定的BGM
        if (Global_AudioManager.Instance != null && bgmClip != null 
        && Global_AudioManager.Instance.GetCurrentBGMName() != "Game1")
        {
            Debug.Log("摄像头开始播放BGM");
            Global_AudioManager.Instance.PlaySFX(bgmClip,false,0.8f);
        }
    }

    void OnDisable()
    {
        clearAllBullet.ClearScreenBullet(false);
    }
}

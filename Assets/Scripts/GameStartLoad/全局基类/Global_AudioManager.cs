using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 全局BGM播放给管理单例类
/// 游戏是一定要有音乐的，自然得有个地方来管理歌单
/// 之后还要对照音乐的节点设计敌人波次，想想都麻烦
/// 这里还可以设置音量（应该）
/// </summary>
public class Global_AudioManager : Singleton<Global_AudioManager>
{
    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效
        
        // 确保场景中只有一个AudioListener
        ManageAudioListener();
    }
    
    /// <summary>
    /// 管理AudioListener，确保场景中只有一个
    /// </summary>
    private void ManageAudioListener()
    {
        // 获取场景中所有的AudioListener
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        // 如果没有AudioListener，为当前对象添加一个
        if (listeners.Length == 0)
        {
            gameObject.AddComponent<AudioListener>();
        }
        else
        {
            // 保留第一个AudioListener，销毁其他的
            AudioListener mainListener = listeners[0];
            for (int i = 1; i < listeners.Length; i++)
            {
                Destroy(listeners[i]);
            }
            
            // 如果第一个AudioListener不在当前对象上，将其移动到当前对象
            if (mainListener.gameObject != gameObject)
            {
                mainListener.transform.SetParent(transform);
            }
        }
    }

    // 音频源组件
    private AudioSource bgmSource;
    
    // Start is called before the first frame update
    void Start()
    {
        // 添加AudioSource组件用于播放BGM
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="clip">音乐剪辑</param>
    /// <param name="volume">音量</param>
    public void PlayBGM(AudioClip clip, float volume = 1.0f)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.Play();
        }
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
    
    /// <summary>
    /// 调整背景音乐音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }
    }
}

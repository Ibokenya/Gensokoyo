using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 全局音频管理单例类
/// 负责管理游戏中的背景音乐和音效
/// </summary>
public class Global_AudioManager : Singleton<Global_AudioManager>
{
    #region 音频源组件

    private AudioSource bgmSource;      // 背景音乐音频源
    private List<AudioSource> sfxPool;  // 音效池
    private readonly int maxSFXPoolSize = 10;    // 音效池最大容量

    #endregion

    #region 背景音乐列表

    [Header("背景音乐列表")]
    public AudioClip menuBGM;      // 菜单音乐
    public AudioClip game1BGM;     // 游戏音乐1
    public AudioClip bossBGM;      // Boss音乐
    public AudioClip overBGM;      // 结束音乐
    public AudioClip endingBGM;    // 结局音乐

    private Dictionary<string, AudioClip> bgmDictionary;  // 背景音乐字典

    #endregion

    #region 音量设置

    private float bgmVolume = 0.70f;     // 背景音乐音量
    private float sfxVolume = 0.80f;     // 音效音量
    public float CurrentTime;

    #endregion

    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效
        
        // 确保场景中只有一个AudioListener
        ManageAudioListener();
        
        // 初始化音效池
        InitializeSFXPool();
        
        // 加载保存的音量设置
        LoadVolumeSettings();
        
        // 初始化背景音乐字典
        InitializeBGMDictionary();
    }

    void Update()
    {
        CurrentTime = bgmSource.time;
    }
    
    /// <summary>
    /// 初始化背景音乐字典
    /// </summary>
    private void InitializeBGMDictionary()
    {
        bgmDictionary = new Dictionary<string, AudioClip>
        {
            { "Menu", menuBGM },
            { "Game1", game1BGM },
            { "Boss", bossBGM },
            { "Over", overBGM },
            { "Ending", endingBGM }
        };
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
  
    #region 公共访问属性
    
    /// <summary>
    /// 当前播放的背景音乐
    /// </summary>
    public AudioClip CurrentBGM
    {
        get { return bgmSource != null ? bgmSource.clip : null; }
    }
    
    /// <summary>
    /// 当前背景音乐的播放位置（秒）
    /// </summary>
    public float CurrentBGMTime
    {
        get { return bgmSource != null ? bgmSource.time : 0f; }
        set { if (bgmSource != null) bgmSource.time = value; }
    }
    
    /// <summary>
    /// 背景音乐是否正在播放
    /// </summary>
    public bool IsBGMPlaying
    {
        get { return bgmSource != null && bgmSource.isPlaying; }
    }
    
    /// <summary>
    /// 背景音乐是否已暂停
    /// </summary>
    public bool IsBGMPaused
    {
        get { return bgmSource != null && !bgmSource.isPlaying && bgmSource.time > 0f; }
    }
    
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        // 添加AudioSource组件用于播放BGM
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
    }

    public float GetBGMVolume()
    {
        return bgmVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    #region 音效池管理
    
    /// <summary>
    /// 初始化音效池
    /// </summary>
    private void InitializeSFXPool()
    {
        sfxPool = new List<AudioSource>();
        
        // 预创建指定数量的AudioSource
        for (int i = 0; i < maxSFXPoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.volume = sfxVolume;
            sfxPool.Add(source);
        }
    }
    
    /// <summary>
    /// 从音效池中获取可用的AudioSource
    /// </summary>
    /// <returns>可用的AudioSource，如果没有则创建新的</returns>
    private AudioSource GetAvailableSFXSource()
    {
        // 查找未在播放的AudioSource
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // 如果没有可用的，创建新的
        if (sfxPool.Count < maxSFXPoolSize)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.loop = false;
            newSource.volume = sfxVolume;
            sfxPool.Add(newSource);
            return newSource;
        }
        
        // 如果达到最大容量，返回第一个（会覆盖正在播放的）
        return sfxPool[0];
    }
    
    #endregion
    
    #region 音效播放方法
    
    /// <summary>
    /// 音效播放
    /// 可以在播放背景音乐的同时播放音效
    /// </summary>
    /// <param name="clip">音效剪辑</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="volume">音量（0-1）</param>
    public void PlaySFX(AudioClip clip, bool isLoop = false, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("音效剪辑为空，无法播放");
            return;
        }
        
        // 从音效池获取可用的AudioSource
        AudioSource source = GetAvailableSFXSource();
        if (source != null)
        {
            source.clip = clip;
            source.loop = isLoop;
            source.volume = Mathf.Clamp01(volume) * sfxVolume;
            source.Play();
        }
    }
    
    #endregion
    
    #region 背景音乐方法
    
    /// <summary>
    /// 播放背景音乐（通过名称）
    /// 同一时间内只能播放一首背景音乐，切换时必须停止正在进行的
    /// </summary>
    /// <param name="bgmName">背景音乐名称（Menu, Game1, Boss, Over, Ending）</param>
    /// <param name="volume">音量（0-1）</param>
    public void PlayBGM(string bgmName, float volume = 1.0f)
    {
        if (bgmSource == null)
        {
            Debug.LogWarning("背景音乐音频源未初始化");
            return;
        }
        
        // 从字典中获取对应的音乐剪辑
        if (bgmDictionary.TryGetValue(bgmName, out AudioClip clip))
        {
            if (clip == null)
            {
                Debug.LogWarning($"背景音乐 '{bgmName}' 未分配音频剪辑");
                return;
            }
            
            // 如果正在播放同一首音乐，则不重复播放
            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return;
            }
            
            // 停止当前背景音乐
            bgmSource.Stop();
            
            // 重置播放时间
            bgmSource.time = 0f;
            
            // 播放新的背景音乐
            bgmSource.clip = clip;
            bgmSource.volume = Mathf.Clamp01(volume) * bgmVolume;
            bgmSource.Play();
            
            Debug.Log($"播放背景音乐: {bgmName}");
        }
        else
        {
            Debug.LogWarning($"未找到名为 '{bgmName}' 的背景音乐");
        }
    }
    
    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }
    
    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying && bgmSource.time > 0f)
        {
            bgmSource.UnPause();
        }
    }
    
    /// <summary>
    /// 停止当前播放的背景音乐
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
    
    #endregion
    
    #region 音量控制方法
    
    /// <summary>
    /// 调整背景音乐音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
        
        // 保存音量设置
        SaveVolumeSettings();
    }
    
    /// <summary>
    /// 调整音效音量
    /// </summary>
    /// <param name="volume">音量值（0-1）</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        
        // 更新音效池中的所有AudioSource音量
        foreach (AudioSource source in sfxPool)
        {
            if (source != null)
            {
                source.volume = sfxVolume;
            }
        }
        
        // 保存音量设置
        SaveVolumeSettings();
    }
    
    /// <summary>
    /// 保存音量设置到PlayerPrefs
    /// </summary>
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 从PlayerPrefs加载音量设置
    /// </summary>
    private void LoadVolumeSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f); // 默认值0.7
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f); // 默认值0.8
    }
    
    #endregion
    
    #region 额外功能
    
    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSFX()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (source != null)
            {
                source.Stop();
            }
        }
    }
    
    /// <summary>
    /// 停止指定的循环音效
    /// </summary>
    /// <param name="clip">要停止的音效剪辑</param>
    public void StopLoopSFX(AudioClip clip)
    {
        foreach (AudioSource source in sfxPool)
        {
            if (source != null && source.clip == clip && source.loop)
            {
                source.Stop();
            }
        }
    }
    
    /// <summary>
    /// 获取当前播放的背景音乐名称
    /// </summary>
    /// <returns>当前背景音乐名称，如果没有播放则返回空字符串</returns>
    public string GetCurrentBGMName()
    {
        if (bgmSource == null || bgmSource.clip == null)
        {
            return "";
        }
        
        // 从字典中查找当前剪辑对应的名称
        foreach (var kvp in bgmDictionary)
        {
            if (kvp.Value == bgmSource.clip)
            {
                return kvp.Key;
            }
        }
        
        return "";
    }
    
    /// <summary>
    /// 获取当前背景音乐的播放位置
    /// </summary>
    /// <returns>当前播放位置（秒）</returns>
    public float GetCurrentBGMPosition()
    {
        return bgmSource != null ? bgmSource.time : 0f;
    }
    
    /// <summary>
    /// 设置背景音乐的播放位置
    /// </summary>
    /// <param name="position">播放位置（秒）</param>
    public void SetBGMPosition(float position)
    {
        if (bgmSource != null)
        {
            bgmSource.time = position;
        }
    }

    /// <summary>
    /// 音乐淡出
    /// 在指定时间内将当前播放的背景音乐音量均匀降低到0
    /// </summary>
    /// <param name="fadeOutTime">淡出时间（秒）</param>
    public void FadeOutMusic(float fadeOutTime)
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            StartCoroutine(FadeOutMusicCoroutine(fadeOutTime));
        }
    }
    
    /// <summary>
    /// 音乐淡出协程
    /// </summary>
    /// <param name="fadeOutTime">淡出时间（秒）</param>
    private IEnumerator FadeOutMusicCoroutine(float fadeOutTime)
    {
        if (fadeOutTime <= 0f)
        {
            // 如果淡出时间小于等于0，直接停止音乐
            bgmSource.Stop();
            yield break;
        }
        
        float startVolume = bgmSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeOutTime);
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        
        // 淡出完成后停止音乐
        bgmSource.Stop();
        // 重置音量，以便下次播放
        bgmSource.volume = startVolume;
    }
    
    #endregion
}



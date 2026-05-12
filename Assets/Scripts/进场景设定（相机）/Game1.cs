using UnityEngine;

public class Game1 : MonoBehaviour
{
    [Header("BGM设置")]
    public AudioClip bgmClip1; // 指定要播放的BGM文件
    public AudioClip bgmClip2; // 指定要播放的BGM文件2

    public ClearAllBullet clearAllBullet;// 清除所有子弹组件

    [Header("时间查看器")]
    public float currentTime;
    [Header("进入对话相关")]
    public GameObject DialogBox;
    

    [Header("镜头抖动设置")]
    [Range(0f, 1f)] public float defaultShakeIntensity = 0.5f; // 默认震动强度
    [Range(5f, 20f)] public float defaultShakeFrequency = 12f; // 默认震动频率
    [Range(0.5f, 5f)] public float defaultDecaySpeed = 2f; // 默认衰减速度
    private Vector3 originalPos; // 原始位置
    private float shakeIntensity = 0f; // 当前震动强度
    private float shakeFrequency = 10f; // 震动频率
    private float decaySpeed = 2f; // 衰减速度
    private float shakeDuration = 0f; // 抖动剩余时长

    private bool isfirst = true;

    void Awake()
    {
        // 保存摄像机原始位置
        originalPos = transform.localPosition;
    }
    
    void Update()
    {
        // 更新当前音乐时间
        currentTime = Global_AudioManager.Instance.CurrentBGMTime;
        if(currentTime >= 118f && currentTime <= 119f && isfirst)
        {
            // 激活对话框，淡出背景音乐由AboutDialog处理
            DialogBox.SetActive(true);
            isfirst = false;
        }
        
        // 处理镜头抖动
        HandleCameraShake();
    }

    void OnDisable()
    {
        clearAllBullet.ClearScreenBullet(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 处理镜头抖动
    /// </summary>
    private void HandleCameraShake()
    {
        if (shakeIntensity > 0 && shakeDuration > 0)
        {
            // 使用 Perlin 噪声做平滑震动（不抽搐核心）
            float x = Mathf.PerlinNoise(Time.time * shakeFrequency, 10f) * 2f - 1f;
            float y = Mathf.PerlinNoise(10f, Time.time * shakeFrequency) * 2f - 1f;
            
            // 将偏移限制在 (-1, -1, 0) 到 (1, 1, 0) 之间
            x = Mathf.Clamp(x, -1f, 1f);
            y = Mathf.Clamp(y, -1f, 1f);
            
            // 设置抖动后的位置
            transform.localPosition = originalPos + new Vector3(x, y, 0) * shakeIntensity;
            
            // 强度逐渐衰减
            shakeIntensity -= decaySpeed * Time.unscaledDeltaTime;
            
            // 更新剩余时长
            shakeDuration -= Time.unscaledDeltaTime;
        }
        else
        {
            // 震动结束，回到原位
            shakeIntensity = 0;
            shakeDuration = 0;
            transform.localPosition = originalPos;
        }
    }
    
    /// <summary>
    /// 触发镜头抖动
    /// </summary>
    /// <param name="duration">抖动时长（秒）</param>
    /// <param name="strength">震动强度（默认0.5）</param>
    /// <param name="frequency">震动频率（默认12）</param>
    /// <param name="decay">衰减速度（默认2）</param>
    public void Shake(float duration = 0.5f, float strength = -1f, float frequency = -1f, float decay = -1f)
    {
        shakeDuration = duration;
        shakeIntensity = strength > 0 ? strength : defaultShakeIntensity;
        shakeFrequency = frequency > 0 ? frequency : defaultShakeFrequency;
        decaySpeed = decay > 0 ? decay : defaultDecaySpeed;
    }
}

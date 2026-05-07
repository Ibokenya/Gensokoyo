using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Security;

public class BossUI : MonoBehaviour
{
    [Header("UI元素")]
    public List<GameObject> HPs;// 几个阴阳玉血条
    public GameObject TimeText;// 时间文本
    private TextMeshProUGUI timeTextComponent;
    public TextMeshProUGUI FinalWarningText;
    
    [Header("时间设置")]
    private float currentTime = 0f; // 当前剩余时间
    public AudioClip timeoutSound;//超时音效
    private int lastPlayedSecond = -1; // 上一次播放音效的秒数

    [Header("阴阳玉阶段图标")]
    public List<Sprite> HpIcons;
    
    [Header("血量状态")]
    private int currentHpIndex = 0; // 当前血量指示物索引（从末尾开始计数，0表示第一个指示物的第一格血）
    
    [Header("阶段信息")]
    public List<GameObject> phaseIndicators; // 阶段指示器

    void OnEnable()
    {
        if (TimeText != null)
        {
            timeTextComponent = TimeText.GetComponent<TextMeshProUGUI>();
        }
        // 重置所有HPs的sprite为HpIcons[2]（满血心形）
        ResetAllHPs();
        ShowUI();
    }

    /// <summary>
    /// 更新时间
    /// </summary>
    void Update()
    {
        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                TimeOver();
            }
            UpdateTimeText(currentTime);
            
            // 播放倒计时音效（仅在剩余5,4,3,2,1秒时播放）
            PlayCountdownSound();
        }
    }
    
    /// <summary>
    /// 重置所有HPs的sprite为满血状态
    /// </summary>
    private void ResetAllHPs()
    {
        currentHpIndex = 0;
        foreach (GameObject hp in HPs)
        {
            if (hp != null && HpIcons.Count > 2)
            {
                Image image = hp.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = HpIcons[2];
                }
            }
        }
    }
    
    /// <summary>
    /// 扣除一格血
    /// 正常情况下共2个血量指示物
    /// 第一次扣血：将索引末尾的指示物的sprite设为HpIcons[1]（半个心形）
    /// 第二次扣血：将该物体sprite设为HpIcons[0]（空心心形）
    /// 再扣血：按照HPs索引末尾往前递推
    /// </summary>
    public void SubLife()
    {
        // 计算当前应该修改的指示物索引（从末尾开始）
        int indicatorIndex = HPs.Count - 1 - (currentHpIndex / 2);
        int subIndex = currentHpIndex % 2; // 0表示第一次扣血，1表示第二次扣血
        
        if (indicatorIndex >= 0 && indicatorIndex < HPs.Count)
        {
            GameObject hpObject = HPs[indicatorIndex];
            if (hpObject != null)
            {
                Image image = hpObject.GetComponent<Image>();
                if (image != null && HpIcons.Count > 1)
                {
                    if (subIndex == 0)
                    {
                        // 第一次扣血，设为半个心形
                        image.sprite = HpIcons[1];
                    }
                    else
                    {
                        // 第二次扣血，设为空心心形
                        image.sprite = HpIcons[0];
                    }
                }
            }
        }
        
        currentHpIndex++;
    }
    /// <summary>
    /// 显示UI，1秒内淡入
    /// </summary>
    public void ShowUI()
    {
        StartCoroutine(FadeInUI());
    }
    
    /// <summary>
    /// UI淡入协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInUI()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        
        // 初始化透明度为0
        foreach (GameObject hp in HPs)
        {
            if (hp != null)
            {
                Image image = hp.GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    color.a = 0f;
                    image.color = color;
                }
            }
        }
        
        if (timeTextComponent != null)
        {
            Color color = timeTextComponent.color;
            color.a = 0f;
            timeTextComponent.color = color;
        }
        
        // 淡入效果
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float alpha = Mathf.Lerp(0f, 1f, t);
            
            foreach (GameObject hp in HPs)
            {
                if (hp != null)
                {
                    Image image = hp.GetComponent<Image>();
                    if (image != null)
                    {
                        Color color = image.color;
                        color.a = alpha;
                        image.color = color;
                    }
                }
            }
            
            if (timeTextComponent != null)
            {
                Color color = timeTextComponent.color;
                color.a = alpha;
                timeTextComponent.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终透明度为1
        foreach (GameObject hp in HPs)
        {
            if (hp != null)
            {
                Image image = hp.GetComponent<Image>();
                if (image != null)
                {
                    Color color = image.color;
                    color.a = 1f;
                    image.color = color;
                }
            }
        }
        
        if (timeTextComponent != null)
        {
            Color color = timeTextComponent.color;
            color.a = 1f;
            timeTextComponent.color = color;
        }
    }
    
    /// <summary>
    /// 播放倒计时音效
    /// 当剩余时间为5秒时开始，每一秒播放一次（仅在正整数秒5,4,3,2,1播放）
    /// </summary>
    private void PlayCountdownSound()
    {
        int currentSecond = Mathf.FloorToInt(currentTime);
        
        // 只在剩余时间5,4,3,2,1秒时播放，且每秒只播放一次
        if (currentSecond >= 1 && currentSecond <= 5 && currentSecond != lastPlayedSecond)
        {
            if (timeoutSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(timeoutSound);
            }
            lastPlayedSecond = currentSecond;
        }
    }
    
    /// <summary>
    /// 设置符卡时间
    /// </summary>
    /// <param name="time">时间（秒）</param>
    public void SetCardTime(float time)
    {
        StartCoroutine(SmoothTimeTransition(time));
    }
    
    /// <summary>
    /// 平滑时间过渡协程
    /// </summary>
    /// <param name="targetTime">目标时间（秒）</param>
    /// <returns></returns>
    private IEnumerator SmoothTimeTransition(float targetTime)
    {
        float duration = 1f; // 过渡时间为1秒
        float startTime = currentTime;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            // 使用线性插值从当前时间过渡到目标时间
            currentTime = Mathf.Lerp(startTime, targetTime, t);
            // 更新时间文本
            UpdateTimeText(currentTime);
            yield return null;
        }
        
        // 确保最终时间为目标时间
        currentTime = targetTime;
        UpdateTimeText(currentTime);
    }
    
    /// <summary>
    /// 时间结束处理
    /// </summary>
    public void TimeOver()
    {
        // 时间归零的处理逻辑
    }
    
    /// <summary>
    /// 更新时间文本
    /// </summary>
    /// <param name="timeLeft">剩余时间（秒）</param>
    public void UpdateTimeText(float timeLeft)
    {
        if (timeTextComponent != null)
        {
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            float milliseconds = (timeLeft % 1f) * 100f;
            timeTextComponent.text = string.Format("{0:0}.{1:00}", seconds, Mathf.FloorToInt(milliseconds));
        }
    }

    public void ShowFinalWarning()
    {
        if(FinalWarningText != null)
        {
            Color color = FinalWarningText.color;
            color.a = 1f;
            FinalWarningText.color = color;
            Invoke("HideFinalWarning", 2f);
        }
    }

    private void HideFinalWarning()
    {
        if(FinalWarningText != null)
        {
            Color color = FinalWarningText.color;
            color.a = 0f;
            FinalWarningText.color = color;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    [Header("UI元素")]
    public List<GameObject> HPs;// 几个阴阳玉血条
    public GameObject TimeText;// 时间文本
    private TextMeshProUGUI timeTextComponent;

    [Header("阴阳玉阶段图标")]
    public List<Sprite> HpIcons;
    
    [Header("阶段信息")]
    public List<GameObject> phaseIndicators; // 阶段指示器

    void OnEnable()
    {
        if (TimeText != null)
        {
            timeTextComponent = TimeText.GetComponent<TextMeshProUGUI>();
        }
        ShowUI();
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
    /// 更新时间文本
    /// </summary>
    /// <param name="timeLeft">剩余时间（秒）</param>
    public void UpdateTimeText(float timeLeft)
    {
        if (timeTextComponent != null)
        {
            timeTextComponent.text = timeLeft.ToString("F2");
        }
    }
    
    /// <summary>
    /// 更新阶段指示器
    /// </summary>
    /// <param name="currentPhaseIndex">当前阶段索引</param>
    public void UpdatePhaseIndicators(int currentPhaseIndex)
    {
        // 禁用所有阶段指示器
        foreach (GameObject indicator in phaseIndicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }
        
        // 启用当前阶段及之前的指示器
        for (int i = 0; i <= currentPhaseIndex && i < phaseIndicators.Count; i++)
        {
            if (phaseIndicators[i] != null)
            {
                phaseIndicators[i].SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// 显示Boss剩余攻击阶段
    /// </summary>
    /// <param name="totalPhases">总阶段数</param>
    /// <param name="currentPhase">当前阶段</param>
    public void ShowRemainingPhases(int totalPhases, int currentPhase)
    {
        int remainingPhases = totalPhases - currentPhase;
        Debug.Log("Boss剩余攻击阶段: " + remainingPhases);
        // 这里可以添加具体的UI显示逻辑
    }
}

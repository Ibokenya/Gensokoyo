using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BossUI : MonoBehaviour
{
    [Header("UI元素")]
    public List<GameObject> HPs;// 几个阴阳玉血条
    public GameObject TimeText;// 时间文本
    private TextMeshProUGUI timeTextComponent;
    
    [Header("阶段信息")]
    public List<GameObject> phaseIndicators; // 阶段指示器
    
    private void Start()
    {
        if (TimeText != null)
        {
            timeTextComponent = TimeText.GetComponent<TextMeshProUGUI>();
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

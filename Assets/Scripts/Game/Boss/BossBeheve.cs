using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBeheve : MonoBehaviour
{
    [Header("阶段配置")]
    public float none1Duration = 30f; // 第一阶段普通攻击持续时间
    public float card1Duration = 45f; // 第一阶段符卡持续时间
    public float none2Duration = 35f; // 第二阶段普通攻击持续时间
    public float card2Duration = 50f; // 第二阶段符卡持续时间
    public float finalCardDuration = 60f; // 最终符卡持续时间
    
    [Header("阶段脚本")]
    public MonoBehaviour none1Script; // 第一阶段普通攻击脚本
    public MonoBehaviour card1Script; // 第一阶段符卡脚本
    public MonoBehaviour none2Script; // 第二阶段普通攻击脚本
    public MonoBehaviour card2Script; // 第二阶段符卡脚本
    public MonoBehaviour finalCardScript; // 最终符卡脚本
    
    [Header("引用")]
    public BossUI bossUI;
    public BossAnime bossAnime;
    
    private float phaseTimer = 0f;
    private int currentPhase = 0;
    private int totalPhases = 5;
    
    // 阶段持续时间数组
    private float[] phaseDurations;
    // 阶段脚本数组
    private MonoBehaviour[] phaseScripts;
    
    private void Start()
    {
        // 初始化阶段配置
        phaseDurations = new float[] { none1Duration, card1Duration, none2Duration, card2Duration, finalCardDuration };
        phaseScripts = new MonoBehaviour[] { none1Script, card1Script, none2Script, card2Script, finalCardScript };
        
        // 禁用所有阶段脚本
        foreach (MonoBehaviour script in phaseScripts)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }
        
        // 开始第一阶段
        StartPhase(0);
    }
    
    private void Update()
    {
        // 更新阶段计时器
        phaseTimer += Time.deltaTime;
        
        // 检查是否需要切换阶段
        if (phaseTimer >= phaseDurations[currentPhase])
        {
            // 结束当前阶段
            EndPhase(currentPhase);
            
            // 进入下一阶段
            currentPhase++;
            if (currentPhase < totalPhases)
            {
                StartPhase(currentPhase);
            }
            else
            {
                // 所有阶段结束
                Debug.Log("Boss战所有阶段结束");
            }
        }
        
        // 更新UI时间文本
        if (bossUI != null)
        {
            float timeLeft = phaseDurations[currentPhase] - phaseTimer;
            bossUI.UpdateTimeText(timeLeft);
        }
    }
    
    /// <summary>
    /// 开始新阶段
    /// </summary>
    /// <param name="phaseIndex">阶段索引</param>
    private void StartPhase(int phaseIndex)
    {
        // 重置阶段计时器
        phaseTimer = 0f;
        
        // 启用当前阶段脚本
        if (phaseIndex < phaseScripts.Length && phaseScripts[phaseIndex] != null)
        {
            phaseScripts[phaseIndex].enabled = true;
            Debug.Log("开始阶段 " + (phaseIndex + 1));
        }
        
        // 更新UI
        if (bossUI != null)
        {
            bossUI.UpdatePhaseIndicators(phaseIndex);
            bossUI.ShowRemainingPhases(totalPhases, phaseIndex);
        }
    }
    
    /// <summary>
    /// 结束当前阶段
    /// </summary>
    /// <param name="phaseIndex">阶段索引</param>
    private void EndPhase(int phaseIndex)
    {
        // 禁用当前阶段脚本
        if (phaseIndex < phaseScripts.Length && phaseScripts[phaseIndex] != null)
        {
            phaseScripts[phaseIndex].enabled = false;
            Debug.Log("结束阶段 " + (phaseIndex + 1));
        }
    }
    
    /// <summary>
    /// 触发Boss移动
    /// </summary>
    /// <param name="direction">移动方向</param>
    public void MoveBoss(BossAnimeType direction)
    {
        if (bossAnime != null)
        {
            bossAnime.SetState(direction);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalCard : MonoBehaviour
{
    public GameObject IcePearl;//冰珠
    public GameObject IcePick;//冰刺
    public GameObject FrozenIce;//冰球
    public GameObject IceSpike;//冰锥
    public GameObject IceFlake;//冰花
    public GameObject IceRealm;//冰领域
    
    [Header("FinalCard阶段参数")]
    public int currentPhase = 1; // 当前阶段（1-4）
    public float phaseTimer = 0f; // 阶段计时器
    public float phase1Duration = 12f; // 第一阶段持续时间
    public float phase2Duration = 12f; // 第二阶段持续时间
    public float phase3Duration = 12f; // 第三阶段持续时间
    public float phase4Duration = 12f; // 第四阶段持续时间
    
    public BossShootSystem bossShootSystem;
    private int previousPhase = 1;// 上一阶段
    private Coroutine icePearlCoroutine;
    private Coroutine icePickCoroutine;
    
    private void OnEnable()
    {
        // 重置阶段参数
        currentPhase = 1;
        previousPhase = 1;
        phaseTimer = 0f;
        
        // 重置协程引用
        icePearlCoroutine = null;
        icePickCoroutine = null;
        
        // 初始化冰锥对象池（20个）
        if (IceSpike != null)
        {
            Global_ObjectPool.Instance.InitPool(IceSpike, 20);
        }
        
        // 开始射击
        StartShooting();
    }
    
    private void Update()
    {
        // 更新计时器
        phaseTimer += Time.deltaTime;
        
        // 阶段切换逻辑
        UpdatePhase();
    }
    
    /// <summary>
    /// 更新阶段
    /// </summary>
    private void UpdatePhase()
    {
        // 检查阶段是否发生变化
        if (currentPhase != previousPhase)
        {
            // 阶段发生变化，启动新的射击
            StartShooting();
            previousPhase = currentPhase;
        }
        
        switch (currentPhase)
        {
            case 1:
                // 第一阶段：同时发射冰珠和冰刺，12秒后进入第二阶段
                if (phaseTimer >= phase1Duration)
                {
                    currentPhase = 2;
                    phaseTimer = 0f;
                    Debug.Log("FinalCard进入第二阶段");
                }
                break;
            case 2:
                // 第二阶段：仅发射冰刺
                // 可以在这里添加第二阶段持续时间判断等
                break;
            case 3:
                // 第三阶段
                break;
            case 4:
                // 第四阶段
                break;
        }
    }
    
    /// <summary>
    /// 开始射击
    /// </summary>
    private void StartShooting()
    {
        if (bossShootSystem != null)
        {
            // 第一阶段：同时发射冰珠和冰刺，方向相反
            if (currentPhase == 1 && IcePearl != null && IcePick != null)
            {
                // 冰珠从左侧开始（从左向右扫）
                icePearlCoroutine = bossShootSystem.RepeatShoot(IcePearl, 60f, 12f, 0.1f, true);
                // 冰刺从右侧开始（从右向左扫）
                icePickCoroutine = bossShootSystem.RepeatShoot(IcePick, 60f, 10f, 0.5f, false);
            }
            // 第二阶段：仅发射冰刺
            else if (currentPhase == 2 && IcePick != null)
            {
                // 停止冰珠射击协程
                if (icePearlCoroutine != null)
                {
                    bossShootSystem.StopCoroutine(icePearlCoroutine);
                    icePearlCoroutine = null;
                }
                // 冰刺从右侧开始（从右向左扫）
                icePickCoroutine = bossShootSystem.RepeatShoot(IcePick, 60f, 10f, 0.4f, false);
            }
        }
    }
}

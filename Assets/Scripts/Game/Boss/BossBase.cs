using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HpConfig
{
    [Header("每波血量配置")]
    public int none1HP; // 第一波普通攻击血量
    public int card1HP; // 第一波符卡血量
    public int none2HP; // 第二波普通攻击血量
    public int card2HP; // 第二波符卡血量
}

public class BossBase : MonoBehaviour
{
    private int HP;
    private int MaxHP;
    private float PowerDefense = 1f; // 灵力相关受伤系数
    private float phaseDamageMultiplier = 1f; // 阶段补正受伤系数
    public bool isLockingHP = false; // 是否处于锁血状态
    public bool isNoDead = false; // 是否无敌
    private float lockHPThreshold = 0.01f; // 锁血阈值（1%）
    private int currentPhaseDamage = 0; // 当前符卡受到的总伤害
    private int currentPhaseIndex = 0; // 当前阶段索引（用于获取对应阶段血量配置）
    private bool hasApplied30PercentCorrection = false; // 是否已应用30%时间点补正
    private bool hasApplied70PercentCorrection = false; // 是否已应用70%时间点补正
    private int hpAtPhaseStart = 0; // 阶段开始时的血量
    private float phaseTotalTime = 0f; // 当前阶段总时长
    private float phaseElapsedTime = 0f; // 当前阶段已进行时间
    
    [Header("血量配置")]
    public HpConfig hpConfig;
    
    [Header("引用")]
    public GameObject DefenseRealm; // 防御屏障
    public BossAnime bossAnime;
    public UIManager uiManager;
    public AudioClip Bonus;
    
    private void OnEnable()
    {
        // 订阅灵力改变事件
        if (Global_GameManager.Instance != null)
        {
            Global_GameManager.Instance.OnPowerChanged += OnPowerChangedHandler;
        }
        isLockingHP = false;
    }
    
    private void OnDisable()
    {
        // 取消订阅灵力改变事件
        if (Global_GameManager.Instance != null)
        {
            Global_GameManager.Instance.OnPowerChanged -= OnPowerChangedHandler;
        }
        isLockingHP = false;
        isNoDead = false;
    }
    
    private void Start()
    {
        // 初始化受伤系数
        UpdateDefense();
        // 初始化血条
        UpdateHPBar();
    }
    
    private void Update()
    {
        // 更新阶段已进行时间
        if (phaseTotalTime > 0f && !isLockingHP)
        {
            phaseElapsedTime += Time.deltaTime;
            
            // 检查30%时间点补正
            float timePercent = phaseElapsedTime / phaseTotalTime;
            if (timePercent >= 0.3f && !hasApplied30PercentCorrection)
            {
                ApplyDamageCorrection(0.3f);
                hasApplied30PercentCorrection = true;
            }
            
            // 检查70%时间点补正
            if (timePercent >= 0.7f && !hasApplied70PercentCorrection)
            {
                ApplyDamageCorrection(0.7f);
                hasApplied70PercentCorrection = true;
            }
        }
    }
    
    /// <summary>
    /// 灵力改变事件处理器
    /// </summary>
    /// <param name="power">当前灵力值</param>
    private void OnPowerChangedHandler(int power)
    {
        UpdateDefense();
    }
    
    /// <summary>
    /// 根据当前角色和灵力值更新受伤系数
    /// </summary>
    private void UpdateDefense()
    {
        if (Global_GameManager.Instance == null)
        {
            PowerDefense = 1f;
            return;
        }
        
        Character character = Global_GameManager.Instance.character;
        int power = Global_GameManager.Instance.Power;
        
        // 获取灵力值的百位数（100-400对应1-4）
        int powerTier = Mathf.Clamp(Mathf.CeilToInt(power / 100f), 1, 4);
        
        // 根据角色和灵力等级设置受伤系数
        switch (character)
        {
            case Character.Reimu:
                // 灵梦：1, 0.94, 0.88, 0.82
                switch (powerTier)
                {
                    case 1:
                        PowerDefense = 1f;
                        break;
                    case 2:
                        PowerDefense = 0.94f;
                        break;
                    case 3:
                        PowerDefense = 0.88f;
                        break;
                    case 4:
                        PowerDefense = 0.82f;
                        break;
                }
                break;
                
            case Character.Marisa:
                // 魔理沙：1, 0.9, 0.8, 0.7
                switch (powerTier)
                {
                    case 1:
                        PowerDefense = 1f;
                        break;
                    case 2:
                        PowerDefense = 0.9f;
                        break;
                    case 3:
                        PowerDefense = 0.8f;
                        break;
                    case 4:
                        PowerDefense = 0.7f;
                        break;
                }
                break;
                
            default:
                PowerDefense = 1f;
                break;
        }
    }
    
    /// <summary>
    /// 处理Boss受伤
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        // 如果无敌，直接返回
        if (isNoDead)
        {
            return;
        }

        // 如果处于锁血状态，将伤害转化为奖励
        if (isLockingHP)
        {
            ConvertDamageToReward(damage);
            return;
        }
        
        // 统计当前符卡受到的伤害
        currentPhaseDamage += damage;
        
        // 应用两个受伤系数：灵力系数 × 阶段补正系数
        float totalMultiplier = PowerDefense * phaseDamageMultiplier;
        int finalDamage = (int)(damage * totalMultiplier);
        // 确保最小伤害为1
        finalDamage = Mathf.Max(1, finalDamage);
        HP = Mathf.Max(0, HP - finalDamage);
        
        // 更新血条
        UpdateHPBar();
        
        // 检查是否达到锁血阈值
        CheckLockHPThreshold();
    }
    
    /// <summary>
    /// 检查时间进度并应用伤害补正
    /// 由BossBeheve调用，传入当前阶段已进行时间和总时间
    /// </summary>
    /// <param name="currentTime">当前阶段已进行时间</param>
    /// <param name="totalTime">当前阶段总时间</param>
    public void CheckTimeCorrection(float currentTime, float totalTime)
    {
        if (totalTime <= 0) return;
        
        float timePercent = currentTime / totalTime;
        
        // 30%时间点补正
        if (timePercent >= 0.3f && !hasApplied30PercentCorrection)
        {
            ApplyDamageCorrection(0.3f);
            hasApplied30PercentCorrection = true;
        }
        
        // 70%时间点补正
        if (timePercent >= 0.7f && !hasApplied70PercentCorrection)
        {
            ApplyDamageCorrection(0.7f);
            hasApplied70PercentCorrection = true;
        }
    }
    
    /// <summary>
    /// 应用伤害补正
    /// </summary>
    /// <param name="timePercent">当前时间百分比（0.3或0.7）</param>
    private void ApplyDamageCorrection(float timePercent)
    {
        // 计算实际失去的血量
        int actualLostHP = hpAtPhaseStart - HP;
        // 计算期望失去的血量（阶段总血量 × 时间百分比）
        int expectedLostHP = Mathf.RoundToInt(MaxHP * timePercent);
        
        // 计算补正系数
        float correctionFactor = 1f;
        if (actualLostHP > 0)
        {
            correctionFactor = (float)expectedLostHP / actualLostHP;
            // 增伤系数不会小于1，减伤系数正常计算
            if (correctionFactor > 1)
            {
                // 增伤：实际受伤少于预期，需要增加伤害
                correctionFactor = Mathf.Max(1f, correctionFactor);
            }
        }
        
        // 应用80%的补正效果
        float finalCorrection = 1f + (correctionFactor - 1f) * 0.8f;
        
        // 更新阶段补正受伤系数，并限制范围：减伤0.8~1，增伤1~1.2
        phaseDamageMultiplier = Mathf.Clamp(finalCorrection, 0.8f, 1.2f);
        
    }
    
    /// <summary>
    /// 输出当前符卡受到的伤害统计并重置
    /// 在每张符卡（none或card脚本）结束时调用
    /// </summary>
    /// <param name="phaseName">当前符卡名称</param>
    public void LogPhaseDamage(string phaseName)
    {
        Debug.Log($"符卡 [{phaseName}] 期间受到的总伤害: {currentPhaseDamage}");
        // 重置伤害统计
        currentPhaseDamage = 0;
    }
    
    /// <summary>
    /// 检查是否达到锁血阈值
    /// </summary>
    private void CheckLockHPThreshold()
    {
        // 当血量下降到1%时，触发锁血
        if (!isLockingHP && (float)HP / MaxHP <= lockHPThreshold)
        {
            LockHP();
        }
    }
    
    /// <summary>
    /// 锁血方法
    /// 当血量下降到1%时触发，停止射击并显示锁血UI和动画
    /// </summary>
    public void LockHP()
    {
        // 播放锁血音效
        if (Bonus != null)
        {
            Global_AudioManager.Instance.PlaySFX(Bonus);
        }
        isLockingHP = true;
        Debug.Log("Boss进入锁血状态");
    }
    
    /// <summary>
    /// 将伤害转化为奖励
    /// 锁血状态下，boss被击中时调用此方法
    /// </summary>
    /// <param name="damage">子弹伤害</param>
    private void ConvertDamageToReward(int damage)
    {
        uiManager.AddExScore(damage);
        CreateItem.Instance.SpawnPowerItems(transform.position);
    }
    
    /// <summary>
    /// 玩家释放技能攻击的通知
    /// boss可以通过特殊动画来规避掉玩家的技能伤害
    /// </summary>
    /// <param name="skillType">技能类型（1:灵梦常规, 2:灵梦决死, 3:魔理沙常规, 4:魔理沙决死）</param>
    public void OnPlayerSkillAttack(int skillType)
    {
        // 开启防御屏障
        DefenseRealm.SetActive(true);
        
        // Boss受到固定伤害：当前阶段最大生命值的1/10
        int fixedDamage = MaxHP / 10;
        TakeDamage(fixedDamage);
        
        Debug.Log($"Boss受到{skillType}技能攻击固定伤害: {fixedDamage}");
    }

    public void DefenseEnd()
    {
        DefenseRealm.SetActive(false);
    }
    
    /// <summary>
    /// 检查战斗结果
    /// </summary>
    /// <returns>true表示玩家击败成功（boss处于锁血状态），false表示玩家失败（boss仍存活）</returns>
    public bool CheckOver()
    {
        if (isLockingHP)
        {
            // boss处于锁血状态，玩家击败成功
            Debug.Log("玩家击败成功！Boss处于锁血状态");
            return true;
        }
        else
        {
            // boss仍然存活，时间到，玩家失败
            Debug.Log("玩家击杀失败！Boss仍然存活");
            return false;
        }
    }
    
    /// <summary>
    /// 更新血条显示
    /// </summary>
    private void UpdateHPBar()
    {
        if (bossAnime != null)
        {
            bossAnime.SetHpBar(HP, MaxHP);
        }
    }
    
    /// <summary>
    /// 设置对应波次的血量
    /// </summary>
    /// <param name="phaseIndex">波次索引（0: none1, 1: card1, 2: none2, 3: card2）</param>
    /// <param name="totalTime">当前阶段总时长（秒）</param>
    public void SetPhaseHP_Time(int phaseIndex, float totalTime = 0f)
    {
        // 更新当前阶段索引
        currentPhaseIndex = phaseIndex;
        // 重置锁血状态
        isLockingHP = false;
        // 重置当前符卡伤害统计
        currentPhaseDamage = 0;
        // 重置阶段补正受伤系数
        phaseDamageMultiplier = 1f;
        // 重置补正标志
        hasApplied30PercentCorrection = false;
        hasApplied70PercentCorrection = false;
        // 设置阶段总时长
        phaseTotalTime = totalTime;
        // 重置阶段已进行时间
        phaseElapsedTime = 0f;
        
        switch (phaseIndex)
        {
            case 0:
                HP = hpConfig.none1HP;
                MaxHP = hpConfig.none1HP;
                break;
            case 1:
                HP = hpConfig.card1HP;
                MaxHP = hpConfig.card1HP;
                break;
            case 2:
                HP = hpConfig.none2HP;
                MaxHP = hpConfig.none2HP;
                break;
            case 3:
                HP = hpConfig.card2HP;
                MaxHP = hpConfig.card2HP;
                break;
        }
        
        // 记录阶段开始时的血量
        hpAtPhaseStart = HP;
        
        UpdateHPBar();
        
        Debug.Log($"阶段 [{phaseIndex}] 开始，总时长={totalTime}秒，血量={HP}");
    }
    
    /// <summary>
    /// 获取当前阶段索引
    /// </summary>
    /// <returns>当前阶段索引</returns>
    private int GetCurrentPhaseIndex()
    {
        return currentPhaseIndex;
    }
    
    /// <summary>
    /// 获取当前血量
    /// </summary>
    /// <returns>当前血量值</returns>
    public int GetHP()
    {
        return HP;
    }
    
    /// <summary>
    /// 获取最大血量
    /// </summary>
    /// <returns>最大血量值</returns>
    public int GetMaxHP()
    {
        return MaxHP;
    }
}

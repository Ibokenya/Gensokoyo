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
    private float defense = 0f; // 减伤值
    
    [Header("血量配置")]
    public HpConfig hpConfig;
    
    [Header("引用")]
    public BossAnime bossAnime;
    public BossUI bossUI;
    
    private void Start()
    {
        // 初始化血条
        UpdateHPBar();
    }
    
    /// <summary>
    /// 处理Boss受伤
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        // 应用减伤
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage - defense));
        HP = Mathf.Max(0, HP - finalDamage);
        
        // 更新血条
        UpdateHPBar();
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
    public void SetPhaseHP(int phaseIndex)
    {
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
        UpdateHPBar();
    }
}

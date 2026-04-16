using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBase : MonoBehaviour
{
    [Header("Boss基础属性")]
    public int HP;
    public int MaxHP;
    public float defense = 0f; // 减伤值
    
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
        
        // 检查是否死亡
        if (HP <= 0)
        {
            Die();
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
    /// Boss死亡
    /// </summary>
    private void Die()
    {
        // 在这里添加死亡逻辑
        Debug.Log("Boss已死亡");
    }
}

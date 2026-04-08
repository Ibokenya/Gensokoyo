using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魔理沙常规技能脚本
/// 挂载在"魔理沙常规"子物体上
/// </summary>
public class MarisaNormal : MonoBehaviour
{
    [Header("播放控制")]
    public bool IsAnime = false; // 设置为true开始播放动画
    private Animator animator; // 子物体上的动画组件
    
    [Header("脚本引用")]
    public SpellCardEffect spellCardEffect; // 引用父物体的SpellCardEffect脚本
    public ClearAllBullet clearAllBullet; // 引用ClearAllBullet脚本
    
    [Header("音效设置")]
    public AudioClip MarisaNormalClip;//魔理沙常规音效clip
    
    [Header("伤害设置")]
    private int Timer = 20;// 定时器，用于技能出伤
    private bool isDamage = false;// 是否正在出伤
    
    private List<GameObject> Enemys => Global_GameManager.Instance.EnemyList;// 敌人列表
    
    void Awake()
    {
        // 获取子物体上的Animator组件
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 未找到Animator组件");
        }
    }

    void OnEnable()
    {
        // 重置状态
        IsAnime = false;
        isDamage = false;
        Timer = 20;
    }
    
    void Update()
    {
        // 检查是否需要开始播放动画
        if (animator != null)
        {
            // 设置Animator的IsAnime参数
            animator.SetBool("IsAnime", IsAnime);
        }
        
        // 如果正在播放，处理出伤逻辑
        if (IsAnime)
        {
            HandleDamage();
        }
    }
    
    /// <summary>
    /// 处理出伤逻辑
    /// </summary>
    void HandleDamage()
    {
        if (Timer > 0)
        {
            Timer--;
        }
        if (Timer <= 0)
        {
            Timer = 20;
            if (isDamage)
            {
                MarisaNormalDamage();
            }
        }
    }
    
    /// <summary>
    /// 开始出伤
    /// </summary>
    public void StartToDamage()
    {
        isDamage = true;
    }
    
    /// <summary>
    /// 魔理沙常规伤害（待实现）
    /// </summary>
    public void MarisaNormalDamage()
    {
        // 待实现魔理沙常规技能伤害逻辑
        if (Enemys.Count > 0)
        {
            // 创建临时列表以避免在遍历过程中修改原始列表
            List<GameObject> tempEnemys = new List<GameObject>(Enemys);
            foreach (var enemy in tempEnemys)
            {
                if (enemy != null)
                {
                    // 这里添加魔理沙常规技能的伤害逻辑
                    // enemy.GetComponent<Enemy>().Damage(damageValue);
                }
            }
        }
    }
    
    /// <summary>
    /// 播放魔理沙常规音效
    /// </summary>
    public void AudioMarisaNormal()
    {
        if (MarisaNormalClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(MarisaNormalClip);
        }
        else
        {
            Debug.Log("没有魔理沙常规音效");
        }
    }
    
    /// <summary>
    /// 清除屏幕子弹
    /// </summary>
    public void ClearAllBullet()
    {
        if (clearAllBullet != null)
        {
            clearAllBullet.ClearScreenBullet(false);
        }
        else if (spellCardEffect != null && spellCardEffect.clearAllBullet != null)
        {
            spellCardEffect.clearAllBullet.ClearScreenBullet(false);
        }
    }
    
    /// <summary>
    /// 动画结束回调
    /// </summary>
    public void OnAnimationEnd()
    {
        IsAnime = false;
        isDamage = false;
        
        // 重置Animator参数
        if (animator != null)
        {
            animator.SetBool("IsAnime", false);
        }
        
        // 通知父脚本动画结束
        if (spellCardEffect != null)
        {
            spellCardEffect.OnChildAnimationEnd(3); // 3表示魔理沙常规
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySystem;

/// <summary>
/// 魔理沙常规技能脚本
/// 挂载在"魔理沙常规"子物体上
/// </summary>
public class MarisaNormal : MonoBehaviour
{
    [Header("播放控制")]
    public bool IsAnime = false; // 设置为true开始播放动画
    public Animator animator; // 子物体上的动画组件
    
    [Header("脚本引用")]
    public SpellCardEffect spellCardEffect; // 引用父物体的SpellCardEffect脚本
    public ClearAllBullet clearAllBullet; // 引用ClearAllBullet脚本
    public LightCircle lightCircle; // 引用LightCircle脚本
    public PlayerAnime playerAnime; // 引用PlayerAnime脚本
    public MagicAnime magicAnime; // 引用MagicAnime脚本
    
    [Header("音效设置")]
    public AudioClip MarisaNormalClip;//魔理沙常规音效clip

    [Header("boss对象")]
    public GameObject boss; // Boss对象
    
    [Header("伤害设置")]
    private readonly int MarisaNormalDamageValue = 25;  // 原始 16 → 按用户要求改为 25
    private float nextDamageTime = 0f;                  // 下一次出伤的 SimTime
    private bool isDamage = false;
    public static bool IsSkillSlowDown = false;
    
    // 原始设计：动画 150 Update帧(2.5s @60fps) → 30段 × 5帧间隔 × 25伤害
    // 5帧 @60fps = 0.0833s
    private const float DamageInterval = 0.0833f;
    
    public Transform playerTransform;
    private List<GameObject> Enemys => Global_GameManager.Instance.EnemyList;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogWarning($"[{gameObject.name}] 未找到Animator组件");
    }

    void OnEnable()
    {
        IsAnime = false;
        isDamage = false;
        nextDamageTime = 0f;
        Global_GameManager.Instance.state = State.SpellCard;
        
        if (lightCircle != null) lightCircle.ResetCircles();
        MarisaNormalDamageToBoss();
    }
    
    void FixedUpdate()
    {
        if (animator != null) animator.SetBool("IsAnime", IsAnime);
    }
    
    /// <summary>
    /// HandleDamage 必须在 Update 里跑（和 Animator.StartToDamage/OnAnimationEnd 同步）。
    /// 用 SimClock.SimTime 差值保证确定性。
    /// </summary>
    void Update()
    {
        if (IsAnime && isDamage)
        {
            if (nextDamageTime <= 0f)
                nextDamageTime = SimClock.SimTime + DamageInterval;
            
            if (SimClock.SimTime >= nextDamageTime)
            {
                MarisaNormalDamage();
                nextDamageTime = SimClock.SimTime + DamageInterval;
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
    /// 停止出伤
    /// </summary>
    public void StopToDamage()
    {
        isDamage = false;
    }

    /// <summary>
    /// 清除所有连线
    /// </summary>
    public void ClearMagicLines()
    {
        if (magicAnime != null)
        {
            magicAnime.ClearLines();
        }
    }
    
    /// <summary>
    /// 魔理沙常规伤害
    /// </summary>
    public void MarisaNormalDamage()
    {
        if (Enemys.Count > 0)
        {
            // 创建临时列表以避免在遍历过程中修改原始列表
            List<GameObject> tempEnemys = new List<GameObject>(Enemys);
            foreach (var enemy in tempEnemys)
            {
                //身前伤害判定
                if (enemy != null && enemy.transform.position.y > (playerTransform.position.y + 0.5f))
                {
                    enemy.GetComponent<Enemy>().Damage(MarisaNormalDamageValue);
                }
            }
        }
    }
    
    /// <summary>
    /// 魔理沙常规对Boss发送技能攻击通知
    /// </summary>
    private void MarisaNormalDamageToBoss()
    {
        if (boss != null && boss.activeInHierarchy)
        {
            BossBase bossBase = boss.GetComponent<BossBase>();
            if (bossBase != null)
            {
                // 发送技能攻击通知，不直接造成伤害，让Boss有机会规避
                bossBase.OnPlayerSkillAttack(3); // 3表示魔理沙常规
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

    public void SlowDown()
    {
        IsSkillSlowDown = true;
        playerAnime.SetMoveSpeed(1f);
    }

    public void ResetMoveSpeed()
    {
        IsSkillSlowDown = false;
        // 根据shift按键状态设置移速和动画
        if (ReplayManager.Input.GetKey(LogicalKey.Shift))
        {
            // 低速态
            playerAnime.SetMoveSpeed(playerAnime.MoveSpeed * 0.4f);
        }
        else
        {
            // 快速态
            playerAnime.SetMoveSpeed(playerAnime.MoveSpeed);
        }
    }
    
    /// <summary>
    /// 动画结束回调
    /// </summary>
    public void OnAnimationEnd()
    {
        Global_GameManager.Instance.SetNoDead(0.1f,State.Gaming);
        BossBase bossBase = boss.GetComponent<BossBase>();
        if (bossBase != null)
        {
            bossBase.DefenseEnd(); // 关闭防御屏障
        }
        IsAnime = false;
        isDamage = false;
        
        // 重置Animator参数
        if (animator != null)
        {
            animator.SetBool("IsAnime", false);
        }
        
        // 重置光圈
        if (lightCircle != null)
        {
            lightCircle.ResetCircles();
        }
        
        // 通知父脚本动画结束
        if (spellCardEffect != null)
        {
            spellCardEffect.OnChildAnimationEnd(3); // 3表示魔理沙常规
        }
    }
}
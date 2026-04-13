using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魔理沙决死技能脚本
/// 挂载在"魔理沙决死"子物体上
/// </summary>
public class MarisaSuper : MonoBehaviour
{
    [Header("播放控制")]
    public bool IsAnime = false; // 设置为true开始播放动画
    private Animator animator; // 子物体上的动画组件
    
    [Header("脚本引用")]
    public SpellCardEffect spellCardEffect; // 引用父物体的SpellCardEffect脚本
    public ClearAllBullet clearAllBullet; // 引用ClearAllBullet脚本
    public ParticleConection particleConection; // 引用质点连线脚本
    
    [Header("音效设置")]
    public AudioClip TimeOverClip;//钟声音效clip
    public AudioClip FireClip;// 火焰音效clip
    
    [Header("伤害设置")]
    private int Timer = 20;// 定时器，用于技能出伤*14
    private readonly int damageValue = 100;// 伤害值

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
        Global_GameManager.Instance.state = State.TimeStop;

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
                MarisaHitDamage();
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
    /// 魔理沙决死伤害
    /// </summary>
    public void MarisaHitDamage()
    {
        if (Enemys.Count > 0)
        {
            // 创建临时列表以避免在遍历过程中修改原始列表
            List<GameObject> tempEnemys = new(Enemys);
            foreach (var enemy in tempEnemys)
            {
                if (enemy != null)
                {
                    if (enemy.TryGetComponent<Enemy>(out var enemyComponent))
                    {
                        // 调用Damage方法，确保设置isKilled标志
                        // 时停期间，Die方法会检测时停状态并延迟处理
                        enemyComponent.Damage(damageValue);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 处理时停期间死亡的敌人
    /// </summary>
    private void ProcessDeadEnemies()
    {
        // 创建敌人列表的副本以避免遍历过程中修改原始列表
        List<GameObject> enemiesToProcess = new(Enemys);
        
        foreach (var enemy in enemiesToProcess)
        {
            if (enemy != null)
            {
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null && enemyComponent.Hp <= 0)
                {
                    // 手动调用Delete方法处理死亡敌人
                    enemyComponent.Delete();
                }
            }
        }
    }
    
    /// <summary>
    /// 播放钟声音效
    /// </summary>
    public void AudioTimeOver()
    {
        if (TimeOverClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(TimeOverClip);
        }
        else
        {
            Debug.Log("没有钟声音效");
        }
    }
    
    /// <summary>
    /// 播放火焰音效
    /// </summary>
    public void AudioFire()
    {
        if (FireClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(FireClip);
        }
        else
        {
            Debug.Log("没有火焰音效");
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
    /// 时停方法
    /// </summary>
    public void TimeStop()
    {
        // 设置时间缩放为0
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 开始创建连线
    /// 由动画事件调用
    /// </summary>
    public void StartCreateLines()
    {
        if (particleConection != null)
        {
            particleConection.StartCreateLines();
        }
    }

    /// <summary>
    /// 清除质点连线
    /// 由动画事件调用
    /// </summary>
    public void ClearPointLines()
    {
        if (particleConection != null)
        {
            particleConection.ClearLines();
        }
    }
    
    /// <summary>
    /// 动画结束回调
    /// </summary>
    public void OnAnimationEnd()
    {
        IsAnime = false;
        isDamage = false;

        // 将时间缩放改回1
        Time.timeScale = 1f;
        
        // 处理时停期间死亡的敌人
        ProcessDeadEnemies();
        
        // 返回0.1s的无敌
        Global_GameManager.Instance.SetNoDead(0.1f, State.Gaming);
        
        // 重置Animator参数
        if (animator != null)
        {
            animator.SetBool("IsAnime", false);
        }
        
        // 通知父脚本动画结束
        if (spellCardEffect != null)
        {
            spellCardEffect.OnChildAnimationEnd(4); // 4表示魔理沙决死
        }
    }
}

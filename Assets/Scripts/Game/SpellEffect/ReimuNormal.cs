using System.Collections;
using System.Collections.Generic;
using ReplaySystem;
using UnityEngine;

/// <summary>
/// 灵梦常规技能脚本
/// 挂载在"灵梦常规"子物体上
/// </summary>
public class ReimuNormal : MonoBehaviour
{
    [Header("播放控制")]
    public bool IsAnime = false; // 设置为true开始播放动画
    public Animator animator; // 子物体上的动画组件
    
    [Header("脚本引用")]
    public SpellCardEffect spellCardEffect; // 引用父物体的SpellCardEffect脚本
    public CardsRotate cardsRotate; // 引用CardsRotate脚本
    public ClearAllBullet clearAllBullet; // 引用ClearAllBullet脚本
    
    [Header("音效设置")]
    public AudioClip ReimuNormalClip;//灵梦常规音效clip
    public AudioClip FireClip;//火焰音效clip

    [Header("boss对象")]
    public GameObject boss; // Boss对象
    
    [Header("伤害设置")]
    private readonly int ReimuFireDamage = 50;// 灵梦常规伤害(实际出伤*15)
    
    private List<GameObject> Enemys => Global_GameManager.Instance.EnemyList;
    private float nextDamageTime = 0f;    // 下一次出伤的 SimTime
    private bool isDamage = false;         // 是否正在出伤
    // 原始设计：动画 90 Update帧(1.5s @60fps) → 15段 × 6帧间隔 × 50伤害
    // 6帧 @60fps = 0.1s → Tick 换算：6帧 = 3 tick (50Hz) 或直接 SimTime 差值
    private const float DamageInterval = 0.1f;  // 每 0.1s 出一次伤（6 Update帧 @60fps）
    
    void OnEnable()
    {
        IsAnime = false;
        isDamage = false;
        nextDamageTime = 0f;
        Global_GameManager.Instance.state = State.SpellCard;
        ReimuNormalDamageToBoss();
    }
    
    void FixedUpdate()
    {
        if (animator != null) animator.SetBool("IsAnime", IsAnime);
    }
    
    /// <summary>
    /// HandleDamage 必须在 Update（渲染帧）里跑，和 Animator 事件同步。
    /// 用 SimClock.SimTime 时间间隔保证确定性（不依赖 Update 帧计数）。
    /// </summary>
    void Update()
    {
        if (IsAnime && isDamage)
        {
            if (nextDamageTime <= 0f)
                nextDamageTime = SimClock.SimTime + DamageInterval;
            
            if (SimClock.SimTime >= nextDamageTime)
            {
                ReimuNormalDamage();
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
    /// 灵梦常规伤害
    /// </summary>
    public void ReimuNormalDamage()
    {
        if (Enemys.Count > 0)
        {
            // 创建临时列表以避免在遍历过程中修改原始列表
            List<GameObject> tempEnemys = new List<GameObject>(Enemys);
            foreach (var enemy in tempEnemys)
            {
                if (enemy != null)
                {
                    enemy.GetComponent<Enemy>().Damage(ReimuFireDamage);
                }
            }
        }
    }
    
    /// <summary>
    /// 灵梦常规对Boss发送技能攻击通知
    /// </summary>
    private void ReimuNormalDamageToBoss()
    {
        if (boss != null && boss.activeInHierarchy)
        {
            BossBase bossBase = boss.GetComponent<BossBase>();
            if (bossBase != null)
            {
                // 发送技能攻击通知，不直接造成伤害，让Boss有机会规避
                bossBase.OnPlayerSkillAttack(1); // 1表示灵梦常规
            }
        }
    }
    
    /// <summary>
    /// 播放灵梦常规音效
    /// </summary>
    public void AudioReimuNormal()
    {
        if (ReimuNormalClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(ReimuNormalClip);
        }
        else
        {
            Debug.Log("没有灵梦常规音效");
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
    /// 符卡淡出
    /// </summary>
    public void SpellCardFadeOut()
    {
        if (cardsRotate != null)
        {
            cardsRotate.FadeOut();
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
        
        // 通知父脚本动画结束
        if (spellCardEffect != null)
        {
            spellCardEffect.OnChildAnimationEnd(1); // 1表示灵梦常规
        }
    }
}

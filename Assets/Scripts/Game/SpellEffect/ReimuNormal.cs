using System.Collections;
using System.Collections.Generic;
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
    
    private List<GameObject> Enemys => Global_GameManager.Instance.EnemyList;// 敌人列表
    private int Timer = 20;// 定时器，用于技能出伤
    private bool isDamage = false;// 是否正在出伤
    
    void OnEnable()
    {
        // 重置状态
        IsAnime = false;
        isDamage = false;
        Timer = 20;
        Global_GameManager.Instance.state = State.SpellCard;
        // 对Boss造成伤害
        ReimuNormalDamageToBoss();
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
                ReimuNormalDamage();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 灵梦决死技能脚本
/// 挂载在"灵梦决死"子物体上
/// </summary>
public class ReimuSuper : MonoBehaviour
{
    [Header("播放控制")]
    public bool IsAnime = false; // 设置为true开始播放动画
    public Animator animator; // 子物体上的动画组件
    
    [Header("脚本引用")]
    public SpellCardEffect spellCardEffect; // 引用父物体的SpellCardEffect脚本
    public ClearAllBullet clearAllBullet; // 引用ClearAllBullet脚本

    [Header("boss对象")]
    public GameObject boss; // Boss对象
    
    [Header("物体引用")]
    public GameObject player;// 玩家物体
    public GameObject spaceEye;// 亚空穴物体
    public List<GameObject> attackEffects;// 攻击效果对象列表（下踹、掌击、器械击、侧踢）
    public GameObject WinEffect;// 退治效果
    
    [Header("音效设置")]
    public AudioClip TimeStopClip;//灵梦决死时停音效clip
    public List<AudioClip> ReimuHitList;//灵梦决死延迟音效clip队列（2个）
    
    [Header("伤害设置")]
    private readonly int ReimuHitDamageValue = 250;// 灵梦决死伤害
    private readonly float attackEffectDuration = 0.25f;// 攻击效果持续时间
    private readonly float attackEffectInterval = 0.167f;// 攻击效果间隔时间
    
    private List<GameObject> Enemys => Global_GameManager.Instance.EnemyList;// 敌人列表
    private bool isOpenOrCloseEye = false;// 是否正在开关亚空穴
    private Coroutine huntCoroutine;// 猎杀敌人的协程
    private float originalTimeScale = 1f;// 原始时间缩放值

    void OnEnable()
    {
        // 重置状态
        IsAnime = false;
        isOpenOrCloseEye = false;
        // 设置游戏状态为时停
        Global_GameManager.Instance.state = State.TimeStop;
        // 对Boss造成伤害
        ReimuSuperDamageToBoss();
    }
    
    void Update()
    {     
        // 检查是否需要开始播放动画
        if (animator != null)
        {
            // 设置Animator的IsAnime参数
            animator.SetBool("IsAnime", IsAnime);
        }
        // 如果正在播放，处理亚空穴绑定
        if (IsAnime)
        {
            if (player != null && spaceEye != null)
            {
                SpaceEyeToPlayer();
            }
            if (isOpenOrCloseEye && player != null && spaceEye != null)
            {
                player.transform.localScale = new Vector3(
                    spaceEye.transform.localScale.x * 4, 
                    player.transform.localScale.y, 
                    player.transform.localScale.z);
            }
        }
    }
    
    /// <summary>
    /// 亚空穴绑定玩家坐标
    /// </summary>
    public void SpaceEyeToPlayer()
    {
        if (spaceEye != null && player != null)
        {
            spaceEye.transform.position = player.transform.position;
        }
    }
    
    /// <summary>
    /// 开关亚空穴（反转状态）
    /// </summary>
    public void OpenOrCloseSpaceEye()
    {
        isOpenOrCloseEye = !isOpenOrCloseEye;
    }
    
    /// <summary>
    /// 播放决死时停音效
    /// </summary>
    public void AudioTimeStop()
    {
        if (TimeStopClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(TimeStopClip);
        }
    }
    
    /// <summary>
    /// 随机播放两种击打音效之一
    /// </summary>
    public void AudioReimuHit()
    {
        if (ReimuHitList != null && ReimuHitList.Count > 0)
        {
            int randomIndex = Random.Range(0, ReimuHitList.Count);
            Global_AudioManager.Instance.PlaySFX(ReimuHitList[randomIndex]);
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
    /// 时停结束，开始猎杀
    /// </summary>
    public void TimeStopOver()
    {
        huntCoroutine = StartCoroutine(HuntEnemiesCoroutine());
    }
    
    /// <summary>
    /// 猎杀敌人的协程
    /// </summary>
    private IEnumerator HuntEnemiesCoroutine()
    {
        // 记录协程开始时间（用于控制退治效果显示时机）
        float startTime = Time.realtimeSinceStartup;
        
        // 保存当前时间缩放
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        // 创建敌人列表的副本
        List<GameObject> tempEnemys = new List<GameObject>();
        foreach (var enemy in Enemys)
        {
            if (enemy != null)
            {
                tempEnemys.Add(enemy);
            }
        }
        
        // 确保攻击效果列表不为空
        if (attackEffects == null || attackEffects.Count == 0)
        {
            Time.timeScale = originalTimeScale;
            yield break;
        }
        
        // 用于跟踪激活的攻击效果
        List<GameObject> activeAttackEffects = new List<GameObject>();
        
        // 攻击效果偏移量配置（默认左侧）
        Vector2[] attackOffsets = new Vector2[]
        {
            new Vector2(-0.5f, 0.5f), // 下踹
            new Vector2(-0.7f, 0f),   // 掌击
            new Vector2(-0.8f, 0f),   // 器械击
            new Vector2(-0.8f, 0f)    // 侧踢
        };
        
        // 持续循环直到动画结束
        while (IsAnime)
        {
            // 过滤掉已经被销毁或死亡的敌人（Hp <= 0表示死亡）
            tempEnemys.RemoveAll(enemy => enemy == null || enemy.GetComponent<Enemy>().Hp <= 0);
            
            // 如果还有存活的敌人，继续攻击
            if (tempEnemys.Count > 0)
            {
                // 遍历每个敌人
                for (int i = 0; i < tempEnemys.Count && IsAnime; i++)
                {
                    var enemy = tempEnemys[i];
                    if (enemy != null)
                    {
                        // 等待攻击效果间隔
                        yield return new WaitForSecondsRealtime(attackEffectInterval);
                        
                        // 查找可用的攻击效果对象
                        List<GameObject> availableEffects = new List<GameObject>();
                        foreach (var effect in attackEffects)
                        {
                            if (effect != null && !activeAttackEffects.Contains(effect) && !effect.activeSelf)
                            {
                                availableEffects.Add(effect);
                            }
                        }
                        
                        if (availableEffects.Count > 0)
                        {
                            // 随机选择一个可用的攻击效果对象
                            GameObject availableAttackEffect = availableEffects[Random.Range(0, availableEffects.Count)];
                            
                            // 随机选择攻击方向
                            bool isRight = Random.value > 0.5f;
                            
                            // 随机选择攻击效果类型
                            int attackIndex = Random.Range(0, attackOffsets.Length);
                            
                            // 计算攻击效果的位置
                            Vector3 enemyPosition = enemy.transform.position;
                            Vector3 effectPosition = enemyPosition;
                            
                            // 获取当前攻击效果的偏移量
                            Vector2 offset = attackOffsets[attackIndex];
                            
                            // 根据攻击方向调整偏移量
                            if (isRight)
                            {
                                offset.x = -offset.x;
                            }
                            
                            // 应用偏移量
                            effectPosition.x += offset.x;
                            effectPosition.y += offset.y;
                            
                            // 设置攻击效果的位置和旋转
                            availableAttackEffect.transform.position = effectPosition;
                            availableAttackEffect.transform.localScale = new Vector3(isRight ? 1 : -1, 1, 1);
                            
                            // 激活攻击效果
                            availableAttackEffect.SetActive(true);
                            activeAttackEffects.Add(availableAttackEffect);
                            
                            // 播放击打音效
                            AudioReimuHit();
                            
                            // 延迟后禁用攻击效果并造成伤害
                            StartCoroutine(DisableAttackEffectAndDamage(availableAttackEffect, activeAttackEffects, enemy));
                        }
                    }
                }
            }
            else
            {
                // 计算从协程开始到现在的时间
                float elapsedTime = Time.realtimeSinceStartup - startTime;
                
                // 如果还没到2秒，等待剩余时间
                if (elapsedTime < 2f)
                {
                    yield return new WaitForSecondsRealtime(2f - elapsedTime);
                }
                
                // 没有敌人时，激活退治效果（至少在动画开始后2秒）
                if (WinEffect != null)
                {
                    WinEffect.SetActive(true);
                }
                
                // 等待一小段时间后停止猎杀
                yield return new WaitForSecondsRealtime(0.5f);
                break; // 退出猎杀循环
            }
            
            // 等待一段时间后再开始下一轮攻击
            yield return new WaitForSecondsRealtime(attackEffectInterval * 2);
        }
    }
    
    /// <summary>
    /// 禁用攻击效果并造成伤害的协程
    /// </summary>
    private IEnumerator DisableAttackEffectAndDamage(GameObject attackEffect, List<GameObject> activeAttackEffects, GameObject enemy)
    {
        yield return new WaitForSecondsRealtime(attackEffectDuration);
        
        // 对敌人造成伤害
        if (enemy != null)
        {
            var enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.Damage(ReimuHitDamageValue);
            }
        }
        
        // 禁用攻击效果
        if (attackEffect != null)
        {
            attackEffect.SetActive(false);
            activeAttackEffects.Remove(attackEffect);
        }
    }
    
    /// <summary>
    /// 灵梦决死对Boss发送技能攻击通知
    /// </summary>
    private void ReimuSuperDamageToBoss()
    {
        if (boss != null && boss.activeInHierarchy)
        {
            BossBase bossBase = boss.GetComponent<BossBase>();
            if (bossBase != null)
            {
                // 发送技能攻击通知，不直接造成伤害，让Boss有机会规避
                bossBase.OnPlayerSkillAttack(2); // 2表示灵梦决死
            }
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
        // 停止猎杀协程
        if (huntCoroutine != null)
        {
            StopCoroutine(huntCoroutine);
            huntCoroutine = null;
        }
        // 重置状态
        IsAnime = false;
        isOpenOrCloseEye = false;
        
        // 重置Animator参数
        if (animator != null)
        {
            animator.SetBool("IsAnime", false);
        }
        
        WinEffect.SetActive(false);
        // 恢复时间缩放
        Time.timeScale = 1f;
        
        // 通知父脚本动画结束
        if (spellCardEffect != null)
        {
            spellCardEffect.OnChildAnimationEnd(2); // 2表示灵梦决死
        }
    }
    /// <summary>
    /// 灵梦从亚空穴返回（关闭退治效果）
    /// </summary>
    public void Back()
    {
        player.transform.position = new(-3,-4,0);
        // 恢复时间缩放
        Time.timeScale = originalTimeScale;
        
        // 恢复游戏状态为游戏中
        Global_GameManager.Instance.state = State.Gaming;
        
        // 处理时停期间死亡的敌人
        ProcessDeadEnemies();
    }
    
    /// <summary>
    /// 处理时停期间死亡的敌人
    /// </summary>
    private void ProcessDeadEnemies()
    {
        // 创建敌人列表的副本以避免遍历过程中修改原始列表
        List<GameObject> enemiesToProcess = new List<GameObject>(Enemys);
        
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
}

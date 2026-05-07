using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalCard : MonoBehaviour
{
    public GameObject IcePearl;//冰珠
    public GameObject IcePick;//冰刺
    public GameObject FrozenBall;//冰球
    public GameObject IceSpike;//冰锥
    public GameObject IceFlake;//冰花
    public GameObject RotatePick;//旋转冰刺
    public IceRealm IceRealm;//冰领域（场景固有对象）

    [Header("脚本引用")]
    public ChangeBG changeBG;
    
    [Header("FinalCard阶段参数")]
    public int currentPhase = 1; // 当前阶段（1-4）
    public float phaseTimer = 0f; // 阶段计时器
    public float phase1Duration = 12f; // 第一阶段持续时间
    public float phase2Duration = 12f; // 第二阶段持续时间
    public float phase3Duration = 12f; // 第三阶段持续时间
    public float phase4Duration = 12f; // 第四阶段持续时间
    public int snowFlakeCount = 50; // 雪花生成总数
    public float attackInterval = 6f; // 攻击间隔（每隔多久发动一次雪花攻击）
    
    public BossShootSystem bossShootSystem;
    private Coroutine icePearlCoroutine_left;
    private Coroutine icePearlCoroutine_right;
    
    private void OnEnable()
    {
        // 重置阶段参数
        currentPhase = 1;
        phaseTimer = 0f;
        
        // 重置协程引用
        icePearlCoroutine_left = null;
        icePearlCoroutine_right = null;
        
        // 初始化冰锥对象池（20个）
        if (IceSpike != null)
        {
            Global_ObjectPool.Instance.InitPool(IceSpike, 20);
        }
        if(FrozenBall != null)
        {
            Global_ObjectPool.Instance.InitPool(FrozenBall, 20);
        }
        if(RotatePick != null)
        {
            Global_ObjectPool.Instance.InitPool(RotatePick, 18);
        }
        
        // 设置BossShootSystem的IceSpike引用
        if (bossShootSystem != null && IceSpike != null)
        {
            bossShootSystem.IceSpike = IceSpike;
        }
        
        // 开始射击
        StartShooting();
    }

    private void OnDisable()
    {
        bossShootSystem.isRealmActive = false;
        IceRealm.StartFadeOut();
        bossShootSystem.isAllowAreaLimit = false;
        // 停止所有协程
        StopAllCoroutines();
        // 取消所有 Invoke 调用
        CancelInvoke();
        
        // 停止 BossShootSystem 中的所有射击协程
        if (bossShootSystem != null)
        {
            bossShootSystem.StopAllShooting();
            bossShootSystem.ClearBullet();
        }
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
        switch (currentPhase)
        {
            case 1:
                // 第一阶段：同时发射冰珠和冰刺，12秒后进入第二阶段
                if (phaseTimer >= phase1Duration)
                {
                    currentPhase = 2;
                    phaseTimer = 0f;
                    Debug.Log("FinalCard进入第二阶段");
                    // 阶段切换，启动新的射击
                    StartShooting();
                }
                break;
            case 2:
                // 第二阶段：冻结冰珠并扩大冰球，继续发射冰刺，12秒后进入第三阶段
                if (phaseTimer >= phase2Duration)
                {
                    currentPhase = 3;
                    phaseTimer = 0f;
                    Debug.Log("FinalCard进入第三阶段");
                    // 阶段切换，启动新的射击
                    bossShootSystem.isAllowAreaLimit = true;
                    StartShooting();
                }
                break;
            case 3:
                // 第三阶段：启动AreaLimit方法，继续发射冰刺
                if (phaseTimer >= phase3Duration)
                {
                    currentPhase = 4;
                    phaseTimer = 0f;
                    Debug.Log("FinalCard进入第四阶段");
                    // 阶段切换，启动新的射击
                    StartShooting();
                }
                break;
            case 4:
                IceRealm.enabled = true;
                changeBG.ShowMagicEffect();
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
            // 第一阶段：同时发射冰珠和冰刺
            if (currentPhase == 1 && IcePearl != null && IcePick != null)
            {
                // 冰珠从左侧开始（从左向右扫），存储子弹以执行后续冻结
                icePearlCoroutine_left = bossShootSystem.RepeatShoot(IcePearl, 60f, 53f, 0.16f, true, true);
                // 冰珠从右侧开始（从右向左扫），存储子弹以执行后续冻结
                icePearlCoroutine_right = bossShootSystem.RepeatShoot(IcePearl, 60f, 47f, 0.22f, false, true);
                
                // 冰刺从左侧开始（从左向右扫）
                bossShootSystem.RepeatShoot(IcePick, 60f, 41f, 0.22f, true);
                // 冰刺从右侧开始（从右向左扫）
                bossShootSystem.RepeatShoot(IcePick, 60f, 37f, 0.26f, false);
            }
            // 第二阶段：冻结冰珠并扩大冰球
            else if (currentPhase == 2 && FrozenBall != null)
            {
                // 停止冰珠射击协程
                if (icePearlCoroutine_left != null)
                {
                    bossShootSystem.StopCoroutine(icePearlCoroutine_left);
                    icePearlCoroutine_left = null;
                }
                // 停止冰珠射击协程
                if (icePearlCoroutine_right != null)
                {
                    bossShootSystem.StopCoroutine(icePearlCoroutine_right);
                    icePearlCoroutine_right = null;
                }
                
                // 冻结所有冰珠（只执行一次）
                bossShootSystem.FrozenPearl(FrozenBall);
            }
            // 第三阶段：启动AreaLimit方法和雪花攻击
            else if (currentPhase == 3 && RotatePick != null && IceFlake != null)
            {
                // 启动AreaLimit方法
                bossShootSystem.AreaLimit(RotatePick, new Vector3(-3f, 0f, 0f), 7f, 90f, 2f, 2.5f);
                // 启动雪花攻击
                StartCoroutine(SnowFlakeAttackLoop());
                bossShootSystem.ShowTerrain();
            }
            // 第四阶段：激活冰囚笼
            else if (currentPhase == 4 && IceRealm != null)
            {
                // 激活冰领域笼
                bossShootSystem.isRealmActive = true;
                // 激活冰领域在区域限制攻击的中心点
                bossShootSystem.ActivateIceRealm(new Vector3(-3f, 0f, 0f));
                bossShootSystem.isInArea = true;
            }
        }
    }

    /// <summary>
    /// 雪花攻击循环
    /// </summary>
    private IEnumerator SnowFlakeAttackLoop()
    {
        while (true)
        {
            // 发动雪花攻击
            if (bossShootSystem != null)
            {
                bossShootSystem.SnowFlakeAttack(IceFlake, snowFlakeCount);
            }
            
            // 等待攻击间隔
            yield return new WaitForSeconds(attackInterval);
        }
    }
}

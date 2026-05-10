using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card2 : MonoBehaviour
{
    [Header("二符参数")]
    public GameObject snowFlakePrefab; // 雪花子弹预制件
    public GameObject iceCloudPrefab; // 冰云预制件

    [Header("脚本引用")]
    public BossShootSystem bossShootSystem; // 射击系统引用
    public UIManager uiManager; // UIManager脚本引用
    public BossBase bossBase; // Boss基础属性引用

    [Header("符卡收取奖励")]
    public List<ItemDropConfig> card2ClearRewards; // 符卡2收取成功时的掉落物配置

    [Header("雪花攻击参数")]
    public int snowFlakeCount = 50; // 雪花生成总数
    public float attackInterval = 4f; // 攻击间隔（每隔多久发动一次雪花攻击）

    [Header("冰云攻击参数")]
    public FreezeSystem freezeSystem; // 冻结系统引用
    public int iceCloudCount = 10; // 冰云生成数量
    public float iceCloudFloatSpeed = 0.5f; // 冰云飘浮速度
    public Vector2 cloudSpawnMin = new Vector2(-10f, -4f); // 冰云生成范围左下角
    public Vector2 cloudSpawnMax = new Vector2(4f, 4f); // 冰云生成范围右上角

    [Header("彗星攻击参数")]
    public GameObject cometPrefab; // 彗星预制件
    public GameObject linePrefab; // 连线预制件
    public float cometAttackInterval = 5f; // 彗星攻击间隔
    public float cometSpawnY = 6f; // 彗星生成y坐标
    public float cometStartDelay = 3f; // 彗星攻击启动延迟时间

    private void OnEnable()
    {
        // 初始化弹幕池
        if (snowFlakePrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(snowFlakePrefab, 80);
        }

        // 初始化冰云对象池
        if (iceCloudPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(iceCloudPrefab, iceCloudCount);
        }

        // 初始化彗星对象池
        if (cometPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(cometPrefab, 3);
        }

        // 初始化连线对象池
        if (linePrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(linePrefab, 3);
        }
        bossShootSystem.ShowColdAir();

        // 设置冻结缩放比例为1.5f
        if (freezeSystem != null)
        {
            freezeSystem.SetFrozenScale(1.5f);
        }

        // 开始攻击
        StartAttacks();
    }

    private void OnDisable()
    {
        // 停止所有协程
        StopAllCoroutines();

        // 取消所有 Invoke 调用
        CancelInvoke();
        bossShootSystem.HideColdAir();

        // 停止 BossShootSystem 中的所有射击协程
        if (bossShootSystem != null)
        {
            bossShootSystem.StopAllShooting();
            // 清除所有子弹
            bossShootSystem.ClearBullet();
        }

        bossBase.isLockingHP = false;
        
        // 恢复冻结缩放比例为1
        FreezeSystem freezeSystem = FindObjectOfType<FreezeSystem>();
        if (freezeSystem != null)
        {
            freezeSystem.ResetFrozenScale();
        }
    }

    private void StartAttacks()
    {
        // 移动到目标位置
        StartCoroutine(MoveToHerPos());

        // 启动雪花攻击协程
        StartCoroutine(SnowFlakeAttackLoop());

        // 生成冰云
        if (bossShootSystem != null && iceCloudPrefab != null)
        {
            bossShootSystem.CreateCloud(iceCloudPrefab, cloudSpawnMin, cloudSpawnMax, iceCloudCount);
        }

        // 启动彗星攻击（带延迟）
        if (bossShootSystem != null && cometPrefab != null && linePrefab != null)
        {
            StartCoroutine(StartCometAttackWithDelay());
        }
    }

    /// <summary>
    /// 带延迟启动彗星攻击
    /// </summary>
    private IEnumerator StartCometAttackWithDelay()
    {
        // 等待启动延迟时间
        yield return new WaitForSeconds(cometStartDelay);

        // 启动彗星攻击
        bossShootSystem.StartCometAttack(cometPrefab, linePrefab, cometAttackInterval, cometSpawnY);
    }

    private IEnumerator MoveToHerPos()
    {
        Vector2 targetPos = new(-3f, 3f);
        // 移动到目标位置
        transform.position = Vector3.Lerp(transform.position, targetPos, 1f);
        yield return null;
    }

    /// <summary>
    /// 雪花攻击循环
    /// </summary>
    private IEnumerator SnowFlakeAttackLoop()
    {
        // 等待移动到目标位置
        yield return new WaitForSeconds(1f);

        while (true)
        {
            // 发动雪花攻击
            if (bossShootSystem != null)
            {
                bossShootSystem.SnowFlakeAttack(snowFlakePrefab, snowFlakeCount);
            }

            // 等待攻击间隔
            yield return new WaitForSeconds(attackInterval);
        }
    }

    /// <summary>
    /// 检查boss是否已经死亡或处于锁血状态
    /// 如果boss处于锁血状态，说明玩家成功讨伐当前阶段
    /// </summary>
    public void CheckOver()
    {
        if (bossBase != null)
        {
            bool isDefeated = bossBase.CheckOver();
            if (isDefeated)
            {
                // 标记为获取了符卡2
                uiManager.isCard2Get = true;
                Debug.Log("card2阶段：玩家成功讨伐Boss！");
                // 发放符卡收取奖励
                SpawnClearRewards();
            }
            else
            {
                Debug.Log("card2阶段：Boss仍然存活，时间到");
                // 标记为未获取符卡2
                uiManager.isCard2Get = false;
            }
        }
    }

    /// <summary>
    /// 生成符卡收取奖励
    /// </summary>
    private void SpawnClearRewards()
    {
        if (bossShootSystem != null && bossShootSystem.boss != null &&
            card2ClearRewards != null && card2ClearRewards.Count > 0)
        {
            CreateItem.Instance.SpawnItems(bossShootSystem.boss.transform.position, card2ClearRewards);
            Debug.Log("card2符卡收取成功，已发放奖励");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card2 : MonoBehaviour
{
    [Header("二符参数")]
    public GameObject snowFlakePrefab; // 雪花子弹预制件
    public GameObject iceCloudPrefab; // 冰云预制件
    public BossShootSystem bossShootSystem; // 射击系统引用
    
    [Header("雪花攻击参数")]
    public int snowFlakeCount = 50; // 雪花生成总数
    public float attackInterval = 4f; // 攻击间隔（每隔多久发动一次雪花攻击）
    
    [Header("冰云攻击参数")]
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
        Vector2 targetPos = new (-3f, 3f);
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
    /// 一个关键的方法：检查boss是否已经死亡
    /// 如果boss没死亡，则会播放时间到的效果
    /// </summary>
    public void CheckOver()
    {
        
    }
}

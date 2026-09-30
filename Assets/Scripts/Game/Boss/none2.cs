using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySystem;

public class none2 : MonoBehaviour
{
    [Header("二非参数")]
    public List<Vector3> movePositions = new List<Vector3>(); // 移动坐标列表
    public float moveInterval = 3f; // 移动时间间隔
    public float shootInterval = 2f; // 射击间隔
    public int bulletCount = 5; // 每轮射击子弹数
    public GameObject randomBulletPrefab; // 随机射击子弹预制件（miniIceBall）
    public BossShootSystem bossShootSystem; // 射击系统引用
    public BossAnime bossAnime; // Boss动画引用
    public GameObject boss; // Boss对象引用
    
    [Header("子弹速度")]
    public float bulletSpeed = 5f; // 子弹速度
    
    [Header("脚本引用")]
    public BossUI bossUI; // BossUI脚本引用
    public BossBase bossBase; // Boss基础属性引用
    
    private int currentPositionIndex = 0; // 当前位置索引
    
    private void OnEnable()
    {
        // 初始化弹幕池
        if (randomBulletPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(randomBulletPrefab, 30);
        }
        bossShootSystem.HideTerrain();
        // 开始攻击
        StartAttacks();
    }
    
    private void OnDisable()
    {
        // 停止所有协程
        StopAllCoroutines();
        // 取消所有 SimTimer 定时器
        SimTimer.CancelAll();
        
        // 停止 BossShootSystem 中的所有射击协程
        if (bossShootSystem != null)
        {
            bossShootSystem.StopAllShooting();
            bossShootSystem.ClearBullet();
        }
    }
    
    private void StartAttacks()
    {
        if (boss != null && movePositions.Count > 0)
        {
            StartCoroutine(MoveToPoint1());
        }
        
        if (bossShootSystem != null)
        {
            // 启动二非随机射击，方法内部会获取当前boss位置作为目标位置
            bossShootSystem.none2RandomShoot(randomBulletPrefab, bulletSpeed, shootInterval, bulletCount);
        }
        
        // 启动移动协程
        StartCoroutine(MoveBossCoroutine());
    }

    private IEnumerator MoveToPoint1()
    {
        if (boss != null)
        {
            Vector3 startPosition = boss.transform.position;
            Vector3 targetPosition = new Vector3(-3f, 3f, 0f);
            float duration = 2f;
            float startSimTime = SimClock.SimTime;
            
            //  用 SimClock 驱动，帧次数固定（2秒 = 100 tick @50Hz）
            while (true)
            {
                float t = Mathf.Clamp01((SimClock.SimTime - startSimTime) / duration);
                t = Mathf.SmoothStep(0f, 1f, t);
                boss.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                if (t >= 1f) break;
                yield return null;
            }
            
            boss.transform.position = targetPosition;
        }
    }
    
    private IEnumerator MoveBossCoroutine()
    {
        while (true)
        {
            // 等待移动间隔（SimClock 驱动，确定性）
            yield return new WaitForSecondsSim(moveInterval);
            
            if (boss != null && movePositions.Count > 1)
            {
                // 随机选择下一个位置，排除当前位置
                //  GameRNG.Range 必须在 WaitForSecondsSim 之后立即调用——此时刻确定性且无其他 RNG 干扰
                int nextPositionIndex = currentPositionIndex;
                while (nextPositionIndex == currentPositionIndex)
                {
                    nextPositionIndex = GameRNG.Range(0, movePositions.Count);
                }
                
                Vector3 targetPosition = movePositions[nextPositionIndex];
                Vector3 startPosition = boss.transform.position;
                
                if (targetPosition.x < startPosition.x)
                {
                    if (bossAnime != null) bossAnime.SetLeft();
                }
                else if (targetPosition.x > startPosition.x)
                {
                    if (bossAnime != null) bossAnime.SetRight();
                }
                
                //  SimClock 驱动的平滑移动（duration=1f = 50 tick @50Hz）
                // 帧次数固定 → none2RandomShoot 重启时机确定性 → RNG 消费顺序稳定
                float duration = 1f;
                float startSimTime = SimClock.SimTime;
                
                while (true)
                {
                    float t = Mathf.Clamp01((SimClock.SimTime - startSimTime) / duration);
                    float st = Mathf.SmoothStep(0f, 1f, t);
                    boss.transform.position = Vector3.Lerp(startPosition, targetPosition, st);

                    if (t >= 1f) break;
                    yield return null;
                }
                
                boss.transform.position = targetPosition;
                currentPositionIndex = nextPositionIndex;
                
                if (bossAnime != null) bossAnime.SetIdle();
                
                // 重新启动射击协程（此时刻确定性，不会因帧率偏移）
                if (bossShootSystem != null)
                {
                    bossShootSystem.none2RandomShoot(randomBulletPrefab, bulletSpeed, shootInterval, bulletCount);
                }
            }
        }
    }
    
    // 检查boss是否已经死亡或处于锁血状态
    // 如果boss处于锁血状态，说明玩家成功讨伐当前阶段
    public void CheckOver()
    {
        if (bossBase != null)
        {
            bool isDefeated = bossBase.CheckOver();
            if (isDefeated)
            {
                Debug.Log("none2阶段：玩家成功讨伐Boss！");
                // 可以在这里添加讨伐成功的效果或奖励逻辑
            }
            else
            {
                Debug.Log("none2阶段：Boss仍然存活，时间到");
                // 可以在这里添加时间到的效果逻辑
            }
        }
    }
}
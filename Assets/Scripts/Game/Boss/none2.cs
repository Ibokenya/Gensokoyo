using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // 取消所有 Invoke 调用
        CancelInvoke();
        
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
            float elapsedTime = 0f;
            
            // 平滑移动boss到中心位置
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                // 使用平滑的缓动函数
                t = Mathf.SmoothStep(0f, 1f, t);
                boss.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保boss到达精确位置
            boss.transform.position = targetPosition;
        }
    }
    
    private IEnumerator MoveBossCoroutine()
    {
        while (true)
        {
            // 等待移动间隔
            yield return new WaitForSeconds(moveInterval);
            
            if (boss != null && movePositions.Count > 1)
            {
                // 随机选择下一个位置，排除当前位置
                int nextPositionIndex = currentPositionIndex;
                while (nextPositionIndex == currentPositionIndex)
                {
                    nextPositionIndex = Random.Range(0, movePositions.Count);
                }
                
                Vector3 targetPosition = movePositions[nextPositionIndex];
                Vector3 startPosition = boss.transform.position;
                
                // 确定移动方向并设置动画状态
                if (targetPosition.x < startPosition.x)
                {
                    // 向左移动
                    if (bossAnime != null)
                    {
                        bossAnime.SetLeft();
                    }
                }
                else if (targetPosition.x > startPosition.x)
                {
                    // 向右移动
                    if (bossAnime != null)
                    {
                        bossAnime.SetRight();
                    }
                }
                
                // 平滑移动到目标位置
                float duration = 1f;
                float elapsedTime = 0f;
                
                while (elapsedTime < duration)
                {
                    float t = elapsedTime / duration;
                    t = Mathf.SmoothStep(0f, 1f, t);
                    boss.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                // 确保boss到达精确位置
                boss.transform.position = targetPosition;
                currentPositionIndex = nextPositionIndex;
                
                // 恢复idle状态
                if (bossAnime != null)
                {
                    bossAnime.SetIdle();
                }
                
                // 重新启动射击协程，传递新的目标位置
                if (bossShootSystem != null)
                {
                    // 重新启动二非随机射击，传递当前boss位置作为目标位置
                    // 注意：none2RandomShoot方法内部会停止之前的射击协程
                    bossShootSystem.none2RandomShoot(randomBulletPrefab, bulletSpeed, shootInterval, bulletCount);
                }
            }
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

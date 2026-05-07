using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card1 : MonoBehaviour
{
    [Header("一符参数")]
    public GameObject stoneBulletPrefab; // 陨石子弹预制件
    public GameObject frozenIceBulletPrefab; // 冰冻子弹预制件
    public GameObject normalIceBulletPrefab; // 普通冰子弹预制件（用于冰块破裂）
    public GameObject randomIcePickBulletPrefab; // 随机射击子弹预制件
    public BossShootSystem bossShootSystem; // 射击系统引用
    public GameObject boss; // Boss对象引用
    
    [Header("陨石冰冻旋转攻击参数")]
    public int stoneCount = 5; // 陨石数量
    public float rotationSpeed = 30f; // 旋转速度
    
    [Header("随机射击参数")]
    public float bulletSpeed = 5f; // 子弹速度
    public float shootInterval = 2f; // 射击间隔
    public int bulletCount = 5; // 每轮射击子弹数
    
    [Header("脚本引用")]
    public BossUI bossUI; // BossUI脚本引用
    public BossBase bossBase; // Boss基础属性引用
    
    private void OnEnable()
    {      
        // 初始化弹幕池
        if (stoneBulletPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(stoneBulletPrefab, 20);
        }
        if (frozenIceBulletPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(frozenIceBulletPrefab, 20);
        }
        if (normalIceBulletPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(normalIceBulletPrefab, 100);
        }
        if (randomIcePickBulletPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(randomIcePickBulletPrefab, 100);
        }
        // 显示冰刺地形
        bossShootSystem.ShowTerrain();
        // 开始攻击
        StartAttacks();
    }
    
    private void OnDisable()
    {
        // 停止所有协程
        StopAllCoroutines();
        // 取消所有 Invoke 调用
        CancelInvoke();
        // 恢复所有陨石的重力
        if (bossShootSystem != null)
        {
            bossShootSystem.ResumeAllStonesGravity();
        }
        
        // 停止 BossShootSystem 中的所有射击协程
        if (bossShootSystem != null)
        {
            bossShootSystem.StopAllShooting();
        }
    }
    
    private void StartAttacks()
    {
        // 启动boss移动协程
        StartCoroutine(MoveBossToCenter());
    }
    
    private IEnumerator MoveBossToCenter()
    {
        if (boss != null)
        {
            Vector3 startPosition = boss.transform.position;
            Vector3 targetPosition = new Vector3(-3f, 0f, 0f);
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
        
        // 移动完成后开始攻击
        if (bossShootSystem != null)
        {
            // 启动陨石冰冻旋转攻击
            bossShootSystem.StoneFrozenAttack(stoneBulletPrefab, frozenIceBulletPrefab, normalIceBulletPrefab, stoneCount, rotationSpeed);
            
            // 启动随机射击
            bossShootSystem.randomIcePick(randomIcePickBulletPrefab, bulletSpeed, shootInterval, bulletCount);
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
                Debug.Log("card1阶段：玩家成功讨伐Boss！");
                // 可以在这里添加讨伐成功的效果或奖励逻辑
            }
            else
            {
                Debug.Log("card1阶段：Boss仍然存活，时间到");
                // 可以在这里添加时间到的效果逻辑
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class none1 : MonoBehaviour
{
    [Header("一非参数")]
    public float shoot_interval1 = 2f; // 射击间隔1
    public float shoot_interval2 = 1f; // 射击间隔2
    public GameObject fanBulletPrefab; // 扇形射击子弹预制件（冰玉）
    public GameObject iceBulletPrefab1; // 冰点射击预制件1（冰刺）
    public GameObject iceBulletPrefab2; // 冰点射击预制件2（冰珠）
    public BossShootSystem bossShootSystem; // 射击系统引用
    
    [Header("子弹速度")]
    public float icePickSpeed = 5f; // IcePick 速度
    public float iceJadeSpeed = 6f; // IceJade 速度
    public float icePearlSpeed = 4f; // IcePearl 速度
    public float rotationSpeed = 30f; // 旋转速度

    [Header("脚本引用")]
    public BossUI bossUI; // BossUI脚本引用
    public BossBase bossBase; // Boss基础属性引用
    
       
    private void OnEnable()
    {
        // 初始化弹幕池
        if (fanBulletPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(fanBulletPrefab, 30);
        }
        if (iceBulletPrefab1 != null)
        {
            Global_ObjectPool.Instance.InitPool(iceBulletPrefab1, 30);
        }
        if (iceBulletPrefab2 != null)
        {
            Global_ObjectPool.Instance.InitPool(iceBulletPrefab2, 30);
        }
        // 开始射击
        StartShooting();
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
        // 取消冰点射击
        bossShootSystem.CancelIcePoint();
    }
    
    private void StartShooting()
    {
        if (bossShootSystem != null)
        {
            // 启动定位扇形射击
            bossShootSystem.Pos_FanShaped_Shoot(fanBulletPrefab, iceJadeSpeed);
            
            // 启动冰点射击（传递参数）
            bossShootSystem.IcePointAttack();

            Invoke(nameof(IcePointAttack), 2f);
            
        }
    }

    private void IcePointAttack()
    {
        bossShootSystem.IcePoint_Shoot(iceBulletPrefab1, iceBulletPrefab2, shoot_interval1, shoot_interval2, rotationSpeed, icePickSpeed, icePearlSpeed);
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
                Debug.Log("none1阶段：玩家成功讨伐Boss！");
                // 可以在这里添加讨伐成功的效果或奖励逻辑
            }
            else
            {
                Debug.Log("none1阶段：Boss仍然存活，时间到");
                // 可以在这里添加时间到的效果逻辑
            }
        }
    }
    
}

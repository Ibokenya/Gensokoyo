using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class none1 : MonoBehaviour
{
    [Header("一非参数")]
    public float shoot_interval1 = 2f; // 射击间隔1
    public float shoot_interval2 = 1f; // 射击间隔2
    public float rotationSpeed = 60f; // 旋转速度
    public GameObject fanBulletPrefab; // 扇形射击子弹预制件
    public GameObject iceBulletPrefab1; // 冰点射击预制件1
    public GameObject iceBulletPrefab2; // 冰点射击预制件2
    public BossShootSystem bossShootSystem; // 射击系统引用
    
    [Header("子弹速度")]
    public float icePickSpeed = 5f; // IcePick 速度
    public float iceJadeSpeed = 6f; // IceJade 速度
    public float icePearlSpeed = 4f; // IcePearl 速度
    
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
        }
        // 取消冰点射击
        bossShootSystem.CancelIcePoint();
    }
    
    private void StartShooting()
    {
        if (bossShootSystem != null)
        {
            // 启动定位扇形射击
            bossShootSystem.Pos_FanShaped_Shoot(fanBulletPrefab, 2f);
            
            // 启动冰点射击（传递参数）
            bossShootSystem.IcePointAttack();

            Invoke(nameof(IcePointAttack), 2f);
            
        }
    }

    private void IcePointAttack()
    {
        bossShootSystem.IcePoint_Shoot(iceBulletPrefab1, iceBulletPrefab2, 2f, 1f, 60f, icePickSpeed, iceJadeSpeed);
    }
    
    /// <summary>
    /// 一个关键的方法：检查boss是否已经死亡
    /// 如果boss没死亡，则会播放时间到的效果
    /// </summary>
    public void CheckOver()
    {

    }
    
}

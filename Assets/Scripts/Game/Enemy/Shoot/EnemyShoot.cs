using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Track;
using static Tail;
using static Remain;
using static Normal;
using static Invisible;

public class EnemyShoot : MonoBehaviour
{
    [Header("射击配置")]
    public ShootMode shootConfig;
    
    private float shootTimer = 0f;
    
    void OnEnable()
    {
        // 重置射击计时器
        shootTimer = 0f;
    }
    
    void Update()
    {
        if (shootConfig.shootMode != Shoot_Mode.none && shootConfig.bulletPrefab != null)
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootConfig.shootInterval)
            {
                shootTimer = 0f;
                // 发射子弹
                FireBullet();
            }
        }
    }
    
    /// <summary>
    /// 发射子弹
    /// </summary>
    private void FireBullet()
    {
        // 实例化子弹
        GameObject bullet = Instantiate(shootConfig.bulletPrefab, transform.position, transform.rotation);
        
        // 设置子弹参数
        SetBulletParameters(bullet);
        
        Debug.Log($"Enemy shooting with mode: {shootConfig.shootMode}");
    }
    
    /// <summary>
    /// 设置子弹参数
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    private void SetBulletParameters(GameObject bullet)
    {
        // 设置跟踪子弹参数
        Track trackBullet = bullet.GetComponent<Track>();
        if (trackBullet != null)
        {
            trackBullet.Speed = shootConfig.trackBulletConfig.Speed;
        }
        
        // 设置拖尾子弹参数
        Tail tailBullet = bullet.GetComponent<Tail>();
        if (tailBullet != null)
        {
            tailBullet.CanClone = shootConfig.tailBulletConfig.CanClone;
            tailBullet.Speed = shootConfig.tailBulletConfig.Speed;
            tailBullet.CloneSpeed = shootConfig.tailBulletConfig.CloneSpeed;
            tailBullet.attenuation = shootConfig.tailBulletConfig.attenuation;
            tailBullet.MinSpeed = shootConfig.tailBulletConfig.MinSpeed;
        }
        
        // 设置滞留子弹参数
        Remain remainBullet = bullet.GetComponent<Remain>();
        if (remainBullet != null)
        {
            remainBullet.LifeTime = shootConfig.remainBulletConfig.LifeTime;
        }
        
        // 设置普通子弹参数
        Normal normalBullet = bullet.GetComponent<Normal>();
        if (normalBullet != null)
        {
            normalBullet.Speed = shootConfig.normalBulletConfig.Speed;
        }
        
        // 设置不可见子弹参数
        Invisible invisibleBullet = bullet.GetComponent<Invisible>();
        if (invisibleBullet != null)
        {
            invisibleBullet.isVisible = shootConfig.invisibleBulletConfig.isVisible;
            invisibleBullet.Speed = shootConfig.invisibleBulletConfig.Speed;
            invisibleBullet.ShowDistance = shootConfig.invisibleBulletConfig.ShowDistance;
            invisibleBullet.ShowTime = shootConfig.invisibleBulletConfig.ShowTime;
        }
    }
    
    /// <summary>
    /// 设置射击配置
    /// </summary>
    /// <param name="config">射击配置</param>
    public void SetShootConfig(ShootMode config)
    {
        shootConfig = config;
    }
}

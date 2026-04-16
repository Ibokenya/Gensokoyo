using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShootSystem : MonoBehaviour
{
    [Header("攻击配置")]
    public GameObject bulletPrefab; // 子弹预制体
    public Transform shootPoint; // 发射点
    public float bulletSpeed = 5f; // 子弹速度
    
    [Header("攻击参数")]
    public float attackInterval = 1f; // 攻击间隔
    private float attackTimer = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 可以在这里添加自动攻击逻辑
        attackTimer += Time.deltaTime;
    }
    
    /// <summary>
    /// 基础攻击 - 单发子弹
    /// </summary>
    public void BasicAttack()
    {
        if (bulletPrefab != null && shootPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = shootPoint.up * bulletSpeed;
            }
        }
    }
    
    /// <summary>
    /// 散射攻击 - 多个方向发射子弹
    /// </summary>
    /// <param name="bulletCount">子弹数量</param>
    /// <param name="angleRange">角度范围</param>
    public void SpreadAttack(int bulletCount, float angleRange)
    {
        float angleStep = angleRange / (bulletCount - 1);
        float startAngle = -angleRange / 2f;
        
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            if (bulletPrefab != null && shootPoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation * rotation);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = rotation * shootPoint.up;
                    rb.velocity = direction * bulletSpeed;
                }
            }
        }
    }
    
    /// <summary>
    /// 环形攻击 - 360度发射子弹
    /// </summary>
    /// <param name="bulletCount">子弹数量</param>
    public void CircleAttack(int bulletCount)
    {
        float angleStep = 360f / bulletCount;
        
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            if (bulletPrefab != null && shootPoint != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, rotation);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = rotation * Vector2.up;
                    rb.velocity = direction * bulletSpeed;
                }
            }
        }
    }
    
    /// <summary>
    /// 追踪攻击 - 向玩家方向发射子弹
    /// </summary>
    public void HomingAttack()
    {
        // 假设玩家在场景中，找到玩家对象
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && bulletPrefab != null && shootPoint != null)
        {
            Vector2 direction = (player.transform.position - shootPoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * bulletSpeed;
            }
        }
    }
    
    /// <summary>
    /// 弹幕攻击 - 连续发射多波子弹
    /// </summary>
    /// <param name="waveCount">波数</param>
    /// <param name="bulletsPerWave">每波子弹数</param>
    /// <param name="waveInterval">波间隔</param>
    public IEnumerator BarrageAttack(int waveCount, int bulletsPerWave, float waveInterval)
    {
        for (int wave = 0; wave < waveCount; wave++)
        {
            // 每波发射bulletsPerWave发子弹
            for (int i = 0; i < bulletsPerWave; i++)
            {
                BasicAttack();
                yield return new WaitForSeconds(0.1f);
            }
            
            // 波间隔
            yield return new WaitForSeconds(waveInterval);
        }
    }
}

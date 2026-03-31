using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 激光类
/// 魔理沙使用激光作为高速副手攻击
/// 总额感觉我这激光没有那么炫酷
/// 之后可能的话，再给激光加一个闪烁的特效
/// 或者子机口处弄点粒子
/// 但现在这样就算可以了
/// 之后做射线碰撞也是头疼的事情
/// </summary>
public class Laser : MonoBehaviour
{
    [Header("激光配置")]
    public float MaxLength = 20f; // 激光最大长度
    public float LaserWidth = 0.3f; // 激光宽度
    public int damage = 1; // 激光伤害(每帧)
    public LayerMask HitLayer;// 激光可攻击目标层

    public List<Sprite> LaserSprites = new();// 激光精灵列表
    public int AnimeSpeed = 4;
    private int CurrentIndex = 0;// 当前精灵索引

    private Vector2 EndPos;// 激光结束位置
    private Transform firePoint; // 发射点引用

    private SpriteRenderer spriteRenderer;// 激光渲染器
    private bool isActive = false;// 激光是否激活

    private float TimeClock = 0f;// 动画时钟

    void Awake()
    {
        // 确保获取到 SpriteRenderer 组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on Laser object!");
        }
        else
        {
            spriteRenderer.enabled = false;
            transform.localScale = new Vector3(1f, LaserWidth, 1f);
        }
    }

    // 设置发射点
    public void SetFirePoint(Transform firePointTransform)
    {
        firePoint = firePointTransform;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isActive || spriteRenderer == null || firePoint == null)
        {
            return;
        }
        
        // 更新激光位置为发射点位置
        transform.position = firePoint.position;
        
        // 射线检测方向为发射点的上方向（默认向上）
        UpdateLaserEndPos();
        UpdateLaserVisual();
        UpdateLaserAnime();
        UpdateLaserDamage();
    }
    
    /// <summary>
    /// 更新激光射线检测确定终点
    /// </summary>
    private void UpdateLaserEndPos()
    {
        // 激光终点始终设置为最大长度处
        EndPos = (Vector2)transform.position + (Vector2)firePoint.up * MaxLength;
    }

    /// <summary>
    /// 更新激光视觉效果（粗细，旋转等）
    /// </summary>
    private void UpdateLaserVisual()
    {
        if (spriteRenderer == null || firePoint == null)
            return;
            
        Vector2 LaserDir = EndPos - (Vector2)transform.position;
        float LaserLength = LaserDir.magnitude;
        
        // 确保激光长度不为0
        if (LaserLength > 0)
        {
            // 计算激光方向角度
            float angle = Mathf.Atan2(LaserDir.y, LaserDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // 缩放：x轴为激光长度，y轴为激光宽度
            transform.localScale = new Vector3(LaserLength, LaserWidth, 1f);
        }
    }

    /// <summary>
    /// 激光持续伤害判定
    /// </summary>
    private void UpdateLaserDamage()
    {
        // 使用RaycastAll检测路径上的所有物体
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, firePoint.up, MaxLength, HitLayer);
        
        // 对所有命中的物体造成伤害
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // 这里可以添加伤害逻辑
            }
        }
    }

    /// <summary>
    /// 激活激光
    /// </summary>
    public void ActivateLaser()
    {
        isActive = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    /// <summary>
    /// 停用激光
    /// </summary>
    public void StopLaser()
    {
        isActive = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    /// <summary>
    /// 更新激光动画
    /// </summary>
    private void UpdateLaserAnime()
    {
        if (LaserSprites.Count == 0)
            return;
            
        TimeClock += Time.deltaTime;
        if (TimeClock >= 1f / AnimeSpeed)
        {
            TimeClock -= 1f / AnimeSpeed;
            CurrentIndex++;
            if (CurrentIndex >= LaserSprites.Count)
                CurrentIndex = 0;
                
            spriteRenderer.sprite = LaserSprites[CurrentIndex];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            // 伤害敌人
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Damage(damage);
            }
        }
    }
}

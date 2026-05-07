using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 暗影弹脚本
/// 负责暗影弹的移动、碰撞和回收逻辑
/// </summary>
public class DarkFO : MonoBehaviour
{
    [Header("伤害属性")]
    public int damage = 10; // 原始伤害值
    private int currentDamage = 10; // 当前伤害值
    
    [Header("移动属性")]
    public float moveSpeed = 3f; // 原始移动速度
    private float currentSpeed = 3f; // 当前移动速度
    public float absorbDistance = 0.5f; // 吸收距离阈值
    
    [Header("状态")]
    public bool isAbsorbed = false; // 是否被吸收
    public bool isEnhanced = false; // 是否强化
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector2 moveDirection;
    private Transform blackHoleTransform;
    private Vector2 outerRectBottomLeft;
    private Vector2 outerRectTopRight;
    private float fadeTimer = 0f;
    private bool isFading = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = new Color(1f, 1f, 1f, 1f);
        currentDamage = damage;
        currentSpeed = moveSpeed;
    }
    
    void OnEnable()
    {
        // 重置所有属性
        ResetAllProperties();
    }
    
    void Update()
    {
        // 检查是否超出最大矩形范围
        CheckOutOfBounds();
        
        // 淡入逻辑
        if (isFading)
        {
            UpdateFade();
        }
    }
    
    void FixedUpdate()
    {
        if (!isFading)
        {
            // 移动逻辑
            MoveTowardsTarget();
            
            // 检查是否接近黑洞
            CheckAbsorbDistance();
        }
    }
    
    /// <summary>
    /// 重置所有属性
    /// </summary>
    private void ResetAllProperties()
    {
        // 重置状态
        isAbsorbed = false;
        isEnhanced = false;
        isFading = false;
        fadeTimer = 0f;
        
        // 重置属性
        currentDamage = damage;
        currentSpeed = moveSpeed;
        
        // 重置颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // 重置速度
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 初始化暗影弹
    /// </summary>
    /// <param name="blackHole">黑洞位置</param>
    /// <param name="outerBottomLeft">最大矩形左下角</param>
    /// <param name="outerTopRight">最大矩形右上角</param>
    public void Initialize(Transform blackHole, Vector2 outerBottomLeft, Vector2 outerTopRight)
    {
        blackHoleTransform = blackHole;
        outerRectBottomLeft = outerBottomLeft;
        outerRectTopRight = outerTopRight;
        
        // 计算朝向黑洞的方向
        if (blackHoleTransform != null)
        {
            moveDirection = (blackHoleTransform.position - transform.position).normalized;
            
            // 设置朝向（让暗影弹朝向黑洞）
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
    
    /// <summary>
    /// 强化后重新发射
    /// </summary>
    /// <param name="blackHole">黑洞位置</param>
    public void ReLaunch(Transform blackHole)
    {
        blackHoleTransform = blackHole;
        
        // 从黑洞位置发射
        transform.position = blackHoleTransform.position;
        
        // 随机方向
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        
        // 设置朝向（沿着发射方向）
        transform.rotation = Quaternion.Euler(0f, 0f, randomAngle);
        
        // 强化属性
        isEnhanced = true;
        currentDamage = (int)(currentDamage * 1.5f);
        currentSpeed *= 3;
        
        // 设置颜色为红色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0f, 0f, 1f);
        }
    }
    
    /// <summary>
    /// 移动逻辑
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (rb != null)
        {
            rb.velocity = moveDirection * currentSpeed;
        }
    }
    
    /// <summary>
    /// 检查是否接近黑洞
    /// </summary>
    private void CheckAbsorbDistance()
    {
        if (blackHoleTransform == null || isAbsorbed || isEnhanced)
        {
            return;
        }
        
        float distance = Vector2.Distance(transform.position, blackHoleTransform.position);
        
        if (distance <= absorbDistance)
        {
            // 开始淡入
            isAbsorbed = true;
            isFading = true;
            fadeTimer = 0f;
            
            // 停止移动
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }
    
    /// <summary>
    /// 更新淡入效果
    /// </summary>
    private void UpdateFade()
    {
        fadeTimer += Time.deltaTime;
        float fadeProgress = fadeTimer / 0.5f; // 0.5秒淡入
        
        if (fadeProgress >= 1f)
        {
            // 淡入完成，透明度为0
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0f;
                spriteRenderer.color = color;
            }
            
            isFading = false;
            
            // 强化并重新发射
            ReLaunch(blackHoleTransform);
        }
        else
        {
            // 淡入中
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f - fadeProgress;
                spriteRenderer.color = color;
            }
        }
    }
    
    /// <summary>
    /// 检查是否超出最大矩形范围
    /// </summary>
    private void CheckOutOfBounds()
    {
        Vector2 currentPos = transform.position;
        
        // 检查是否超出范围
        if (currentPos.x < outerRectBottomLeft.x || currentPos.x > outerRectTopRight.x ||
            currentPos.y < outerRectBottomLeft.y || currentPos.y > outerRectTopRight.y)
        {
            // 超出范围，回收
            Recycle();
        }
    }
    
    /// <summary>
    /// 碰撞检测
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Enemy":
                var enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Damage(currentDamage);
                }
                break;
            case "Boss":
                var boss = collision.GetComponent<BossBase>();
                if (boss != null)
                {
                    boss.TakeDamage(currentDamage);
                }
                break;
            case "FrozenIce":
                var frozenIce = collision.GetComponent<FrozenIce>();
                if (frozenIce != null)
                {
                    frozenIce.TakeDamage(currentDamage);
                }
                break;
            case "FrozenBall":
                var frozenBall = collision.GetComponent<FrozenBall>();
                if (frozenBall != null)
                {
                    frozenBall.TakeDamage(currentDamage);
                }
                break;
            case "MiniBall":
                var miniBall = collision.GetComponent<miniIceBall>();
                if (miniBall != null)
                {
                    miniBall.TakeDamage(currentDamage);
                }
                break;
            default:
                break;
        }
        Recycle();
    }
    
    /// <summary>
    /// 回收暗影弹
    /// </summary>
    public void Recycle()
    {
        // 重置所有属性
        ResetAllProperties();
        
        // 使用Global_ObjectPool回收
        Global_ObjectPool.Instance.Recycle(gameObject);
    }
}

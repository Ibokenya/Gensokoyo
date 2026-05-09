using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class miniIceBall : MonoBehaviour
{
    public float moveSpeed = 5f; // 移动速度
    public float TurnInterval = 2f; // 转向间隔
    public Vector3 TargetPosition; // 目标坐标
    public int maxHP = 50; // 生命值
    public int hp;
    public bool isMini = true; // 是否为mini态（是否有折返）
    
    private Rigidbody2D rb2D;
    private float timer = 0f; // 计时器
    private bool hasTurned = false; // 是否已转向
    private bool isFused = false; // 是否已融合
    
    // 边界范围
    private readonly float minX = -11f;
    private readonly float maxX = 5f;
    private readonly float minY = -7.5f;
    private readonly float maxY = 6.5f;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D != null && isMini)
        {
            // 只有mini态的子弹才设置初始向上的速度
            Vector2 direction = transform.up;
            rb2D.velocity = direction * moveSpeed;
        }
    }
    
    void OnEnable()
    {
        // 当子弹从对象池取出时，确保rb2D引用存在
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        
        // 只有mini态的子弹才设置初始向上的速度
        if (rb2D != null && isMini)
        {
            Vector2 direction = transform.up;
            rb2D.velocity = direction * moveSpeed;
        }
    }

    void Update()
    {
        if (!isFused)
        {
            // 检查边界
            CheckBounds();
            
            // 只有mini态的子弹才有折返行为
            if (isMini && !hasTurned)
            {
                timer += Time.deltaTime;
                if (timer >= TurnInterval)
                {
                    TurnToTarget();
                    hasTurned = true;
                }
            }
        }
    }
    
    /// <summary>
    /// 转向目标位置
    /// </summary>
    private void TurnToTarget()
    {
        if (rb2D != null)
        {
            Vector2 direction = (TargetPosition - transform.position).normalized;
            // 旋转子弹朝向目标
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            // 设置速度，保持原速飞回
            rb2D.velocity = direction * moveSpeed;
        }
    }
    
    /// <summary>
    /// 检查边界，超出范围则回收
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = transform.position;
        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
        {
            Recycle();
        }
    }
    
    /// <summary>
    /// 受伤方法
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Recycle();
        }
    }
    
    /// <summary>
    /// 回收子弹
    /// </summary>
    public void Recycle()
    {
        if (Global_ObjectPool.Instance != null)
        {
            Global_ObjectPool.Instance.Recycle(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 向指定方向发射
    /// </summary>
    /// <param name="direction">方向向量</param>
    public void FireInDirection(Vector2 direction)
    {
        if (rb2D != null)
        {
            // 归一化方向向量
            direction = direction.normalized;
            // 自机狙
            rb2D.velocity = direction * moveSpeed;
            isFused = false;
        }
    }

    void OnDisable()
    {
        // 重置参数
        hasTurned = false;
        isFused = false;
        isMini = true; // 重置为mini态
        timer = 0f;
        hp = maxHP; // 重置HP为默认值
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
    }
}

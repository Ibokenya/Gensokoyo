using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 追踪飞行物（Tracked Flying Object）
/// 追踪距离最近的敌人
/// </summary>
public class TrackedFO : MonoBehaviour
{
    [Header("追踪配置")]
    public float TrackSpeed = 5f; // 追踪弹飞行速度
    public float TurnSpeed = 10f; // 转向速度

    public int damage = 10;// 伤害值
    private readonly float minX = -8.4f;
    private readonly float maxX = 3f;
    private readonly float minY = -5.3f;
    private readonly float maxY = 4.5f;
    private GameObject target; // 目标
    private Rigidbody2D rb2D;
    private int scanFrameCounter = 0; // 扫描帧计数器
    private const int SCAN_INTERVAL = 10; // 每10帧扫描一次目标

    void OnEnable()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("一个追踪飞行物未找到刚体");
        }
        
        // 初始化帧计数器
        scanFrameCounter = 0;
        
        // 寻找初始目标
        FindTarget();
        
        // 初始化速度
        if (rb2D != null)
        {
            // 初始默认向上飞行
            rb2D.velocity = TrackSpeed * Vector2.up;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 每SCAN_INTERVAL帧扫描一次目标
        scanFrameCounter++;
        if (scanFrameCounter >= SCAN_INTERVAL)
        {
            scanFrameCounter = 0;
            CheckAndFindTarget();
        }
        
        TrackedMove();
        MoveCheck();
    }

    /// <summary>
    /// 检查目标是否存在，如果不存在则重新寻找
    /// </summary>
    private void CheckAndFindTarget()
    {
        FindTarget();
    }

    /// <summary>
    /// 寻找距离最近的敌人作为目标
    /// </summary>
    private void FindTarget()
    {
        GameObject closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        // 遍历敌人列表
        if (Global_GameManager.Instance != null)
        {
            foreach (GameObject enemy in Global_GameManager.Instance.EnemyList)
            {
                if (enemy != null && enemy.activeSelf)
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
        }
        
        // 更新目标
        target = closestEnemy;
    }

    /// <summary>
    /// 追踪飞行物的移动
    /// </summary>
    public void TrackedMove()
    {
        if (rb2D != null)
        {
            // 如果有目标，平滑转向目标
            if (target != null && target.activeSelf)
            {
                // 计算目标方向
                Vector2 targetDirection = (target.transform.position - transform.position).normalized;
                
                // 计算当前速度方向
                Vector2 currentDirection = rb2D.velocity.normalized;
                
                // 平滑转向
                Vector2 newDirection = Vector2.Lerp(currentDirection, targetDirection, Time.deltaTime * TurnSpeed);
                
                // 应用新速度
                rb2D.velocity = newDirection * TrackSpeed;
            }
            else if (rb2D.velocity == Vector2.zero)
            {
                // 没有目标且速度为0时，默认向上飞行
                rb2D.velocity = TrackSpeed * Vector2.up;
            }
        }
    }

    /// <summary>
    /// 边界检查
    /// </summary>
    public void MoveCheck()
    {
        if(transform.position.x<minX || transform.position.x>maxX || transform.position.y<minY || transform.position.y>maxY)
        {
            Global_ObjectPool.Instance.Recycle(this.gameObject);
        }
    }

    /// <summary>
    /// 碰撞检测
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 只与敌人碰撞
        if (collision.CompareTag("Enemy"))
        {
            Global_ObjectPool.Instance.Recycle(this.gameObject);
            // 伤害敌人
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Damage(damage);
            }
        }
    }
}

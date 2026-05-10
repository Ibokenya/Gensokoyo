using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一般飞行物（Normal Flying Object）
/// 啊，这就是NFO!
/// </summary>
public class NormalFO : MonoBehaviour
{
    public float speed;
    public int damage = 10;// 伤害值
private readonly float minX = -9.5f;
    private readonly float maxX = 3.5f;
    private readonly float minY = -5.5f;
    private readonly float maxY = 5.5f;
    public bool isNeedle = false;
    private Rigidbody2D rb2D;

    void OnEnable()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("一个一般飞行物未找到刚体");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move(speed);
        MoveCheck();
    }

    /// <summary>
    /// 普通飞行物的移动
    /// </summary>
    /// <param name="speed">移动速度</param>
    public void Move(float speed)
    {
        if (rb2D != null)
        {
            rb2D.velocity = speed * Vector2.up;
        }
    }

    public void MoveCheck()
    {
        if(transform.position.x<minX || transform.position.x>maxX || transform.position.y<minY || transform.position.y>maxY)
        {
            Global_ObjectPool.Instance.Recycle(this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Enemy":
                var enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Damage(damage);
                }
                break;
            case "Boss":
                var boss = collision.GetComponent<BossBase>();
                if (boss != null)
                {
                    boss.TakeDamage(damage);
                }
                break;
            case "FrozenIce":
                    var frozenIce = collision.GetComponent<FrozenIce>();
                    if (frozenIce != null)
                    {
                        frozenIce.TakeDamage(damage);
                    }
                break;
            case "FrozenBall":
                var frozenBall = collision.GetComponent<FrozenBall>();
                if (frozenBall != null)
                {
                    frozenBall.TakeDamage(damage);
                }
                break;
            case "MiniBall":
                var miniBall = collision.GetComponent<miniIceBall>();
                if (miniBall != null)
                {
                    miniBall.TakeDamage(damage);
                }
                break;
            default:
                break;
        }
        Global_GameManager.Instance.AddScore(3);
        if(!isNeedle)
        {
            Global_ObjectPool.Instance.Recycle(this.gameObject);
        }
    }
}

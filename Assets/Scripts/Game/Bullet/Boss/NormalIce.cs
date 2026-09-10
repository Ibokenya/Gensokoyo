using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySystem;

public class NormalIce : MonoBehaviour
{
    public float BaseSpeed = 5f;
    public bool useSpeedOffset = true; // 是否使用速度偏移
    private float actualSpeed;
    private bool speedInitialized = false; // 🔴 SetSpeed 或 FixedUpdate 初始化后 = true
    private Rigidbody2D rb2D;
    public BossShootSystem bossShootSystem; // Boss射击系统引用
    
    // 边界范围
    public float minX = -11f;
    public float maxX = 5f;
    public float minY = -7.5f;
    public float maxY = 6.5f;
    
    void Start()
    {
        // 获取Rigidbody2D组件
        rb2D = GetComponent<Rigidbody2D>();
    }
    
    /// <summary>
    /// 🔴 由 BossShootSystem spawner 层调用 —— 确定性消费 GameRNG + 设好速度
    /// 顺序调用，GameRNG 消费顺序 100% 确定
    /// </summary>
    public void SetSpeed(float speed)
    {
        BaseSpeed = speed;
        InitSpeedFromRng();
        ApplyVelocity();
    }

    /// <summary>🔴 从 GameRNG 计算实际速度 —— 只在 spawner 层或 FixedUpdate 第一帧调用一次</summary>
    private void InitSpeedFromRng()
    {
        if (useSpeedOffset)
        {
            float speedOffset = Mathf.Round(GameRNG.Range(-5f, 6f)) * 0.1f;
            actualSpeed = BaseSpeed + speedOffset;
        }
        else
        {
            actualSpeed = BaseSpeed;
        }
        speedInitialized = true;
    }

    /// <summary>🔴 把 actualSpeed 应用到 Rigidbody2D（spawner 和 FixedUpdate 都复用）</summary>
    private void ApplyVelocity()
    {
        if (rb2D != null)
        {
            Vector2 direction = transform.TransformDirection(Vector2.right);
            rb2D.velocity = direction * actualSpeed;
            rb2D.isKinematic = false;
        }
    }
    
    void OnEnable()
    {
        // 获取Rigidbody2D组件（OnEnable 可能在 Start 之前触发）
        if (rb2D == null) rb2D = GetComponent<Rigidbody2D>();
        speedInitialized = false; // 🔴 重置 —— SetSpeed 或 FixedUpdate 会处理
    }

    void FixedUpdate()
    {
        // 🔴 如果 spawner (BossShootSystem) 没调 SetSpeed（speed <= 0 的兜底路径）
        // 才在 FixedUpdate 第一帧消费 GameRNG
        if (!speedInitialized)
        {
            InitSpeedFromRng(); // spawner 层顺序调用时不会走到这里
            ApplyVelocity();
        }
        CheckBounds();
    }
    
    /// <summary>
    /// 检查边界，超出范围则回收
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = transform.position;
        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
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
    }
    
    void OnDisable()
    {
        // 从BossShootSystem的activeIcePearls列表中移除自己
        if (bossShootSystem != null)
        {
            bossShootSystem.RemoveIcePearl(this.gameObject);
        }
        
        // 重置参数
        bossShootSystem = null;
    }
}

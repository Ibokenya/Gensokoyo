using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 雪花型子弹
/// </summary>
public class SnowFlake : MonoBehaviour
{
    public float rotationSpeed = 30f; // 旋转速度（度/秒）
    public float moveSpeed = 2f; // 基础移动速度
    public Vector2 baseDirection = Vector2.down; // 基础移动方向（归一化向量）
    public float noiseIntensity = 0.5f; // 噪声强度，控制飘移程度
    public float noiseFrequency = 0.5f; // 噪声频率，控制飘移变化速度
    public float noiseTimeScale = 0.1f; // 噪声时间缩放，控制飘移时间变化
    public List<Sprite> snowFlakeSprites = new List<Sprite>(); // 雪花精灵列表
    
    // 边界范围
    private readonly float minX = -12.1f;
    private readonly float maxX = 6.1f;
    private readonly float minY = -6.1f;
    private readonly float maxY = 6.1f;
    
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float timeCounter;

    void OnEnable()
    {
        // 获取或添加SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 随机选择一种精灵
        if (snowFlakeSprites != null && snowFlakeSprites.Count > 0)
        {
            int randomIndex = Random.Range(0, snowFlakeSprites.Count);
            spriteRenderer.sprite = snowFlakeSprites[randomIndex];
        }
        
        // 初始化噪声偏移值，使每个雪花的飘移轨迹不同
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
        timeCounter = 0f;
    }

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 旋转雪花
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // 计算Perlin噪声，生成无序飘移
        timeCounter += Time.deltaTime * noiseTimeScale;
        float noiseX = Mathf.PerlinNoise(noiseOffsetX + timeCounter, 0f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0f, noiseOffsetY + timeCounter) * 2f - 1f;
        
        // 生成飘移向量
        Vector2 drift = new Vector2(noiseX, noiseY) * noiseIntensity;
        
        // 计算最终移动方向（基础方向 + 飘移）
        Vector2 finalDirection = (baseDirection + drift).normalized;
        
        // 设置速度
        if (rb2D != null)
        {
            rb2D.velocity = finalDirection * moveSpeed;
        }
        
        // 检查边界，超出范围则回收
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
            Recycle();
        }
    }
    
    /// <summary>
    /// 回收子弹
    /// </summary>
    private void Recycle()
    {
        // 解除父子关系
        transform.parent = null;
        
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
    /// 设置基础移动方向
    /// </summary>
    /// <param name="direction">归一化的方向向量</param>
    public void SetDirection(Vector2 direction)
    {
        baseDirection = direction.normalized;
    }
    
    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="speed">移动速度</param>
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    void OnDisable()
    {
        // 清理速度
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCloud : MonoBehaviour
{
    [Header("冰云参数")]
    public float fadeInTime = 1f; // 淡入时间
    public float fadeOutTime = 1f; // 淡出时间
    public float floatSpeed = 0.5f; // 飘浮速度
    public float perlinSpeed = 0.1f; // 噪声变化速度
    
    private SpriteRenderer spriteRenderer;
    private Collider2D cloudCollider;
    private Rigidbody2D rb2D;
    private bool isFadingOut = false;
    private float fadeTimer = 0f;
    private float perlinOffsetX;
    private float perlinOffsetY;
    public BossShootSystem bossShootSystem;
    
    // 边界检测范围（与stone相同）
    private readonly float minX = -12f;
    private readonly float maxX = 6f;
    private readonly float minY = -7f;
    private readonly float maxY = 7f;
    private bool isBecauseBounds = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cloudCollider = GetComponent<Collider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        
        // 初始化噪声偏移
        perlinOffsetX = Random.Range(0f, 1000f);
        perlinOffsetY = Random.Range(0f, 1000f);
        
        // 初始状态：透明，碰撞器禁用
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
        if (cloudCollider != null)
        {
            cloudCollider.enabled = false;
        }
    }
    
    private void OnEnable()
    {
        // 开始淡入
        StartCoroutine(FadeIn());
    }
    
    private void Update()
    {
        // 随机飘浮
        if (!isFadingOut)
        {
            FloatRandomly();
        }
        
        // 边界检测
        CheckBounds();
    }
    
    /// <summary>
    /// 随机飘浮
    /// </summary>
    private void FloatRandomly()
    {
        perlinOffsetX += perlinSpeed * Time.deltaTime;
        perlinOffsetY += perlinSpeed * Time.deltaTime;
        
        // 使用Perlin噪声生成随机方向
        float noiseX = Mathf.PerlinNoise(perlinOffsetX, 0f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0f, perlinOffsetY) * 2f - 1f;
        
        // 生成随机移动向量
        Vector2 randomDirection = new Vector2(noiseX, noiseY).normalized;
        
        // 使用刚体移动
        if (rb2D != null)
        {
            rb2D.velocity = randomDirection * floatSpeed;
        }
    }
    
    /// <summary>
    /// 边界检测
    /// </summary>
    private void CheckBounds()
    {
        Vector3 position = transform.position;
        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
        {
            isBecauseBounds = true;
            // 超出边界，回收
            Recycle();
        }
    }
    
    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeIn()
    {
        fadeTimer = 0f;
        
        while (fadeTimer < fadeInTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.2f, fadeTimer / fadeInTime);
            
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        // 淡入完成，激活碰撞器
        if (cloudCollider != null)
        {
            cloudCollider.enabled = true;
        }
    }
    
    /// <summary>
    /// 回收冰云
    /// </summary>
    public void Recycle()
    {
        Global_ObjectPool.Instance.Recycle(this.gameObject);
        // 通知BossShootSystem生成新的冰云
        if (bossShootSystem != null && !isBecauseBounds)
        {
            bossShootSystem.OnCloudRecycled();
        }
    }
    
    private void OnDisable()
    {
        isBecauseBounds = false;
        // 清理速度
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;
    }
}

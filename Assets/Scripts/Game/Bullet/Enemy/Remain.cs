using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 滞留子弹Remain
/// 留在场上不会移动，等到了生命周期后消失
/// 会旋转
/// 小星星，大星星
/// </summary>
public class Remain : MonoBehaviour
{
    public float LifeTime = 5f;// 生存时间
    public float Speed = 0f;// 移动速度
    public float WindUp = 0f;// 前摇时间（秒）
    public List<Sprite> spriteVariants = new List<Sprite>(); // 子弹颜色变体
    private float lifeTimer = 0f;
    private float windUpTimer = 0f;
    private bool isFading = false;
    private bool isWindingUp = true;
    private float fadeTimer = 0f;
    private const float fadeDuration = 1f;
    public SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb2D;
    
    void Start()
    {
        // 确保组件存在
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 获取原始颜色
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void OnEnable()
    {
        Debug.Log($"Remain bullet enabled: {gameObject.name}, Speed={Speed}, WindUp={WindUp}, LifeTime={LifeTime}");
        
        // 确保组件存在
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
            Debug.Log($"Got Rigidbody2D: {rb2D != null}");
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            Debug.Log($"Got SpriteRenderer: {spriteRenderer != null}");
        }
        
        // 初始化前摇状态
        windUpTimer = 0f;
        isWindingUp = true;
        lifeTimer = 0f;
        isFading = false;
        fadeTimer = 0f;
        
        // 随机选择一个sprite变体
        if (spriteRenderer != null && spriteVariants.Count > 0)
        {
            int randomIndex = Random.Range(0, spriteVariants.Count);
            spriteRenderer.sprite = spriteVariants[randomIndex];
            Debug.Log($"Set sprite variant: {randomIndex}");
        }
        
        // 重置颜色
        if (spriteRenderer != null)
        {
            // 确保originalColor有值
            if (originalColor.a == 0)
            {
                originalColor = Color.white;
            }
            spriteRenderer.color = originalColor;
        }
        
        // 设置初始速度（直线移动）
        if (rb2D != null)
        {
            Vector2 direction = transform.up;
            rb2D.velocity = direction * Speed;
            rb2D.isKinematic = false;
        }
        
        // 确保物体是激活状态
        gameObject.SetActive(true);
    }
    
    // 旋转速度（固有参数）
    private readonly float rotationSpeed = 180f; // 每秒旋转180度
    
    void Update()
    {
        // 自旋转动画
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // 前摇逻辑
        if (isWindingUp)
        {
            windUpTimer += Time.deltaTime;
            if (windUpTimer >= WindUp)
            {
                // 前摇结束，停止移动
                isWindingUp = false;
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                    rb2D.isKinematic = true;
                }
            }
        }
        else if (!isFading)
        {
            // 计时
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= LifeTime)
            {
                // 开始淡出
                isFading = true;
                fadeTimer = 0f;
            }
        }
        else
        {
            // 淡出效果
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (fadeTimer / fadeDuration));
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }
            
            // 淡出完成，回收子弹
            if (alpha <= 0f)
            {
                RecycleBullet();
            }
        }
    }
    
    /// <summary>
    /// 回收子弹
    /// </summary>
    private void RecycleBullet()
    {
        // 重置状态
        ResetState();
        
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
    /// 重置状态
    /// </summary>
    private void ResetState()
    {
        lifeTimer = 0f;
        isFading = false;
        fadeTimer = 0f;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.isKinematic = true;
        }
    }
    
    /// <summary>
    /// 碰撞检测
    /// </summary>
    /// <param name="other">碰撞对象</param>
    private void OnTriggerEnter2D(Collider2D other)
    {

    }
}

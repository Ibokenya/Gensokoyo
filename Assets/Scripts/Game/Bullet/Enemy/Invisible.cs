using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 不可见子弹Invisible
/// 当靠近自机时才会显现
/// 中玉，大玉
/// </summary>
public class Invisible : MonoBehaviour
{
    public bool isVisible = true;
    public float Speed = 5f;
    public float ShowDistance = 1.2f;// 靠近显形距离
    public float ShowTime = 1f;// 淡入时间
    public List<Sprite> spriteVariants = new List<Sprite>(); // 子弹颜色变体
    private GameObject player;
    
    private bool isShowing = false;
    private float showTimer = 0f;
    public SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Rigidbody2D rb2D;
    private float distance;
    
    // 边界范围
    private readonly float minX = -11f;
    private readonly float maxX = 5f;
    private readonly float minY = -7.5f;
    private readonly float maxY = 6.5f;
    
    void Start()
    {
        // 获取精灵渲染器
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        
        // 如果没有玩家对象，尝试获取
        if (player == null)
        {
            Debug.LogError("隐形弹玩家对象未设置");
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }
    
    void OnEnable()
    {
        // 确保刚体组件存在
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        
        // 确保精灵渲染器存在
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        isVisible = false;
        isShowing = false;
        
        // 随机选择一个sprite变体
        if (spriteRenderer != null && spriteVariants.Count > 0)
        {
            int randomIndex = Random.Range(0, spriteVariants.Count);
            spriteRenderer.sprite = spriteVariants[randomIndex];
        }
        
        // 初始时不可见
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0f);
        }
        
        // 设置刚体速度
        if (rb2D != null)
        {
            Vector2 direction = transform.up;
            rb2D.velocity = direction * Speed;
            // 确保刚体不是运动学的
            rb2D.isKinematic = false;
        }
    }
    
    void Update()
    {
        // 检查是否需要显现
        if (!isVisible && !isShowing && player != null)
        {
            distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= ShowDistance)
            {
                // 开始显现
                isShowing = true;
                showTimer = 0f;
            }
        }
        
        // 淡入效果
        if (isShowing)
        {
            showTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(showTimer / ShowTime);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1, 1, 1, alpha);
            }
            
            // 淡入完成
            if (alpha >= 1f)
            {
                isShowing = false;
                isVisible = true;
            }
        }
        
        // 边界检测
        CheckBounds();
    }
    
    /// <summary>
    /// 检查边界，超出边界则回收
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = transform.position;
        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
        {
            // 超出边界，回收子弹
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
    
    /// <summary>
    /// 碰撞检测
    /// </summary>
    /// <param name="other">碰撞对象</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 普通子弹Normal
/// 可以通过设置移速的方式实现快慢效果
/// 小玉，持国天，小米弹，大米弹
/// </summary>
public class Normal : MonoBehaviour
{
    public float Speed = 5f;
    public List<Sprite> spriteVariants = new List<Sprite>(); // 子弹颜色变体
    private Rigidbody2D rb2D;
    public SpriteRenderer spriteRenderer;
    
    // 边界范围
    private readonly float minX = -11f;
    private readonly float maxX = 5f;
    private readonly float minY = -7.5f;
    private readonly float maxY = 6.5f;
    
    void Start()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        // 获取精灵渲染器
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        
        // 设置刚体速度
        if (rb2D != null)
        {
            Vector2 direction = transform.up;
            rb2D.velocity = direction * Speed;
            // 确保刚体不是运动学的
            rb2D.isKinematic = false;
        }
        
        // 随机选择一个sprite变体
        if (spriteRenderer != null && spriteVariants.Count > 0)
        {
            int randomIndex = Random.Range(0, spriteVariants.Count);
            spriteRenderer.sprite = spriteVariants[randomIndex];
        }
    }
    
    void Update()
    {
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
}

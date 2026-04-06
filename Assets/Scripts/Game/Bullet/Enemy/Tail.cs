using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拖尾弹Tail
/// 移动时会不断以更低的速度复制自己
/// 姬虫百百世同款
/// 每次复制令速度-1直到为0
/// 子弹
/// </summary>
public class Tail : MonoBehaviour
{
    public bool CanClone = true;// 是否可以自我复制（仅母弹可以）
    public float Speed = 3f;// 移动速度
    public float CloneSpeed = 0.2f;// 复制间隔
    public float attenuation = 0.5f;// 衰减系数
    public float MinSpeed = 1f;// 最小速度(小于该值将不再复制)
    public List<Sprite> spriteVariants = new List<Sprite>(); // 子弹颜色变体
    private float cloneTimer = 0f;
    private float initialSpeed;// 母弹初始速度
    private int cloneCount = 0;// 复制次数
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
        
        // 记录初始速度
        initialSpeed = Speed;
        
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
        // 如果可以复制，计时并创建克隆体
        if (CanClone)
        {
            cloneTimer += Time.deltaTime;
            if (cloneTimer >= CloneSpeed)
            {
                cloneTimer = 0f;
                CreateClone();
            }
        }
        
        // 边界检测
        CheckBounds();
    }
    
    /// <summary>
    /// 创建克隆体
    /// </summary>
    private void CreateClone()
    {
        // 计算克隆体的速度
        cloneCount++;
        float cloneSpeed = initialSpeed - (cloneCount * attenuation);
        
        // 如果克隆体速度小于等于最小速度，不再创建
        if (cloneSpeed <= MinSpeed)
        {
            CanClone = false;
            return;
        }
        
        // 创建克隆体
        if (Global_ObjectPool.Instance != null)
        {
            GameObject clone = Global_ObjectPool.Instance.GetObject(gameObject, transform.position, transform.rotation);
            Tail cloneTail = clone.GetComponent<Tail>();
            
            if (cloneTail != null)
            {
                // 设置克隆体参数
                cloneTail.CanClone = false; // 克隆体不可复制
                cloneTail.Speed = cloneSpeed; // 克隆体速度
                cloneTail.CloneSpeed = CloneSpeed;
                cloneTail.attenuation = attenuation;
                cloneTail.MinSpeed = MinSpeed;
            }
            else
            {
                Debug.LogError("创建克隆体失败，组件未找到");
            }
        }
        else
        {
            // 后备方案：直接实例化
            GameObject clone = Instantiate(gameObject, transform.position, transform.rotation);
            Tail cloneTail = clone.GetComponent<Tail>();
            
            if (cloneTail != null)
            {
                // 设置克隆体参数
                cloneTail.CanClone = false; // 克隆体不可复制
                cloneTail.Speed = cloneSpeed; // 克隆体速度
                cloneTail.CloneSpeed = CloneSpeed;
                cloneTail.attenuation = attenuation;
                cloneTail.MinSpeed = MinSpeed;
            }
            else
            {
                Debug.LogError("创建克隆体失败，组件未找到");
            }
        }
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
}

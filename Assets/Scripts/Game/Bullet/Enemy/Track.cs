using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 跟踪子弹Track
/// 移动时会根据目标机的位置进行移动（仅追踪一次）
/// 箭头弹，符箓弹，苦无弹
/// </summary>
public class Track : MonoBehaviour
{
    public float Speed = 5f;// 移动速度
    public float WindUp = 0f;// 前摇时间（秒）
    public List<Sprite> spriteVariants = new List<Sprite>(); // 子弹颜色变体
    private float windUpTimer = 0f;
    private bool isWindingUp = true;
    private GameObject Target;// 目标机对象
    private Vector2 targetPosition;
    private bool hasTarget = false;
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
        
        // 初始化前摇状态
        windUpTimer = 0f;
        isWindingUp = true;
        
        // 设置初始速度（直线移动）
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
        // 前摇逻辑
        if (isWindingUp)
        {
            windUpTimer += Time.deltaTime;
            if (windUpTimer >= WindUp)
            {
                // 前摇结束，开始追踪
                isWindingUp = false;
                // 获取目标位置（仅获取一次）
                if (Target != null)
                {
                    targetPosition = Target.transform.position;
                    hasTarget = true;
                }
                else
                {
                    Debug.LogError("跟踪弹目标机未设置");
                    // 如果没有目标，尝试获取玩家对象
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        targetPosition = player.transform.position;
                        hasTarget = true;
                    }
                }
                
                // 如果有目标，计算朝向目标的方向并旋转
                if (hasTarget)
                {
                    Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                    
                    // 更新刚体速度
                    if (rb2D != null)
                    {
                        rb2D.velocity = transform.up * Speed;
                    }
                }
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
    public void SetTarget(GameObject target)
    {
        this.Target = target;
    }
}

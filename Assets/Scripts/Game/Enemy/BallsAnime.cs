using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 球体移动模式枚举
public enum BallsMoveMode
{
    Path,       // 按照路径点移动
    Track,      // 追踪式移动
    Flicker,    // 闪烁模式
    Stationary, // 不移动
    Gravity     // 重力移动
}

public class BallsAnime : MonoBehaviour
{
    public List<Sprite> ballsSprites;// 球体精灵
    private Sprite currentBallSprite;// 当前球体

    [Header("移动参数")]
    public float MoveSpeed = 5f;// 球体移动速度
    [SerializeField]
    private float RotateSpeed = 360f;// 球体旋转速度
    public BallsMoveMode moveMode = BallsMoveMode.Path;// 球体移动模式
    public BallsMoveMode secondaryMoveMode = BallsMoveMode.Stationary;// 二段移动模式
    
    [Header("移动点参数")]
    private List<GameObject> MovePoints;// 移动点列表
    private int currentPointIndex = 0;// 当前目标移动点索引
    public float ArrivalDistance = 0.1f;// 到达移动点的判定距离
    private Vector2 moveDirection;// 移动方向向量
    
    [Header("追踪参数")]
    private Vector2 trackDirection;// 追踪方向向量
    private GameObject player;// 玩家对象
    
    [Header("闪烁参数")]
    public float FlickerLifeTime = 8f;// 闪烁模式下的生存时间
    private float flickerTimer = 0f;// 闪烁模式计时器
    private float fadeTime = 2f;// 淡入时间
    private float fadeTimer = 0f;// 淡入计时器
    
    [Header("重力参数")]
    public bool useGravity = false;// 是否使用重力
    public float gravityScale = 1f;// 重力缩放
    
    [Header("组件")]
    private SpriteRenderer spriteRenderer;// 精灵渲染器组件
    private Rigidbody2D rb2D;// 刚体组件

    // 边界值
    private readonly float minX = -10.5f;
    private readonly float maxY = 4.5f;
    private readonly float minY = -6.5f;
    private readonly float maxX = 6.0f;

    void OnEnable()
    {

        player = GameObject.FindGameObjectWithTag("Player");

        int randomIndex = Random.Range(0, ballsSprites.Count);
        currentBallSprite = ballsSprites[randomIndex];// 随机选择一个球体精灵
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer.sprite = currentBallSprite;// 初始化精灵渲染器的精灵为当前球体精灵
        
        // 重置移动点索引
        currentPointIndex = 0;
        
        // 初始化移动方向
        moveDirection = Vector2.zero;
        
        // 初始化重力参数
        if (rb2D != null)
        {
            rb2D.gravityScale = 0f; // 默认禁用重力
        }
        
        // 初始化追踪模式
        if (moveMode == BallsMoveMode.Track)
        {
            InitializeTracking();
        }
        // 初始化闪烁模式
        else if (moveMode == BallsMoveMode.Flicker)
        {
            InitializeFlicker();
        }
        // 初始化重力模式
        else if (moveMode == BallsMoveMode.Gravity)
        {
            InitializeGravity();
        }
    }

    void Update()
    {
        // 旋转球体
        RotateBall();
        
        // 检查边界
        CheckBounds();
        
        // 根据移动模式执行不同的移动逻辑
        switch (moveMode)
        {
            case BallsMoveMode.Path:
                MoveToNextPoint();
                break;
            case BallsMoveMode.Track:
                TrackMove();
                break;
            case BallsMoveMode.Stationary:
                // 不移动
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                }
                break;
            case BallsMoveMode.Flicker:
                FlickerUpdate();
                break;
            case BallsMoveMode.Gravity:
                GravityUpdate();
                break;
        }
    }

    public void SetMovePoints(List<GameObject> movePoints)// 设置移动点列表
    {
        MovePoints = movePoints;
        currentPointIndex = 0;
        // 初始化第一个移动方向
        if (MovePoints != null && MovePoints.Count > 0)
        {
            UpdateMoveDirection();
        }
    }
    
    /// <summary>
    /// 初始化追踪模式
    /// </summary>
    private void InitializeTracking()
    {
        if (player != null)
        {
            // 获取玩家当前位置作为追踪目标
            Vector3 trackTargetPosition = player.transform.position;
            // 计算追踪方向（只计算一次）
            trackDirection = (trackTargetPosition - transform.position).normalized;
        }
    }
    
    /// <summary>
    /// 初始化闪烁模式
    /// </summary>
    private void InitializeFlicker()
    {
        // 检查路径点列表是否有且仅有一个元素
        if (MovePoints != null && MovePoints.Count == 1)
        {
            // 移动到指定位置
            transform.position = MovePoints[0].transform.position;
        }
        
        // 初始化透明度为0
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        
        // 重置计时器
        flickerTimer = 0f;
        fadeTimer = 0f;
    }
    
    /// <summary>
    /// 检查边界
    /// </summary>
    private void CheckBounds()
    {
        if (transform.position.x < minX || transform.position.x > maxX || 
            transform.position.y < minY || transform.position.y > maxY)
        {
            // 超出边界，回收敌人
            Global_ObjectPool.Instance.Recycle(gameObject);
        }
    }
    
    /// <summary>
    /// 更新移动方向
    /// </summary>
    private void UpdateMoveDirection()
    {
        if (MovePoints == null || MovePoints.Count == 0 || currentPointIndex >= MovePoints.Count)
        {
            moveDirection = Vector2.zero;
            return;
        }
        
        // 获取当前目标点
        GameObject targetPoint = MovePoints[currentPointIndex];
        if (targetPoint != null)
        {
            // 计算移动方向
            moveDirection = (targetPoint.transform.position - transform.position).normalized;
        }
    }
    
    /// <summary>
    /// 移动到下一个点
    /// </summary>
    private void MoveToNextPoint()
    {
        if (MovePoints == null || MovePoints.Count == 0 || currentPointIndex >= MovePoints.Count)
        {
            return;
        }
        
        // 获取当前目标点
        GameObject targetPoint = MovePoints[currentPointIndex];
        if (targetPoint == null)
        {
            // 目标点不存在，移动到下一个点
            currentPointIndex++;
            UpdateMoveDirection();
            return;
        }
        
        // 直接计算目标方向（无平滑）
        Vector2 targetDirection = (targetPoint.transform.position - transform.position).normalized;
        moveDirection = targetDirection;
        
        // 移动敌人
        if (rb2D != null)
        {
            rb2D.velocity = moveDirection * MoveSpeed;
        }
        
        // 检查是否到达目标点
            if (Vector2.Distance(transform.position, targetPoint.transform.position) < ArrivalDistance)
            {
                // 到达目标点，移动到下一个点
                currentPointIndex++;
                UpdateMoveDirection();
                
                // 如果所有点都已到达
                if (currentPointIndex >= MovePoints.Count)
                {
                    // 所有移动点都已到达
                    if (rb2D != null)
                    {
                        rb2D.velocity = Vector2.zero;
                    }
                    // 切换到二段移动模式
                    SwitchToSecondaryMoveMode();
                }
            }
    }
    
    /// <summary>
    /// 追踪式移动
    /// </summary>
    private void TrackMove()
    {
        if (rb2D == null)
        {
            return;
        }
        
        // 一直向初始化时计算的方向移动
        if (trackDirection != Vector2.zero)
        {
            rb2D.velocity = trackDirection * MoveSpeed;
        }
    }
    
    /// <summary>
    /// 闪烁模式更新
    /// </summary>
    private void FlickerUpdate()
    {
        // 淡入效果
        if (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        }
        
        // 倒计时
        flickerTimer += Time.deltaTime;
        if (flickerTimer >= FlickerLifeTime)
        {
            // 生存时间结束，回收
            Global_ObjectPool.Instance.Recycle(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化重力模式
    /// </summary>
    private void InitializeGravity()
    {
        if (rb2D != null)
        {
            // 启用重力
            rb2D.gravityScale = gravityScale;
            // 禁用速度，开始自由落体
            rb2D.velocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 重力模式更新
    /// </summary>
    private void GravityUpdate()
    {
        // 重力模式下，物理引擎会自动处理移动
        // 这里可以添加额外的逻辑，比如碰撞检测等
    }
    
    /// <summary>
    /// 切换到二段移动模式
    /// </summary>
    private void SwitchToSecondaryMoveMode()
    {
        // 切换移动模式为二段移动模式
        moveMode = secondaryMoveMode;
        
        // 根据新的移动模式进行初始化
        switch (moveMode)
        {
            case BallsMoveMode.Track:
                InitializeTracking();
                break;
            case BallsMoveMode.Stationary:
                // 不移动，保持当前状态
                break;
            case BallsMoveMode.Flicker:
                InitializeFlicker();
                break;
            case BallsMoveMode.Gravity:
                InitializeGravity();
                break;
        }
    }
    
    /// <summary>
    /// 旋转球体
    /// </summary>
    private void RotateBall()
    {
        transform.Rotate(Vector3.forward, RotateSpeed * Time.deltaTime);
    }
}

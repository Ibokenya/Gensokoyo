using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Dir{// 标记妖精的左右移动方向
// 由于左右移动涉及动画，所以每次更换方向至少需要间隔一定时间（否则动画会抽搐）
    Idle,// 大多情况下使用Idle，哪怕上下左右飞
    Left,// 只会当妖精从屏幕左右出现时转为左或右
    Right,
}

// 敌人移动模式枚举
public enum EnemyMoveMode
{
    Path,       // 按照路径点移动
    Track,      // 追踪式移动
    Stationary, // 不移动
    Flicker,    // 闪烁模式
    Gravity     // 重力移动
}

public class EnemyAnime : MonoBehaviour
{
    [Header("不同类型妖精的变体精灵")]
    public List<Sprite> enemySprites1;// 妖精的变体精灵
    public List<Sprite> enemySprites2;// 妖精的变体精灵
    public List<Sprite> enemySprites3;// 妖精的变体精灵
    public List<Sprite> enemySprites4;// 妖精的变体精灵
    private List<Sprite> currentEnemySprites;// 当前妖精的变体精灵

    [Header("动画参数")]
    [SerializeField]    
    private int _currentIndex = 0;// 动画索引
    private float TimeClock;// 时钟，用来记录过了多长时间
    [Header("动画速度（每隔多少帧切换一次动画）")]
    public int AnimeSpeed = 4;// 每隔多少帧切换一次动画

    [Header("移动参数")]
    public float MoveSpeed = 5f;// 妖精移动速度
    [SerializeField]
    private Dir _currentDir = Dir.Idle;// 当前移动方向
    public EnemyMoveMode moveMode = EnemyMoveMode.Path;// 敌人移动模式
    public EnemyMoveMode secondaryMoveMode = EnemyMoveMode.Stationary;// 二段移动模式
    
    [Header("移动点参数")]
    private List<GameObject> MovePoints;// 移动点列表
    private int currentPointIndex = 0;// 当前目标移动点索引
    public float ArrivalDistance = 0.1f;// 到达移动点的判定距离
    private Vector2 moveDirection;// 移动方向向量
    private float t = 0f;// 贝塞尔曲线参数
    public float bezierSpeed = 0.5f;// 贝塞尔曲线移动速度
    private Vector2 startPoint;// 贝塞尔曲线起点
    private Vector2 controlPoint;// 贝塞尔曲线控制点
    private Vector2 endPoint;// 贝塞尔曲线终点
    private bool isMovingAlongBezier = false;// 是否正在沿贝塞尔曲线移动
    
    [Header("闪烁参数")]
    public float FlickerLifeTime = 8f;// 闪烁模式下的生存时间
    private float flickerTimer = 0f;// 闪烁模式计时器
    private readonly float fadeTime = 2f;// 淡入时间
    private float fadeTimer = 0f;// 淡入计时器
    
    [Header("重力参数")]
    public bool useGravity = false;// 是否使用重力
    public float gravityScale = 1f;// 重力缩放
    
    [Header("追踪参数")]
    private Vector2 trackDirection;// 追踪方向向量
    private Vector3 trackTargetPosition;// 追踪目标位置
    private GameObject player;// 玩家对象

    [Header("组件")]
    private SpriteRenderer spriteRenderer;// 精灵渲染器组件
    private Rigidbody2D rb2D;// 刚体组件

    // 边界值
    private readonly float minX = -10.5f;
    private readonly float maxX = 4.5f;
    private readonly float minY = -6.5f;
    private readonly float maxY = 6.0f;

    void OnEnable()
    {

        player = GameObject.FindGameObjectWithTag("Player");
        
        int randomIndex = Random.Range(0, 4);
        switch (randomIndex)// 随机选择一个妖精的变体精灵
        {
            case 0:
                currentEnemySprites = enemySprites1;
                break;
            case 1:
                currentEnemySprites = enemySprites2;
                break;
            case 2:
                currentEnemySprites = enemySprites3;
                break;
            case 3:
                currentEnemySprites = enemySprites4;
                break;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D= GetComponent<Rigidbody2D>();
        spriteRenderer.sprite = currentEnemySprites[_currentIndex];// 初始化精灵渲染器的精灵为当前妖精的变体精灵
        
        // 重置移动点索引
        currentPointIndex = 0;
        
        // 初始化移动方向
        moveDirection = Vector2.zero;
        
        // 初始化贝塞尔曲线参数
        t = 0f;
        isMovingAlongBezier = false;
        
        // 初始化闪烁模式参数
        flickerTimer = 0f;
        fadeTimer = 0f;
        
        // 初始化重力参数
        if (rb2D != null)
        {
            rb2D.gravityScale = 0f; // 默认禁用重力
        }
        
        // 初始化追踪模式
        if (moveMode == EnemyMoveMode.Track)
        {
            InitializeTracking();
        }
        // 初始化闪烁模式
        else if (moveMode == EnemyMoveMode.Flicker)
        {
            InitializeFlicker();
        }
        // 初始化重力模式
        else if (moveMode == EnemyMoveMode.Gravity)
        {
            InitializeGravity();
        }
        
        // 初始化渐入效果
        InitializeFadeIn();
    }

    void Update()
    {
        // 播放动画
        PlayAnimation();
        
        // 处理渐入效果
        HandleFadeIn();
        
        // 检查边界
        CheckBounds();
        
        // 根据移动模式执行不同的移动逻辑
        switch (moveMode)
        {
            case EnemyMoveMode.Path:
                MoveToNextPoint();
                break;
            case EnemyMoveMode.Track:
                TrackMove();
                break;
            case EnemyMoveMode.Stationary:
                // 不移动
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                }
                break;
            case EnemyMoveMode.Flicker:
                FlickerUpdate();
                break;
            case EnemyMoveMode.Gravity:
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
            // 初始化贝塞尔曲线起点
            startPoint = transform.position;
            if (MovePoints.Count > 0)
            {
                endPoint = MovePoints[0].transform.position;
                // 计算控制点（在起点和终点之间）
                controlPoint = (startPoint + endPoint) * 0.5f;
                // 稍微向上偏移控制点，使曲线更平滑
                controlPoint += new Vector2(0, 0.5f);
                t = 0f;
                isMovingAlongBezier = true;
            }
        }
    }
    
    /// <summary>
    /// 设置敌人的移动方向
    /// </summary>
    /// <param name="newDir">新的方向</param>
    public void SetDirection(Dir newDir)
    {
        if (_currentDir != newDir)
        {
            _currentDir = newDir;
            // 重置动画索引
            _currentIndex = 0;
            TimeClock = 0f;
            // 立即更新精灵
            UpdateSprite();
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
            trackTargetPosition = player.transform.position;
            // 计算追踪方向（只计算一次）
            trackDirection = (trackTargetPosition - transform.position).normalized;
        }
    }
    
    /// <summary>
    /// 初始化渐入效果
    /// </summary>
    private void InitializeFadeIn()
    {
        // 初始化透明度为0
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        
        // 重置计时器
        fadeTimer = 0f;
    }
    
    /// <summary>
    /// 处理渐入效果
    /// </summary>
    private void HandleFadeIn()
    {
        if (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        }
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
        
        // 如果正在沿贝塞尔曲线移动
        if (isMovingAlongBezier)
        {
            // 更新贝塞尔曲线参数
            t += Time.deltaTime * bezierSpeed;
            t = Mathf.Clamp01(t);
            
            // 计算贝塞尔曲线上的当前点
            // B(t) = (1-t)^2 * P0 + 2*(1-t)*t * P1 + t^2 * P2
            Vector2 currentPosition = Mathf.Pow(1 - t, 2) * startPoint + 2 * (1 - t) * t * controlPoint + Mathf.Pow(t, 2) * endPoint;
            
            // 直接设置位置，避免方向计算导致的偏移
            if (rb2D != null)
            {
                // 使用刚体的MovePosition方法，确保平滑移动
                rb2D.MovePosition(currentPosition);
            }
            else
            {
                // 如果没有刚体，直接设置位置
                transform.position = currentPosition;
            }
            
            // 检查是否到达终点
            if (t >= 1f)
            {
                // 到达目标点，移动到下一个点
                currentPointIndex++;
                
                // 如果还有下一个点
                if (currentPointIndex < MovePoints.Count)
                {
                    // 更新贝塞尔曲线参数
                    startPoint = endPoint;
                    endPoint = MovePoints[currentPointIndex].transform.position;
                    // 计算控制点（在起点和终点之间）
                    controlPoint = (startPoint + endPoint) * 0.5f;
                    // 根据路径点之间的方向调整控制点偏移
                    Vector2 direction = (endPoint - startPoint).normalized;
                    // 垂直于移动方向的偏移
                    Vector2 perpendicular = new (-direction.y, direction.x);
                    // 偏移量根据路径长度动态调整
                    float offset = Mathf.Min(Vector2.Distance(startPoint, endPoint) * 0.3f, 1f);
                    controlPoint += perpendicular * offset;
                    t = 0f;
                }
                else
                {
                    // 所有点都已到达，切换到二段移动模式
                    isMovingAlongBezier = false;
                    if (rb2D != null)
                    {
                        rb2D.velocity = Vector2.zero;
                    }
                    // 切换到二段移动模式
                    SwitchToSecondaryMoveMode();
                }
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
            case EnemyMoveMode.Track:
                InitializeTracking();
                break;
            case EnemyMoveMode.Stationary:
                // 不移动，保持当前状态
                break;
            case EnemyMoveMode.Flicker:
                InitializeFlicker();
                break;
            case EnemyMoveMode.Gravity:
                InitializeGravity();
                break;
        }
    }
    
    /// <summary>
    /// 播放动画
    /// </summary>
    private void PlayAnimation()
    {
        // 增加时钟计数
        TimeClock += Time.deltaTime;

        // 计算当前应该显示的帧数
        int currentFrame = Mathf.FloorToInt(TimeClock * 60f);

        // 检查是否需要切换动画帧
        if (currentFrame >= AnimeSpeed)
        {
            // 根据当前方向更新动画索引
            switch (_currentDir)
            {
                case Dir.Idle:
                    // Idle状态使用前5张精灵（0-4）
                    _currentIndex = (_currentIndex + 1) % 5;
                    break;
                case Dir.Left:
                case Dir.Right:
                    // 左右移动状态使用后7张精灵
                    _currentIndex = 5 + ((_currentIndex - 5 + 1) % 7);
                    break;
            }
            
            // 更新精灵
            UpdateSprite();

            // 重置时钟，保留余数以保持动画流畅
            TimeClock -= (float)AnimeSpeed / 60f;
        }
    }
    
    /// <summary>
    /// 更新精灵
    /// </summary>
    private void UpdateSprite()
    {
        if (spriteRenderer != null && currentEnemySprites.Count > 0)
        {
            // 根据当前方向设置精灵和翻转
            switch (_currentDir)
            {
                case Dir.Idle:
                case Dir.Right:
                    // Idle和Right状态不翻转
                    spriteRenderer.flipX = false;
                    // Idle状态使用前5张精灵
                    if (_currentDir == Dir.Idle)
                    {
                        spriteRenderer.sprite = currentEnemySprites[Mathf.Min(_currentIndex, 4)];
                    }
                    // Right状态使用后7张精灵
                    else
                    {
                        spriteRenderer.sprite = currentEnemySprites[Mathf.Min(5 + (_currentIndex % 7), currentEnemySprites.Count - 1)];
                    }
                    break;
                case Dir.Left:
                    // Left状态翻转精灵
                    spriteRenderer.flipX = true;
                    // 使用后7张精灵
                    spriteRenderer.sprite = currentEnemySprites[Mathf.Min(5 + (_currentIndex % 7), currentEnemySprites.Count - 1)];
                    break;
            }
        }
    }
}

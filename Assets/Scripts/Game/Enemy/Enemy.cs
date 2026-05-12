using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基础属性")]
    public int Hp = 100;// 敌人生命值
    public int maxHp = 100;// 敌人最大生命值

    [Header("移动参数")]
    public float MoveSpeed = 5f;// 敌人移动速度
    public MoveMode moveMode = MoveMode.Path;// 敌人移动模式
    public SecondaryMode secondaryMoveMode = SecondaryMode.Stationary;// 二段移动模式

    [Header("移动路径")]
    protected List<GameObject> MovePoints;// 移动点列表
    protected int currentPointIndex = 0;// 当前目标移动点索引
    public float ArrivalDistance = 0.1f;// 到达移动点判断距离
    protected Vector2 moveDirection;// 移动方向向量
    
    [Header("贝塞尔曲线移动参数")]
    protected float bezierT = 0f;// 当前曲线参数t
    protected float bezierStep = 0.02f;// 曲线步长（每帧t的增量）
    protected Vector2[] bezierControlPoints = new Vector2[4];// 当前贝塞尔曲线的4个控制点
    protected Vector2 bezierStartPosition;// 当前曲线段的起始位置
    protected Vector2 bezierEndPosition;// 当前曲线段的结束位置

    [Header("闪烁参数")]
    public float FlickerLifeTime = 8f;// 闪烁模式下的生存时间
    protected float flickerTimer = 0f;// 闪烁模式计时器
    public float fadeTime = 2f;// 淡入时间
    protected float fadeTimer = 0f;// 淡入计时器

    [Header("重力参数")]
    public float gravityScale = 1f;// 重力缩放

    [Header("追踪参数")]
    protected Vector2 trackDirection;// 追踪方向向量
    protected GameObject player;// 玩家对象

    [Header("组件")]
    protected SpriteRenderer spriteRenderer;// 精灵渲染器
    protected Rigidbody2D rb2D;// 刚体
    protected bool isFirstMoveCompleted = false;// 是否第一段移动完成

    // 边界值
    protected readonly float minX = -11f;
    protected readonly float maxX = 5f;
    protected readonly float minY = -7.5f;
    protected readonly float maxY = 6.5f;

    // 掉落物配置
    public List<ItemDropConfig> itemDrops = new List<ItemDropConfig>();

    // 瞄准标记
    public GameObject aimMarker; // 瞄准标记对象
    public bool isMarked = false; // 是否已被标记

    // 颜色控制
    private Color originalColor; // 原始颜色
    private float redIntensity = 0f; // 红色强度
    
    // 死亡状态
    private bool isDead = false; // 敌人是否已经死亡
    private bool isKilled = false; // 敌人是否被击杀（而非自然回收）

    protected virtual void OnEnable()
    {
        isFirstMoveCompleted = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D = GetComponent<Rigidbody2D>();
        currentPointIndex = 0;
        moveDirection = Vector2.zero;
        flickerTimer = 0f;
        fadeTimer = 0f;
        isMarked = false; // 重置标记状态
        isDead = false; // 重置死亡状态
        isKilled = false; // 重置被击杀标记
        maxHp = Hp;
        
        // 重置贝塞尔曲线参数
        bezierT = 0f;
        bezierStartPosition = Vector2.zero;
        bezierEndPosition = Vector2.zero;
        for (int i = 0; i < 4; i++)
        {
            bezierControlPoints[i] = Vector2.zero;
        }

        if (rb2D != null)
        {
            rb2D.gravityScale = 0f; // 默认禁用重力
        }

        // 初始化各种移动模式
        if (moveMode == MoveMode.Path)
        {
            InitializePath();
        }
        else if (moveMode == MoveMode.Track)
        {
            InitializeTracking();
        }
        else if (moveMode == MoveMode.Flicker)
        {
            InitializeFlicker();
        }
        else if (moveMode == MoveMode.Gravity)
        {
            InitializeGravity();
        }

        // 重置颜色
        ResetColor();
    }

    protected virtual void Update()
    {
        // 检查边界
        CheckBounds();

        // 根据移动模式执行不同的移动逻辑
        switch (moveMode)
        {
            case MoveMode.Path:
                MoveToNextPoint();
                break;
            case MoveMode.Track:
                TrackMove();
                break;
            case MoveMode.Flicker:
                FlickerUpdate();
                break;
            case MoveMode.Gravity:
                break;
        }

        // 处理FlickerOut模式
        if (secondaryMoveMode == SecondaryMode.FlickerOut && isFirstMoveCompleted)
        {
            FlickerOutUpdate();
        }
    }

    /// <summary>
    /// 设置玩家对象
    /// </summary>
    public virtual void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
    }

    /// <summary>
    /// 设置移动点列表
    /// </summary>
    public virtual void SetMovePoints(List<GameObject> movePoints)
    {
        MovePoints = movePoints;
        currentPointIndex = 0;
        if (MovePoints != null && MovePoints.Count > 0)
        {
            UpdateMoveDirection();
        }
    }

    /// <summary>
    /// 设置掉落物配置
    /// </summary>
    public virtual void SetItemDrops(List<ItemDropConfig> drops)
    {
        itemDrops = drops;
    }

    /// <summary>
    /// 伤害敌人
    /// </summary>
    public virtual void Damage(int damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 处理敌人死亡
    /// </summary>
    public virtual void Die()
    {
        // 防止重复调用
        if (isDead)
        {
            return;
        }
        
        isDead = true;
        isKilled = true; // 标记敌人被击杀
        
        // 检查是否处于时停状态
        if (Global_GameManager.Instance.state == State.TimeStop)
        {
            // 时停期间，延迟到非时停状态时再处理
            return;
        }
        
        // 检查是否满足处决条件
        if (CheckExecuteCondition())
        {
            // 满足处决条件，延迟回收
            StartCoroutine(DelayedDelete());
        }
        else
        {
            // 不满足处决条件，立即回收
            Delete();
        } 
    }
    
    /// <summary>
    /// 检查是否满足处决条件
    /// </summary>
    /// <returns>是否满足处决条件</returns>
    private bool CheckExecuteCondition()
    {
        // 检查敌人初始血量是否>=700
        if (maxHp >= 700)
        {
            // 查找恶魔之眼攻击脚本实例
            EvilEyeAttack evilEyeAttack = FindObjectOfType<EvilEyeAttack>();
            if (evilEyeAttack != null)
            {
                // 检查恶魔之眼的透明度是否为1
                SpriteRenderer evilEyeRenderer = evilEyeAttack.GetComponent<SpriteRenderer>();
                if (evilEyeRenderer != null && evilEyeRenderer.color.a >= 0.99f)
                {
                    // 停止敌人的一切行为
                    StopEnemyActions();
                    
                    // 触发处决效果
                    evilEyeAttack.ExecuteEnemy(transform.position);
                    return true;
                }
            }
            else
            {
                Debug.LogWarning("未找到恶魔之眼攻击脚本实例");
            }
        }
        return false;
    }
    
    /// <summary>
    /// 停止敌人的一切行为
    /// </summary>
    private void StopEnemyActions()
    {
        // 停止移动
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.isKinematic = true;
        }
        
        // 禁用碰撞
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        
        // 禁用动画
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // 禁用脚本
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// 延迟回收敌人
    /// </summary>
    private IEnumerator DelayedDelete()
    {
        // 等待1秒，让处决动画完成
        yield return new WaitForSeconds(1f);
        Delete();
    }

    public virtual void Delete()
    {
        // 只有被击杀的敌人才生成掉落物
        if (isKilled)
        {
            SpawnItemDrops();
            CreateItem.Instance.SpawnScoreItems(transform.position);
        }
        // 解除标记与敌人的父子关系，防止对象池复用时出现异常
        if (aimMarker != null)
        {
            aimMarker.transform.parent = null;
            // 查找MagicAttack实例并回收标记
            MagicAttack magicAttack = FindObjectOfType<MagicAttack>();
            if (magicAttack != null)
            {
                magicAttack.RecycleMarker(aimMarker);
            }
            aimMarker = null;
        }
        isMarked = false;
        
        transform.parent = null;
        // 从游戏管理器中移除
        if (Global_GameManager.Instance != null)
        {
            Global_GameManager.Instance.RemoveEnemy(gameObject);
        }
        // 回收敌人
        Global_ObjectPool.Instance.Recycle(gameObject);
    }

    /// <summary>
    /// 生成掉落物
    /// </summary>
    protected virtual void SpawnItemDrops()
    {
        if (itemDrops == null || itemDrops.Count == 0)
        {
            return;
        }

        CreateItem.Instance.SpawnItems(transform.position, itemDrops);
    }

    /// <summary>
    /// 更新移动方向
    /// </summary>
    protected virtual void UpdateMoveDirection()
    {
        if (MovePoints == null || MovePoints.Count == 0 || currentPointIndex >= MovePoints.Count)
        {
            moveDirection = Vector2.zero;
            return;
        }

        GameObject targetPoint = MovePoints[currentPointIndex];
        if (targetPoint != null)
        {
            moveDirection = (targetPoint.transform.position - transform.position).normalized;
        }
    }

    /// <summary>
    /// 计算3次贝塞尔曲线上的点
    /// </summary>
    /// <param name="t">曲线参数，范围[0,1]</param>
    /// <param name="p0">控制点0</param>
    /// <param name="p1">控制点1</param>
    /// <param name="p2">控制点2</param>
    /// <param name="p3">控制点3</param>
    /// <returns>曲线上的点</returns>
    protected Vector2 CalculateCubicBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 point = uuu * p0;
        point += 3f * uu * t * p1;
        point += 3f * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    /// <summary>
    /// 初始化贝塞尔曲线控制点
    /// </summary>
    protected virtual void InitializeBezierCurve()
    {
        if (MovePoints == null || MovePoints.Count < 2)
        {
            return;
        }

        // 获取起始点和结束点
        bezierStartPosition = transform.position;
        GameObject endPointObj = MovePoints[currentPointIndex];
        if (endPointObj == null)
        {
            return;
        }
        bezierEndPosition = endPointObj.transform.position;

        // 计算控制点
        Vector2 direction = (bezierEndPosition - bezierStartPosition).normalized;
        float distance = Vector2.Distance(bezierStartPosition, bezierEndPosition);

        // 控制点1：从起点沿方向延伸1/3距离
        bezierControlPoints[0] = bezierStartPosition;
        bezierControlPoints[1] = bezierStartPosition + direction * (distance * 0.33f);

        // 控制点2：从终点沿反方向延伸1/3距离
        bezierControlPoints[2] = bezierEndPosition - direction * (distance * 0.33f);
        bezierControlPoints[3] = bezierEndPosition;

        // 计算合适的步长：基于移动速度和曲线长度
        // 贝塞尔曲线的近似长度约为直线距离的1.1倍
        float curveLength = distance * 1.1f;
        float timeToComplete = curveLength / (MoveSpeed * 0.8f);
        bezierStep = Time.deltaTime / timeToComplete;

        // 重置t值
        bezierT = 0f;
    }

    /// <summary>
    /// 移动到下一个点（使用3次贝塞尔曲线）
    /// </summary>
    protected virtual void MoveToNextPoint()
    {
        if (MovePoints == null || MovePoints.Count == 0 || currentPointIndex >= MovePoints.Count)
        {
            return;
        }

        GameObject targetPoint = MovePoints[currentPointIndex];
        if (targetPoint == null)
        {
            currentPointIndex++;
            InitializeBezierCurve();
            return;
        }

        // 检查是否为两个点的简单路径，如果是则直接直线移动
        if (MovePoints.Count == 2)
        {
            MoveToNextPointLinear(targetPoint);
        }
        else
        {
            // 使用贝塞尔曲线移动
            MoveWithBezierCurve();
        }
    }

    /// <summary>
    /// 使用贝塞尔曲线移动
    /// </summary>
    protected virtual void MoveWithBezierCurve()
    {
        if (rb2D == null)
        {
            return;
        }

        // 如果是第一次移动，初始化贝塞尔曲线
        if (bezierT <= 0f)
        {
            InitializeBezierCurve();
        }

        // 增加t值
        bezierT += bezierStep;

        // 计算当前曲线上的点
        Vector2 currentPos = CalculateCubicBezierPoint(
            Mathf.Clamp01(bezierT),
            bezierControlPoints[0],
            bezierControlPoints[1],
            bezierControlPoints[2],
            bezierControlPoints[3]
        );

        // 设置位置
        transform.position = currentPos;

        // 计算移动方向（用于其他可能需要方向的地方）
        Vector2 nextPos = CalculateCubicBezierPoint(
            Mathf.Clamp01(bezierT + 0.01f),
            bezierControlPoints[0],
            bezierControlPoints[1],
            bezierControlPoints[2],
            bezierControlPoints[3]
        );
        moveDirection = (nextPos - currentPos).normalized;

        // 检查是否到达终点
        if (bezierT >= 1f)
        {
            currentPointIndex++;

            if (currentPointIndex >= MovePoints.Count)
            {
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                }
                SwitchToSecondaryMoveMode();
                isFirstMoveCompleted = true;
            }
            else
            {
                // 准备下一个曲线段
                bezierT = 0f;
            }
        }
    }

    /// <summary>
    /// 直线移动到目标点（用于两个点的简单路径）
    /// </summary>
    protected virtual void MoveToNextPointLinear(GameObject targetPoint)
    {
        if (rb2D == null)
        {
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 targetPos = targetPoint.transform.position;
        Vector2 direction = (targetPos - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, targetPos);

        // 计算本帧移动距离
        float moveDistance = MoveSpeed * 0.8f * Time.deltaTime;

        // 如果距离小于本帧移动距离，直接到达目标点
        if (distance <= moveDistance)
        {
            transform.position = targetPos;
            moveDirection = direction;
            
            currentPointIndex++;
            
            if (currentPointIndex >= MovePoints.Count)
            {
                rb2D.velocity = Vector2.zero;
                SwitchToSecondaryMoveMode();
                isFirstMoveCompleted = true;
            }
            else
            {
                InitializeBezierCurve();
            }
        }
        else
        {
            // 正常移动
            transform.position += (Vector3)(direction * moveDistance);
            moveDirection = direction;
            rb2D.velocity = direction * MoveSpeed;
        }
    }

    /// <summary>
    /// 初始化路径移动模式
    /// </summary>
    protected virtual void InitializePath()
    {
        // 初始化贝塞尔曲线
        InitializeBezierCurve();
    }

    /// <summary>
    /// 初始化追踪模式
    /// </summary>
    protected virtual void InitializeTracking()
    {
        if (player != null)
        {
            Vector3 trackTargetPosition = player.transform.position;
            trackDirection = (trackTargetPosition - transform.position).normalized;
        }
    }

    /// <summary>
    /// 追踪式移动
    /// </summary>
    protected virtual void TrackMove()
    {
        if (rb2D == null)
        {
            return;
        }

        if (trackDirection != Vector2.zero)
        {
            rb2D.velocity = trackDirection * MoveSpeed;
        }
    }

    /// <summary>
    /// 初始化闪烁模式
    /// </summary>
    protected virtual void InitializeFlicker()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        }
        flickerTimer = 0f;
        fadeTimer = 0f;
    }

    /// <summary>
    /// 闪烁模式更新
    /// </summary>
    protected virtual void FlickerUpdate()
    {
        if (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeTime);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            }
        }
        else
        {
            flickerTimer += Time.deltaTime;
            if (flickerTimer >= FlickerLifeTime)
            {
                SwitchToSecondaryMoveMode();
                isFirstMoveCompleted = true;
            }
        }
    }

    /// <summary>
    /// 初始化重力模式
    /// </summary>
    public virtual void InitializeGravity()
    {
        if (rb2D != null)
        {
            rb2D.gravityScale = gravityScale;
            rb2D.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 切换到二段移动模式
    /// </summary>
    protected virtual void SwitchToSecondaryMoveMode()
    {
        switch (secondaryMoveMode)
        {
            case SecondaryMode.Track:
                moveMode = MoveMode.Track;
                InitializeTracking();
                break;
            case SecondaryMode.FlickerOut:
                InitializeFlickerOut();
                break;
            case SecondaryMode.Gravity:
                moveMode = MoveMode.Gravity;
                InitializeGravity();
                break;
            case SecondaryMode.Stationary:
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                }
                break;
        }
    }

    /// <summary>
    /// 初始化闪烁淡出模式
    /// </summary>
    protected virtual void InitializeFlickerOut()
    {
        flickerTimer = 0f;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 闪烁淡出模式更新
    /// </summary>
    protected virtual void FlickerOutUpdate()
    {
        flickerTimer += Time.deltaTime;
        float alpha = Mathf.Clamp01(1f - (flickerTimer / 1f));
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        }

        if (alpha <= 0f)
        {
            Delete();
        }
    }

    /// <summary>
    /// 检查边界
    /// </summary>
    protected virtual void CheckBounds()
    {
        if (transform.position.x < minX || transform.position.x > maxX ||
            transform.position.y < minY || transform.position.y > maxY)
        {
            Delete();
        }
    }

    /// <summary>
    /// 重置颜色
    /// </summary>
    public void ResetColor()
    {
        if (spriteRenderer != null)
        {
            originalColor = new Color(1f, 1f, 1f, 1f);
            spriteRenderer.color = originalColor;
            redIntensity = 0f;
        }
    }

    /// <summary>
    /// 更新红色度（基于剩余血量）
    /// </summary>
    /// <param name="currentHp">当前血量</param>
    /// <param name="maxHpValue">最大血量</param>
    public void UpdateRedIntensity()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        // 计算血量百分比
        float hpPercentage = Mathf.Clamp01((float)Hp / maxHp);

        // 计算红色度（0血时红80%）
        redIntensity = (1f - hpPercentage) * 0.8f;

        // 应用红色度：默认初色为1,1,1，要求红色度为0则g与b都不降，要求红色度0.25则降低g与b0.25的值
        Color newColor = originalColor;
        newColor.g = Mathf.Clamp01(originalColor.g - redIntensity);
        newColor.b = Mathf.Clamp01(originalColor.b - redIntensity);
        spriteRenderer.color = newColor;
    }
}

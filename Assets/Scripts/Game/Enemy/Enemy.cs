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
        maxHp = Hp;

        if (rb2D != null)
        {
            rb2D.gravityScale = 0f; // 默认禁用重力
        }

        // 初始化各种移动模式
        if (moveMode == MoveMode.Path)
        {
            Debug.Log(transform.name + "初始化路径移动模式");
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
        // 生成掉落物
        SpawnItemDrops();
        Delete();
    }

    public virtual void Delete()
    {
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

        // 查找CreateItem实例
        CreateItem createItem = FindObjectOfType<CreateItem>();
        if (createItem != null)
        {
            createItem.SpawnItems(transform.position, itemDrops);
        }
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
    /// 移动到下一个点
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
            UpdateMoveDirection();
            return;
        }

        // 检查是否为两个点的简单路径，如果是则直接直线移动
        if (MovePoints.Count == 2)
        {
            MoveToNextPointLinear(targetPoint);
        }
        else
        {
            // 多点路径使用平滑移动
            Vector2 targetDirection = (targetPoint.transform.position - transform.position).normalized;
            moveDirection = targetDirection;

            if (rb2D != null)
            {
                rb2D.velocity = moveDirection * MoveSpeed;
            }
        }

        if (Vector2.Distance(transform.position, targetPoint.transform.position) < ArrivalDistance)
        {
            currentPointIndex++;
            UpdateMoveDirection();

            if (currentPointIndex >= MovePoints.Count)
            {
                if (rb2D != null)
                {
                    rb2D.velocity = Vector2.zero;
                }
                SwitchToSecondaryMoveMode();
                isFirstMoveCompleted = true;
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

        rb2D.velocity = direction * MoveSpeed;
        moveDirection = direction;
    }

    /// <summary>
    /// 初始化路径移动模式
    /// </summary>
    protected virtual void InitializePath()
    {
        UpdateMoveDirection();
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

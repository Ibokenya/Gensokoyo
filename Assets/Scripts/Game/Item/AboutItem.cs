using System;
using System.Collections;
using UnityEngine;

public class AboutItem : MonoBehaviour
{
    [Header("道具种类:0:HP+,1:HP,2:Power+,3:Power,4:Bomb+,5:Bomb,6:Grade,7:Grade-,8:Grade--")]
    [SerializeField]
    private int itemType;

    [Header("收集速度")]
    public float collectSpeed = 10f;

    private Rigidbody2D rb2D;
    public GameObject player;
    private int CheckInterval = 10;
    private bool isCollecting = false;
    public AudioClip collectClip;

    private Coroutine collectCoroutine;

    private bool isAutoFlying = false;
    private float autoFlySpeed = 15f;
    private float autoFlyCheckInterval = 0.3f;
    private float autoFlyCheckTimer = 0f;
    private const float autoFlyThreshold = 0.3f;
    private Vector3 autoFlyTargetPosition; // 缓存的目标位置
    
    // 公开属性供外部访问
    public bool IsCollecting { get { return isCollecting; } }
    public bool IsAutoFlying { get { return isAutoFlying; } }

    void OnEnable()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("Rigidbody2D component not found on Item object!");
        }
        else
        {
            // 重置重力为默认值，确保道具能正常下落
            rb2D.gravityScale = 0.12f;
            rb2D.velocity = Vector2.zero;
        }

        // 重置收集状态
        isCollecting = false;
        collectCoroutine = null;
        isAutoFlying = false;
        autoFlyCheckTimer = 0f;
    }

    void OnDisable()
    {
        // 停止收集协程
        if (collectCoroutine != null)
        {
            StopCoroutine(collectCoroutine);
            collectCoroutine = null;
        }
        isCollecting = false;
        isAutoFlying = false;
        autoFlyCheckTimer = 0f;
    }

    void Update()
    {
        // 检测SpellCard状态，自动飞向玩家
        CheckSpellCardState();

        // 自动飞向玩家逻辑
        if (isAutoFlying && player != null)
        {
            autoFlyCheckTimer += Time.deltaTime;
            if (autoFlyCheckTimer >= autoFlyCheckInterval)
            {
                autoFlyCheckTimer = 0f;
                // 更新目标位置
                autoFlyTargetPosition = player.transform.position;
            }
            FlyToPlayerContinuous();
        }

        CheckInterval--;
        if (CheckInterval <= 0)
        {
            CheckInterval = 10;
            CheckPos();
            CheckRecycleLine();
        }
    }

    /// <summary>
    /// 检测SpellCard状态，将道具标记为自动飞向玩家状态
    /// </summary>
    private void CheckSpellCardState()
    {
        // 如果已经在自动飞向状态或正在收集，无需处理
        if (isAutoFlying || isCollecting)
        {
            return;
        }

        // 检测游戏状态是否为SpellCard
        if (Global_GameManager.Instance != null && 
            Global_GameManager.Instance.state == State.SpellCard)
        {
            // 检查player是否有效
            if (player == null)
            {
                // 尝试查找玩家对象
                player = GameObject.FindGameObjectWithTag("Player");
            }

            if (player != null)
            {
                // 将道具标记为自动飞向玩家状态
                SetAutoFlyToPlayer(true, autoFlySpeed);
            }
        }
    }

    private void CheckPos()
    {
        // 检查道具是否在边界内
        if (transform.position.y < -8f)
        {
            // 道具超出边界，销毁道具
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 设置自动飞向玩家模式
    /// </summary>
    /// <param name="autoFly">是否自动飞向玩家</param>
    /// <param name="flySpeed">飞行速度</param>
    public void SetAutoFlyToPlayer(bool autoFly, float flySpeed = 15f)
    {
        isAutoFlying = autoFly;
        autoFlySpeed = flySpeed;
        if (autoFly)
        {
            // 立即初始化目标位置为玩家当前位置
            if (player != null)
            {
                autoFlyTargetPosition = player.transform.position;
            }
            // 禁用重力
            if (rb2D != null)
            {
                rb2D.gravityScale = 0f;
                rb2D.velocity = Vector2.zero;
            }
            // 停止之前的收集协程
            if (collectCoroutine != null)
            {
                StopCoroutine(collectCoroutine);
                collectCoroutine = null;
            }
            isCollecting = false;
        }
    }

    /// <summary>
    /// 持续飞向玩家（每隔autoFlyCheckInterval更新目标位置，每帧平滑移动）
    /// </summary>
    private void FlyToPlayerContinuous()
    {
        if (player == null || isCollecting)
        {
            return;
        }

        // 检查是否已经到达目标位置
        float distance = Vector3.Distance(transform.position, autoFlyTargetPosition);
        if (distance <= autoFlyThreshold)
        {
            // 到达目标位置，触发收集
            if (!isCollecting)
            {
                isCollecting = true;
                if (collectCoroutine != null)
                {
                    StopCoroutine(collectCoroutine);
                }
                collectCoroutine = StartCoroutine(CollectItemImmediate());
            }
            return;
        }

        // 计算朝向缓存目标位置的方向
        Vector3 direction = (autoFlyTargetPosition - transform.position).normalized;

        // 移动道具（每帧平滑移动）
        transform.position += direction * autoFlySpeed * Time.deltaTime;
    }

    /// <summary>
    /// 立即收集（用于自动飞向模式到达玩家时）
    /// </summary>
    private IEnumerator CollectItemImmediate()
    {
        // 触发效果
        Effect();
        // 播放收集音效
        if (Global_AudioManager.Instance != null && collectClip != null)
        {
            Global_AudioManager.Instance.PlayCollectSFX(collectClip);
        }

        yield return null;

        // 销毁道具
        if (Global_ObjectPool.Instance != null)
        {
            Global_ObjectPool.Instance.Recycle(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 碰撞器事件代表进入玩家收集范围
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCollecting && !isAutoFlying)
        {
            FlyToPlayer(collision.transform);
        }
    }

    /// <summary>
    /// 道具自动被玩家收取
    /// </summary>
    /// <param name="playerTransform">玩家transform</param>
    /// <param name="isRecycleLineTriggered">是否由回收线触发</param>
    public void FlyToPlayer(Transform playerTransform, bool isRecycleLineTriggered = false)
    {
        isCollecting = true;
        collectCoroutine = StartCoroutine(CollectItem(playerTransform, isRecycleLineTriggered));
    }

    /// <summary>
    /// 收集道具的协程
    /// </summary>
    private IEnumerator CollectItem(Transform playerTransform, bool isRecycleLineTriggered)
    {
        // 禁用重力，使道具不受重力影响
        if (rb2D != null)
        {
            rb2D.gravityScale = 0f;
            rb2D.velocity = Vector2.zero;
        }

        int updateCounter = 0;
        const int updateInterval = 10;

        // 触发器触发时，只获取一次玩家坐标
        Vector3 initialTargetPosition = playerTransform != null ? playerTransform.position : transform.position;

        // 回收线触发时的目标位置
        Vector3 recycleLineTargetPosition = playerTransform != null ? playerTransform.position : transform.position;

        // 持续移动
        while (true)
        {
            // 检查玩家对象是否存在
            if (playerTransform == null)
            {
                // 玩家对象已销毁，停止收集
                break;
            }

            // 计算目标位置
            Vector3 targetPosition;
            if (isRecycleLineTriggered)
            {
                // 回收线触发时，每10帧更新一次目标位置
                if (updateCounter % updateInterval == 0)
                {
                    recycleLineTargetPosition = playerTransform.position;
                }
                targetPosition = recycleLineTargetPosition;
                updateCounter++;

                // 检查是否到达当前目标位置
                if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
                {
                    break;
                }
            }
            else
            {
                // 触发器触发时，使用初始目标位置
                targetPosition = initialTargetPosition;

                // 检查是否到达初始目标位置
                if (Vector3.Distance(transform.position, initialTargetPosition) <= 0.1f)
                {
                    break;
                }
            }

            // 计算移动方向
            Vector3 direction = (targetPosition - transform.position).normalized;

            // 移动道具
            transform.position += collectSpeed * Time.deltaTime * direction;

            yield return null;
        }

        // 道具到达玩家位置，触发效果
        Effect();
        // 播放收集音效（限制同时播放数量）
        if (Global_AudioManager.Instance != null && collectClip != null)
        {
            Global_AudioManager.Instance.PlayCollectSFX(collectClip);
        }

        // 销毁道具
        if (Global_ObjectPool.Instance != null)
        {
            Global_ObjectPool.Instance.Recycle(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Effect()
    {
        switch (itemType)
        {
            case 0:// 道具类型为HP+
                Global_GameManager.Instance.AddLeftLife(1, 0);
                break;
            case 1:// 道具类型为HP
                Global_GameManager.Instance.AddLeftLife(0, 1);
                break;
            case 2:// 道具类型为Power+
                Global_GameManager.Instance.AddPower(100);
                break;
            case 3:// 道具类型为Power
                Global_GameManager.Instance.AddPower(1);
                break;
            case 4:// 道具类型为Bomb+
                Global_GameManager.Instance.AddBomb(1, 0);
                break;
            case 5:// 道具类型为Bomb
                Global_GameManager.Instance.AddBomb(0, 1);
                break;
            case 6:// 道具类型为Grade
                Global_GameManager.Instance.AddGrade(1);
                break;
            case 7:// 道具类型为Grade-
                Global_GameManager.Instance.AddScore(100);
                break;
            case 8:// 道具类型为Grade--
                Global_GameManager.Instance.AddScore(10);
                break;
        }
    }

    /// <summary>
    /// 检查回收线逻辑
    /// </summary>
    private void CheckRecycleLine()
    {
        // 回收线高度（与PlayerCollision中的BorderGetLine保持一致）
        const float borderGetLine = 1.27f;

        // 检查玩家是否在回收线之上
        if (player != null && player.transform != null && !isAutoFlying)
        {
            Transform playerTransform = player.transform;
            if (playerTransform.position.y >= borderGetLine && !isCollecting)
            {
                // 玩家在回收线之上，自动收集道具
                FlyToPlayer(playerTransform, true);
            }
        }
    }

    public void SetCollectClip(AudioClip clip)
    {
        collectClip = clip;
    }
}
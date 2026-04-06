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
    }

    void Update()
    {
        CheckInterval--;
        if(CheckInterval <= 0)
        {
            CheckInterval = 10;
            CheckPos();
            CheckRecycleLine();
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
    /// 碰撞器事件代表进入玩家收集范围
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isCollecting)
        {
            FlyToPlayer(collision.transform);
        }
    }
    /// <summary>
    /// 道具自动被玩家收取
    /// </summary>
    /// <param name="playerTransform">玩家transform</param>
    /// <param name="isRecycleLineTriggered">是否由回收线触发</param>
    private void FlyToPlayer(Transform playerTransform, bool isRecycleLineTriggered = false)
    {
        isCollecting = true;
        StartCoroutine(CollectItem(playerTransform, isRecycleLineTriggered));
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
        Vector3 initialTargetPosition = playerTransform.position;
        
        // 回收线触发时的目标位置
        Vector3 recycleLineTargetPosition = playerTransform.position;
        
        // 持续移动
        while (true)
        {
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
        // 播放收集音效
        if (Global_AudioManager.Instance != null && collectClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(collectClip,false);
        }
        
        // 销毁道具
        Global_ObjectPool.Instance.Recycle(this.gameObject);
    }
    private void Effect()
    {
        switch(itemType)
        {
            case 0:// 道具类型为HP+
                Global_GameManager.Instance.AddLeftLife(1,0);
                break;
            case 1:// 道具类型为HP
                Global_GameManager.Instance.AddLeftLife(0,1);
                break;
            case 2:// 道具类型为Power+
                Global_GameManager.Instance.AddPower(100);
                break;
            case 3:// 道具类型为Power
                Global_GameManager.Instance.AddPower(1);
                break;
            case 4:// 道具类型为Bomb+
                Global_GameManager.Instance.AddBomb(1,0);
                break;
            case 5:// 道具类型为Bomb
                Global_GameManager.Instance.AddBomb(0,1);
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
        if (player != null)
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

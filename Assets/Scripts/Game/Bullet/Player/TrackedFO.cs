using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 追踪飞行物（Tracked Flying Object）
/// 要追踪怎样的敌人实在是有些难以决定
/// 你看，无论是最近的，还是最弱的，或者优先杂鱼优先boss什么的都各有道理
/// 仔细想了想还是最近的吧
/// 毕竟敌人是有碰撞的
/// 被撞到就会死
/// 
/// 注意！由于敌人代码还没写好所以当前寻敌逻辑为空
/// </summary>
public class TrackedFO : MonoBehaviour
{
    [Header("追踪配置")]
    public float TrackSpeed ; // 追踪弹飞行速度
    private readonly float minX = -8.4f;
    private readonly float maxX = 3f;
    private readonly float minY = -5.3f;
    private readonly float maxY = 4.5f;
    private GameObject target; // 目标.transform
    private Rigidbody2D rb2D;

    void OnEnable()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("一个追踪飞行物未找到刚体");
        }
        
        FindTarget();
    }

    // Update is called once per frame
    void Update()
    {
        //CheckTarget();
        TrackedMove();
        MoveCheck();
    }

    private void CheckTarget()// 检查目标是否存在，如果当前敌人死亡（被对象池回收）则重新寻敌
    {
        if(!target.activeSelf)
        {
            FindTarget();
        }
        else
        {
            // 目标存在，继续追踪(获得其当前位置)
        }
    }

    private void FindTarget()// 寻找追踪目标
    {
        // 寻敌逻辑为空
    }

    /// <summary>
    /// 追踪飞行物的移动
    /// </summary>
    public void TrackedMove()
    {
        if (rb2D != null)
        {
            // 如果有目标，朝向目标移动
            if (target != null && target.activeSelf)
            {
                Vector2 direction = (target.transform.position - transform.position).normalized;
                rb2D.velocity = direction * TrackSpeed;
            }
            else
            {
                // 没有目标时，默认向右移动
                rb2D.velocity = TrackSpeed * Vector2.up;
            }
        }
    }

    public void MoveCheck()
    {
        if(transform.position.x<minX || transform.position.x>maxX || transform.position.y<minY || transform.position.y>maxY)
        {
            Global_ObjectPool.Instance.Recycle(this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Global_ObjectPool.Instance.Recycle(this.gameObject);
    }
}

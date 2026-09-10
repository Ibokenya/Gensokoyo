using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySystem;

/// <summary>
/// 玩家碰撞触发器
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    private Rigidbody2D rb2D;// 刚体组件
    private Vector2 moveDirection = Vector2.zero;// 移动方向
    private bool isDiagonalMove = false;// 是否为斜向移动
    // 边界值
    private readonly float minX = -8.9f;
    private readonly float maxX = 2.95f;
    private readonly float minY = -4.7f;
    private readonly float maxY = 4.5f;

    void OnEnable()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("没有找到玩家的刚体组件");
        }
        else
        {
            // 🔴 把 Rigidbody2D 设为 Kinematic —— 从 Box2D 物理模拟里彻底摘掉
            // 原因：即使我们不用 velocity，Dynamic 刚体的 gravity/碰撞仍会让 Box2D 修改位置
            // 我们用纯 transform 移动，不需要 Box2D 积分
            rb2D.bodyType = RigidbodyType2D.Kinematic;
            rb2D.gravityScale = 0f;
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
    }

    // 固定 50Hz 物理帧驱动
    void FixedUpdate()
    {
        // 确保rb2D已获取
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
            if (rb2D == null)
            {
                Debug.LogError("PlayerCollision: 找不到刚体组件！");
                return;
            }
        }
        
        // 处理不同状态
        if(Global_GameManager.Instance.state == State.Gaming || 
           Global_GameManager.Instance.state == State.NoDead ||
           Global_GameManager.Instance.state == State.SpellCard)   
        {
            // 处理边界检测
            HandleBounds();
        }
        else if(Global_GameManager.Instance.state == State.Reincarnation ||
                Global_GameManager.Instance.state == State.Frozen)
        {
            // 重生状态或冻结状态时，设置速度为0
            rb2D.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// 更新移动状态 —— 🔴 纯 transform 移动，完全绕开 Box2D velocity 积分
    /// 原因：Unity 的 Rigidbody2D velocity 积分在不同 FixedUpdate 顺序下有微小差异（浮点舍入），
    ///      会导致录 vs 回放位置逐步偏移（0.3px → 累积到致命）
    ///      transform.position += direction * speed * dt 是纯确定性的浮点运算
    /// </summary>
    public void UpdateMovement(bool leftPressed, bool rightPressed, bool upPressed, bool downPressed, float moveSpeed)
    {
        // 只有在游戏状态、无敌状态和符卡状态时才处理移动
        if(Global_GameManager.Instance.state != State.Gaming && 
           Global_GameManager.Instance.state != State.NoDead &&
           Global_GameManager.Instance.state != State.SpellCard) return;

        // 计算方向
        float horizontal = 0f;
        if (leftPressed) horizontal = -1f;
        else if (rightPressed) horizontal = 1f;

        float vertical = 0f;
        if (upPressed) vertical = 1f;
        else if (downPressed) vertical = -1f;

        moveDirection = new Vector2(horizontal, vertical);
        isDiagonalMove = (horizontal != 0f && vertical != 0f);

        float speed = moveSpeed;
        if (isDiagonalMove) speed = moveSpeed * 0.7f;

        // 🔴 纯 transform 移动 —— 确定性浮点乘法，不走 Box2D
        Vector3 pos = transform.position;
        float dt = SimClock.FixedTickDt;  // 0.02f，固定值
        float scale = Global_GameManager.Instance.GetSpeedScale();
        pos.x += moveDirection.x * speed * scale * dt;
        pos.y += moveDirection.y * speed * scale * dt;
        transform.position = pos;
    }

    /// <summary>
    /// 强行停止玩家移动
    /// 即使玩家还按着方向键，也会立即停止
    /// </summary>
    public void StopMove()
    {
        // 确保rb2D已获取
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
            if (rb2D == null)
            {
                Debug.LogError("PlayerCollision: 找不到刚体组件，无法停止移动！");
                return;
            }
        }
        
        // 将速度设为0，停止移动
        rb2D.velocity = Vector2.zero;
        moveDirection = Vector2.zero;
    }

    /// <summary>
    /// 处理边界检测
    /// </summary>
    private void HandleBounds()
    {
        // 只有在游戏状态和无敌状态时才处理边界检测
        if(Global_GameManager.Instance.state != State.Gaming && 
           Global_GameManager.Instance.state != State.NoDead &&
           Global_GameManager.Instance.state != State.SpellCard &&
           Global_GameManager.Instance.state != State.Dialog) return;
        
        // 获取当前位置
        Vector3 position = transform.position;

        // 边界检测
        if (position.x < minX)
        {
            position.x = minX;
        }
        if (position.x > maxX)
        {
            position.x = maxX;
        }
        if (position.y < minY)
        {
            position.y = minY;
        }
        if (position.y > maxY)
        {
            position.y = maxY;
        }

        // 更新位置
        transform.position = position;
    }
}

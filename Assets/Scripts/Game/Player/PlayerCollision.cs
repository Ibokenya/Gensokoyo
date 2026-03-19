using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    // Update is called once per frame
    void Update()
    {
        // 处理边界检测
        HandleBounds();
    }

    /// <summary>
    /// 更新移动状态
    /// </summary>
    /// <param name="leftPressed">左键是否按下</param>
    /// <param name="rightPressed">右键是否按下</param>
    /// <param name="upPressed">上键是否按下</param>
    /// <param name="downPressed">下键是否按下</param>
    /// <param name="moveSpeed">移动速度</param>
    public void UpdateMovement(bool leftPressed, bool rightPressed, bool upPressed, bool downPressed, float moveSpeed)
    {
        // 计算水平移动方向
        float horizontal = 0f;
        if (leftPressed)
        {
            horizontal = -1f;
        }
        else if (rightPressed)
        {
            horizontal = 1f;
        }

        // 计算垂直移动方向
        float vertical = 0f;
        if (upPressed)
        {
            vertical = 1f;
        }
        else if (downPressed)
        {
            vertical = -1f;
        }

        // 计算移动方向向量
        moveDirection = new Vector2(horizontal, vertical);

        // 检查是否为斜向移动
        isDiagonalMove = (horizontal != 0f && vertical != 0f);

        // 计算移动速度
        float speed = moveSpeed;
        if (isDiagonalMove)
        {
            // 斜向移动时速度补正（乘以根号2的倒数）
            speed = moveSpeed * 0.7f;
        }

        // 应用移动
        if (rb2D != null)
        {
            rb2D.velocity = moveDirection * speed;
        }
    }

    /// <summary>
    /// 处理边界检测
    /// </summary>
    private void HandleBounds()
    {


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

    /// <summary>
    /// 触发器检测
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 处理触发器逻辑
        Debug.Log("玩家触发: " + collision.gameObject.name);
        
    }
}

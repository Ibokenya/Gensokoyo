using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenStar : MonoBehaviour
{
    [Header("彗星参数")]
    public float initialSpeed = 5f; // 初始速度
    public float deceleration = 0.5f; // 减速度
    public float minSpeed = 0.5f; // 最小速度
    public float y1 = 3f; // 第一个速度变更点的y坐标
    public float y2 = -3f; // 第二个速度变更点的y坐标
    public float v1 = 2f; // 第一个速度常量
    public float v2 = 0.5f; // 第二个速度常量
    
    private Rigidbody2D rb2D;
    private float currentSpeed;
    private bool hasReachedY1 = false; // 是否到达y1
    private bool hasReachedY2 = false; // 是否到达y2
    
    private void OnEnable()
    {
        rb2D = GetComponent<Rigidbody2D>();
        currentSpeed = initialSpeed;
        hasReachedY1 = false;
        hasReachedY2 = false;
    }
    
    private void Update()
    {
        // 检查是否到达速度变更点
        CheckSpeedChangePoints();
        
        // 如果还没到达y1，继续减速
        if (!hasReachedY1)
        {
            currentSpeed = Mathf.Max(minSpeed, currentSpeed - deceleration * Time.deltaTime);
        }
        
        // 向下移动
        if (rb2D != null)
        {
            rb2D.velocity = new Vector2(0, -currentSpeed);
        }
        
        // 边界检测
        CheckBounds();
    }
    
    /// <summary>
    /// 检查速度变更点
    /// </summary>
    private void CheckSpeedChangePoints()
    {
        float currentY = transform.position.y;
        
        // 到达y1，速度改为v1
        if (!hasReachedY1 && currentY <= y1)
        {
            hasReachedY1 = true;
            currentSpeed = v1;
        }
        // 到达y2，速度改为v2
        else if (hasReachedY1 && !hasReachedY2 && currentY <= y2)
        {
            hasReachedY2 = true;
            currentSpeed = v2;
        }
    }
    
    /// <summary>
    /// 边界检测
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = transform.position;
        if (position.y < -10.5f) // 超出下边界
        {
            Recycle();
        }
    }
    
    /// <summary>
    /// 回收彗星
    /// </summary>
    private void Recycle()
    {
        if (Global_ObjectPool.Instance != null)
        {
            Global_ObjectPool.Instance.Recycle(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDisable()
    {
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
    }
}

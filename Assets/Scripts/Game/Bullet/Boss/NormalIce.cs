using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalIce : MonoBehaviour
{
    public float BaseSpeed = 5f;
    public bool useSpeedOffset = true; // 是否使用速度偏移
    private float actualSpeed;
    private Rigidbody2D rb2D;
    
    // 边界范围
    private readonly float minX = -12f;
    private readonly float maxX = 6f;
    private readonly float minY = -9f;
    private readonly float maxY = 9f;
    
    void Start()
    {
        // 获取Rigidbody2D组件
        rb2D = GetComponent<Rigidbody2D>();
    }
    
    public void SetSpeed(float speed)
    {
        BaseSpeed = speed;
        // 计算实际速度
        if (useSpeedOffset)
        {
            // 计算速度偏移
            float speedOffset = Mathf.Round(Random.Range(-5f, 6f)) * 0.1f;
            actualSpeed = BaseSpeed + speedOffset;
        }
        else
        {
            // 使用基础速度
            actualSpeed = BaseSpeed;
        }
        
        // 设置速度
        if (rb2D != null)
        {
            // 设置速度
            Vector2 direction = transform.TransformDirection(Vector2.right);
            rb2D.velocity = direction * actualSpeed;
        }
    }
    
    void OnEnable()
    {
        // 获取Rigidbody2D组件
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        
        // 计算实际速度
        if (useSpeedOffset)
        {
            // 计算速度偏移
            float speedOffset = Mathf.Round(Random.Range(-5f, 6f)) * 0.1f;
            actualSpeed = BaseSpeed + speedOffset;
        }
        else
        {
            // 使用基础速度
            actualSpeed = BaseSpeed;
        }
        
        // 设置速度
        if (rb2D != null)
        {
            // 设置速度
            Vector2 direction = transform.TransformDirection(Vector2.right);
            rb2D.velocity = direction * actualSpeed;
            rb2D.isKinematic = false;
        }
    }
    
    void Update()
    {
        // 检查边界
        CheckBounds();
    }
    
    /// <summary>
    /// 检查边界，超出范围则回收
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = transform.position;
        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
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
    }
}

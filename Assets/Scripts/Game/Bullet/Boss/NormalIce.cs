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
    private readonly float minX = -11f;
    private readonly float maxX = 5f;
    private readonly float minY = -7.5f;
    private readonly float maxY = 6.5f;
    
    void Start()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
    }
    
    public void SetSpeed(float speed)
    {
        BaseSpeed = speed;
        // 重新计算实际速度并更新刚体速度
        if (useSpeedOffset)
        {
            // 生成速度偏移（-0.5 到 0.5 之间的十分位值）
            float speedOffset = Mathf.Round(Random.Range(-5f, 6f)) * 0.1f;
            actualSpeed = BaseSpeed + speedOffset;
        }
        else
        {
            // 不使用速度偏移，直接使用基础速度
            actualSpeed = BaseSpeed;
        }
        
        // 更新移动速度
        if (rb2D != null)
        {
            // 根据子弹的旋转角度计算移动方向
            Vector2 direction = transform.right;
            rb2D.velocity = direction * actualSpeed;
        }
    }
    
    void OnEnable()
    {
        // 确保刚体组件存在
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        
        // 根据是否使用速度偏移计算实际速度
        if (useSpeedOffset)
        {
            // 生成速度偏移（-0.5 到 0.5 之间的十分位值）
            float speedOffset = Mathf.Round(Random.Range(-5f, 6f)) * 0.1f;
            actualSpeed = BaseSpeed + speedOffset;
        }
        else
        {
            // 不使用速度偏移，直接使用基础速度
            actualSpeed = BaseSpeed;
        }
        
        // 设置移动速度
        if (rb2D != null)
        {
            // 根据子弹的旋转角度计算移动方向
            Vector2 direction = transform.right;
            rb2D.velocity = direction * actualSpeed;
            // 确保物体不是运动学的
            rb2D.isKinematic = false;
        }
    }
    
    void Update()
    {
        // 边界检测
        CheckBounds();
    }
    
    /// <summary>
    /// 检测边界，超出边界则回收
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

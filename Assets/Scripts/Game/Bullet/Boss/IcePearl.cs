using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冰珠子弹脚本(自带旋转效果)
/// </summary>
public class IcePearl : MonoBehaviour
{
    public GameObject icePoint; // 冰点对象
    public float rotationSpeed; // 旋转速度（度/秒）
    public float moveSpeed; // 移动速度
    private Vector2 initialDirection; // 初始移动方向
    private float angle; // 当前旋转角度
    private float distance; // 距离冰点的距离
    
    // 边界范围
    private readonly float minX = -11f;
    private readonly float maxX = 5f;
    private readonly float minY = -7.5f;
    private readonly float maxY = 6.5f;
    
    void Start()
    {
        // 计算初始方向（基于当前旋转）
        initialDirection = transform.right;
        
        // 初始距离为0
        distance = 0f;
        
        // 初始角度为0
        angle = 0f;
    }
    
    void Update()
    {
        if (icePoint != null)
        {
            // 计算旋转角度
            angle += rotationSpeed * Time.deltaTime;
            
            // 计算距离（持续增加）
            distance += moveSpeed * Time.deltaTime;
            
            // 计算旋转后的方向
            Vector2 rotatedDirection = Quaternion.Euler(0, 0, angle) * initialDirection;
            
            // 计算新位置
            Vector2 newPosition = (Vector2)icePoint.transform.position + rotatedDirection * distance;
            
            // 更新位置
            transform.position = newPosition;
            
            // 更新旋转（使子弹始终朝向移动方向）
            float angleRad = Mathf.Atan2(rotatedDirection.y, rotatedDirection.x);
            float angleDeg = angleRad * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angleDeg);
        }
        
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
    
    void OnEnable()
    {
        // 当子弹被激活时，重置参数
        // 计算初始方向（基于当前旋转）
        initialDirection = transform.right;
        
        // 初始距离为0
        distance = 0f;
        
        // 初始角度为0
        angle = 0f;
    }
    
    void OnDisable()
    {
        // 当子弹被回收时，重置参数
        icePoint = null;
        rotationSpeed = 0f;
        moveSpeed = 0f;
    }
}
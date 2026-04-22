using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private Vector2 targetPosition;
    private bool hasReachedTarget = false;
    public bool IsReachedTarget { get { return hasReachedTarget; } }// 是否到达目标位置
    
    // 边界范围
    private readonly float minX = -12f;
    private readonly float maxX = 6f;
    private readonly float minY = -9f;
    private readonly float maxY = 9f;
    
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }
    
    void OnEnable()
    {
        if (rb2D == null)
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        
        hasReachedTarget = false;
        
        if (rb2D != null)
        {
            rb2D.isKinematic = false;
        }
    }
    
    void Update()
    {
        if (!hasReachedTarget && rb2D != null)
        {
            CheckTargetPosition();
        }
        
        CheckBounds();
    }
    
    public void Initialize(float targetX, float targetY)
    {
        targetPosition = new Vector2(targetX, targetY);
        
        transform.position = new Vector2(targetX, 7f);
        
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.isKinematic = false;
        }
        
        hasReachedTarget = false;
    }
    
    private void CheckTargetPosition()
    {
        Vector2 currentPos = transform.position;
        
        if (currentPos.y <= targetPosition.y)
        {
            hasReachedTarget = true;
            
            if (rb2D != null)
            {
                rb2D.velocity = Vector2.zero;
                rb2D.isKinematic = true;
            }
            
            transform.position = targetPosition;
        }
    }
    
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
    
    void OnDisable()
    {
        hasReachedTarget = false;
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
    }
}

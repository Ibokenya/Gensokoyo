using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpike : MonoBehaviour
{
    private Rigidbody2D rb2D;
    
    // 边界范围
    private readonly float minY = -6f;
    private readonly float maxY = 6f;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            // 设置为自由下落
            rb2D.gravityScale = 1f;
        }
    }

    void Update()
    {
        CheckBounds();
    }
    
    /// <summary>
    /// 检查边界，超出范围则回收
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = transform.position;
        if (position.y < minY || position.y > maxY)
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
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
    }
}

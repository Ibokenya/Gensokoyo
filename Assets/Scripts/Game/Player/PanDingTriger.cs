using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanDingTriger : MonoBehaviour
{
    private Rigidbody2D rb2D;// 刚体组件

    void OnEnable()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("没有找到判定点的刚体组件");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 处理触发器逻辑
        
    }
}

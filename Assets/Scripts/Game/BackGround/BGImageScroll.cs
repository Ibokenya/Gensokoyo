using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGImageScroll : MonoBehaviour
{
    [Header("滚动配置")]
    [Tooltip("滚动速度（X=水平，Y=垂直）")]
    public Vector2 scrollSpeed = new Vector2(0f, 0.5f); // 竖直滚动示例

    private SpriteRenderer spriteRenderer;
    private Vector2 Offset;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // 计算每帧偏移量（方向*速度*时间，保证帧率无关）
        Vector2 deltaOffset = scrollSpeed * Time.deltaTime;
        // 累加偏移量
        Offset += deltaOffset;
        // 关键：循环偏移（超过1个平铺单位则重置，实现无缝）
        Offset = new Vector2(
            Mathf.Repeat(Offset.x, 1f), // X轴循环
            Mathf.Repeat(Offset.y, 1f)  // Y轴循环
        );

        // 应用偏移到背景纹理
        spriteRenderer.material.mainTextureOffset = Offset;
    }
}

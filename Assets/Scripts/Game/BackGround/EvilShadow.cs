using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 恶魔阴影脚本
/// 负责处理屏幕变黑特效的旋转和透明度变化
/// </summary>
public class EvilShadow : MonoBehaviour
{
    public float maxRotationSpeed = 180f; // 最大旋转速度（度/秒）
    public float targetAlpha = 0.8f; // 目标透明度
    public float fadeInDuration = 6f; // 淡入时间（6秒）
    public float fadeOutDuration = 3f; // 基础淡出时间（0.8透明度需要3秒）
    
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // 初始化 SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // 旋转逻辑，转速与透明度成正比
        if (spriteRenderer != null)
        {
            float alpha = spriteRenderer.color.a;
            float rotationSpeed = maxRotationSpeed * alpha;
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
    
    void OnDisable()
    {
        // 重置状态
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
    }
    
    /// <summary>
    /// 开始淡入
    /// </summary>
    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// 淡入协程
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        originalColor.a = 0f;
        spriteRenderer.color = originalColor;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            originalColor.a = alpha * targetAlpha; // 最终透明度为 targetAlpha
            spriteRenderer.color = originalColor;
            yield return null;
        }
        // 确保最终透明度为 targetAlpha
        originalColor.a = targetAlpha;
        spriteRenderer.color = originalColor;
    }
    
    /// <summary>
    /// 开始淡出
    /// </summary>
    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// 淡出协程
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        float currentAlpha = originalColor.a;
        
        // 动态计算淡出时间：根据当前透明度计算
        // 0.8透明度需要3秒，0.4透明度需要0.4*3/0.8 = 1.5秒
        float dynamicFadeOutDuration = (currentAlpha / targetAlpha) * fadeOutDuration;
        
        while (elapsedTime < dynamicFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - elapsedTime / dynamicFadeOutDuration);
            originalColor.a = alpha * currentAlpha; // 从当前透明度淡出到0
            spriteRenderer.color = originalColor;
            yield return null;
        }
        
        // 确保最终透明度为0并禁用
        originalColor.a = 0f;
        spriteRenderer.color = originalColor;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsRotate : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>(); // 存储所有符卡子物体
    public float fadeInDuration = 0.5f; // 淡入持续时间
    public float fadeOutDuration = 0.3f; // 淡出持续时间

    public float maxAlpha = 0.8f; // 最大透明度
    public float rotationSpeed = 120f; // 旋转速度（度/秒）
    public Transform targetTransform; // 围绕旋转的目标物体（SpellCardEffect所在物体）
    private float fadeTimer = 0f;

    void OnEnable()
    {
        fadeTimer = 0f;
        
        // 初始化所有符卡的透明度为0
        foreach (var card in cards)
        {
            if (card != null)
            {
                SetCardAlpha(card, 0f);
            }
        }
        
        // 开始淡入协程
        StartCoroutine(FadeInCoroutine());
    }

    void Update()
    {
        // 让每个实际符卡物体围绕目标物体旋转
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card != null)
            {
                // 每个符卡根据自身索引，将旋转速度添加index*10
                float adjustedSpeed = rotationSpeed + (i * 10);
                
                if (targetTransform != null)
                {
                    card.transform.RotateAround(targetTransform.position, Vector3.up, adjustedSpeed * Time.deltaTime);
                }
                else
                {
                    Debug.Log("没有目标物体");
                }
            }
        }
    }

    /// <summary>
    /// 淡入协程
    /// </summary>
    private IEnumerator FadeInCoroutine()
    {
        while (fadeTimer < fadeInDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, fadeTimer / fadeInDuration);
            
            foreach (var card in cards)
            {
                if (card != null)
                {
                    SetCardAlpha(card, alpha);
                }
            }
            
            yield return null;
        }
        
        // 确保所有符卡达到最大透明度
        foreach (var card in cards)
        {
            if (card != null)
            {
                SetCardAlpha(card, maxAlpha);
            }
        }
    }

    /// <summary>
    /// 淡出方法（供动画事件调用）
    /// </summary>
    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    /// <summary>
    /// 淡出协程
    /// </summary>
    private IEnumerator FadeOutCoroutine()
    {
        float fadeOutTimer = 0f;
        while (fadeOutTimer < fadeOutDuration)
        {
            fadeOutTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(maxAlpha, 0f, fadeOutTimer / fadeOutDuration);
            
            foreach (var card in cards)
            {
                if (card != null)
                {
                    SetCardAlpha(card, alpha);
                }
            }
            
            yield return null;
        }
        
        // 确保所有符卡透明度为0
        foreach (var card in cards)
        {
            if (card != null)
            {
                SetCardAlpha(card, 0f);
            }
        }
    }

    /// <summary>
    /// 设置符卡的透明度
    /// </summary>
    private void SetCardAlpha(GameObject card, float alpha)
    {
        // 获取符卡的所有渲染器组件
        Renderer[] renderers = card.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                foreach (var material in renderer.materials)
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
            }
        }
        
        // 处理SpriteRenderer
        SpriteRenderer spriteRenderer = card.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}

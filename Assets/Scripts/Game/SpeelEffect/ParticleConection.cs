using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleConection : MonoBehaviour
{
    [Header("质点设置")]
    public List<Transform> particles; // 存储12个质点的Transform
    public Sprite secondSprite; // 存储每个质点的第二姿态图片
    
    [Header("连线设置")]
    public LineRenderer[] lineRenderers; // 存储所有连线的LineRenderer
    public int totalFrames; // 连线期间总时长（帧）
    
    [Header("连线音效")]
    public AudioClip lineClip; // 连线音效clip

    private int currentIndex = 0; // 当前质点索引
    private int nextIndex = 1; // 下一个质点索引
    private readonly int maxLines = 11; // 需要创建的连线数量
    private int createdLines = 0; // 已创建的连线数量
    
    /// <summary>
    /// 开始创建连线
    /// 由动画事件调用
    /// </summary>
    public void StartCreateLines()
    {
        currentIndex = 0;
        nextIndex = 1;
        createdLines = 0;
        int framesPerLine = totalFrames / maxLines; // 每根连线的帧数
        StartCoroutine(CreateLinesCoroutine(framesPerLine));
    }
    
    /// <summary>
    /// 创建连线的协程
    /// </summary>
    /// <param name="framesPerLine">每根连线的帧数</param>
    private IEnumerator CreateLinesCoroutine(int framesPerLine)
    {
        while (createdLines < maxLines && currentIndex < particles.Count && nextIndex < particles.Count && createdLines < lineRenderers.Length)
        {
            // 显示连线
            StartCoroutine(ShowLine(lineRenderers[createdLines], particles[currentIndex], particles[nextIndex], framesPerLine));
            
            // 增加索引
            currentIndex++;
            nextIndex++;
            createdLines++;
            
            // 立即创建下一条连线，无间隔
            yield return null;
        }
    }
    
    /// <summary>
    /// 显示连线，从起点缓缓画到终点
    /// </summary>
    /// <param name="lineRenderer">连线的LineRenderer</param>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <param name="frames">绘制连线的帧数</param>
    private IEnumerator ShowLine(LineRenderer lineRenderer, Transform start, Transform end, int frames)
    {
        if (lineRenderer == null || start == null || end == null)
            yield break;
        
        // 启用连线
        lineRenderer.gameObject.SetActive(true);
        lineRenderer.gameObject.GetComponent<LineRenderer>().material.color = Color.white;
        
        // 添加偏移量
        Vector3 offset = new Vector3(3, 0, 0);
        
        // 设置初始位置
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start.position + offset);
        lineRenderer.SetPosition(1, start.position + offset);
        
        // 在指定帧数内绘制连线
        for (int i = 0; i <= frames; i++)
        {
            float t = (float)i / frames;
            Vector3 currentPosition = Vector3.Lerp(start.position + offset, end.position + offset, t);
            lineRenderer.SetPosition(1, currentPosition);
            yield return null;
        }
        
        // 播放连线音效
        if (lineClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(lineClip);
        }
        
        // 确保到达终点
        lineRenderer.SetPosition(1, end.position + offset);
        
        // 切换目标质点为第二姿态
        if (end != null && secondSprite != null)
        {
            int particleIndex = particles.IndexOf(end);
            if (particleIndex >= 0)
            {
                SpriteRenderer spriteRenderer = end.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = secondSprite;
                }
            }
        }
    }
    
    /// <summary>
    /// 淡出所有连线和质点
    /// 在0.5秒内将所有连线和质点淡出
    /// </summary>
    public void ClearLines()
    {
        StartCoroutine(FadeOutCoroutine());
    }
    
    /// <summary>
    /// 淡出效果的协程
    /// </summary>
    private IEnumerator FadeOutCoroutine()
    {
        // 淡出所有连线
        float duration = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            
            // 淡出连线
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                if (lineRenderer != null && lineRenderer.gameObject.activeInHierarchy)
                {
                    // 淡出效果
                    Color color = lineRenderer.material.color;
                    color.a = 1f - t;
                    lineRenderer.material.color = color;
                }
            }
            
            // 淡出质点
            foreach (Transform particle in particles)
            {
                if (particle != null)
                {
                    SpriteRenderer spriteRenderer = particle.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        Color color = spriteRenderer.color;
                        color.a = 1f - t;
                        spriteRenderer.color = color;
                    }
                }
            }
            
            yield return null;
        }
    }
}
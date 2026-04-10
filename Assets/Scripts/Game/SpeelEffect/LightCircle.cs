using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 光圈管理器脚本
/// 管理4个光圈的动画和移动
/// </summary>
public class LightCircle : MonoBehaviour
{
    [Header("基本设置")]
    public GameObject player; // 玩家物体引用
    public List<Sprite> circleSprites; // 光圈旋转帧动画
    public List<GameObject> lightCircles; // 4个光圈子物体
    public GameObject laser; // 激光物体
    public float moveSpeed = 0.5f; // 固定移动速度（每秒）
    
    [Header("光圈配置")]
    public List<CircleConfig> circleConfigs; // 每个光圈的配置
    
    private int circleIndex = 0; // 当前激活的光圈索引
    private bool[] isCircleActive; // 记录每个光圈是否已激活
    private Dictionary<GameObject, Coroutine> rotatingCoroutines = new Dictionary<GameObject, Coroutine>(); // 存储每个光圈的旋转协程
    
    /// <summary>
    /// 光圈配置类
    /// </summary>
    [System.Serializable]
    public class CircleConfig
    {
        public Vector3 startPosition; // 起始坐标（相对于父对象）
        public Vector3 endPosition; // 终点坐标（相对于父对象）
        public float moveTime; // 位移时间
        public float animationSpeed; // 帧动画速度
    }
    
    private void Start()
    {
        // 初始化
        if (lightCircles != null)
        {
            isCircleActive = new bool[lightCircles.Count];
        }
        else
        {
            lightCircles = new List<GameObject>();
            isCircleActive = new bool[0];
        }
    }
    
    private void Update()
    {
        // 每帧同步位置到玩家
        SyncPositionsToPlayer();
    }
    
    /// <summary>
    /// 同步位置到玩家
    /// </summary>
    private void SyncPositionsToPlayer()
    {
        if (player == null)
            return;
        
        // 同步光圈位置 - 保持x轴与玩家同步
        foreach (var circle in lightCircles)
        {
            if (circle != null && circle.activeInHierarchy)
            {
                // 计算当前相对位置（只保留y轴）
                float relativeY = circle.transform.position.y - player.transform.position.y;
                // 设置新位置（保持y轴不变，x轴与玩家同步）
                circle.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + relativeY, player.transform.position.z);
            }
        }
        
        // 同步激光位置 - 世界坐标系中激光需要在player上方5.48f处
        if (laser != null)
        {
            laser.transform.position = player.transform.position + new Vector3(0, 5.48f, 0);
        }
    }
    
    /// <summary>
    /// 激活光圈
    /// 由动画事件调用
    /// </summary>
    public void ActivateCircle()
    {
        if (lightCircles == null || circleConfigs == null)
            return;
        
        if (circleIndex < lightCircles.Count && circleIndex < circleConfigs.Count)
        {
            // 激活对应索引的光圈
            GameObject circle = lightCircles[circleIndex];
            if (circle != null)
            {
                circle.SetActive(true);
                isCircleActive[circleIndex] = true;
                
                // 开始执行位移
                StartCoroutine(MoveCircle(circleIndex));
                
                // 开始旋转动画
                StartRotateAnimation(circle, circleIndex);
            }
            
            // 索引+1
            circleIndex++;
        }
    }
    
    /// <summary>
    /// 开始旋转动画
    /// </summary>
    /// <param name="circle">光圈物体</param>
    /// <param name="index">光圈索引</param>
    private void StartRotateAnimation(GameObject circle, int index)
    {
        if (circleSprites == null || circleSprites.Count == 0)
            return;
        
        // 停止之前的旋转协程
        if (rotatingCoroutines.ContainsKey(circle))
        {
            StopCoroutine(rotatingCoroutines[circle]);
            rotatingCoroutines.Remove(circle);
        }
        
        // 开始新的旋转协程
        Coroutine coroutine = StartCoroutine(RotateAnimation(circle, index));
        rotatingCoroutines[circle] = coroutine;
    }
    
    /// <summary>
    /// 旋转动画协程
    /// </summary>
    /// <param name="circle">光圈物体</param>
    /// <param name="index">光圈索引</param>
    private IEnumerator RotateAnimation(GameObject circle, int index)
    {
        if (circle == null || circleConfigs == null || index >= circleConfigs.Count)
            yield break;
        
        SpriteRenderer spriteRenderer = circle.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || circleSprites == null || circleSprites.Count == 0)
            yield break;
        
        CircleConfig config = circleConfigs[index];
        int currentSpriteIndex = 0;
        
        while (isCircleActive != null && index < isCircleActive.Length && isCircleActive[index])
        {
            // 设置当前帧
            spriteRenderer.sprite = circleSprites[currentSpriteIndex];
            
            // 计算下一帧
            currentSpriteIndex = (currentSpriteIndex + 1) % circleSprites.Count;
            
            // 根据速度等待
            yield return new WaitForSeconds(1f / config.animationSpeed);
        }
    }
    
    /// <summary>
    /// 移动光圈
    /// </summary>
    /// <param name="index">光圈索引</param>
    private IEnumerator MoveCircle(int index)
    {
        if (lightCircles == null || circleConfigs == null || index >= lightCircles.Count || index >= circleConfigs.Count)
            yield break;
        
        GameObject circle = lightCircles[index];
        CircleConfig config = circleConfigs[index];
        
        if (circle == null)
            yield break;
        
        // 开始移动
        float elapsedTime = 0f;
        while (elapsedTime < config.moveTime && isCircleActive != null && index < isCircleActive.Length && isCircleActive[index])
        {
            float t = elapsedTime / config.moveTime;
            
            // 计算本地位置
            Vector3 localPosition = Vector3.Lerp(config.startPosition, config.endPosition, t);
            
            // 转换为世界位置 - 直接以玩家为基准
            if (player != null)
            {
                circle.transform.position = player.transform.position + localPosition;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保到达终点
        if (isCircleActive != null && index < isCircleActive.Length && isCircleActive[index])
        {
            if (player != null)
            {
                circle.transform.position = player.transform.position + config.endPosition;
            }
        }
        
        // 开始以固定速度向上移动
        while (isCircleActive != null && index < isCircleActive.Length && isCircleActive[index])
        {
            // 计算向上移动的距离
            float moveDistance = moveSpeed * Time.deltaTime;
            
            // 向上移动
            circle.transform.position += new Vector3(0, moveDistance, 0);
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 重置所有光圈
    /// </summary>
    public void ResetCircles()
    {
        // 停止所有旋转协程
        foreach (var coroutine in rotatingCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        rotatingCoroutines.Clear();
        
        circleIndex = 0;
        
        if (lightCircles != null)
        {
            for (int i = 0; i < lightCircles.Count; i++)
            {
                if (lightCircles[i] != null)
                {
                    lightCircles[i].SetActive(false);
                    if (isCircleActive != null && i < isCircleActive.Length)
                    {
                        isCircleActive[i] = false;
                    }
                }
            }
        }
    }
}
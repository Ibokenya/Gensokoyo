using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReplaySystem;

public enum BossAnimeType
{
    Idle,
    Left,
    Right
}

public class BossAnime : MonoBehaviour
{
    public BossBeheve bossBeheve;
    public Animator ChrinoAnimator;// 琪露诺动画
    public Animator CircleAnimator;// 虹人环动画
    [Header("琪露诺的帧动画")]
    public List<Sprite> sprites;// 琪露诺的帧动画
    private int CurrentAnimeIndex =0;
    private float TimeClock =0;
    private const int AnimeSpeed = 12;// 每秒12帧
    private SpriteRenderer spriteRenderer;
    [Header("琪露诺的相关物体")]
    public GameObject HP;// 琪露诺的血条
    public GameObject Mark;// 琪露诺的标记物

    [Header("RT 渲染管线 —— 必需引用")]
    public RawImage gameRTDisplay;

    private BossAnimeType currentState = BossAnimeType.Idle;

    // 缓存引用（OnEnable 中初始化，避免每帧 GetComponent）
    private RectTransform _hpRect;
    private RectTransform _rtRawImageRect;
    private Camera _rtCamera;        // 渲染到 RT 的那个摄像机

    void OnEnable()
    {
        ChrinoAnimator.SetBool("IsAppear", true);
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetState(BossAnimeType.Idle);

        // ---- 缓存坐标转换所需引用 ----
        if (HP != null) _hpRect = HP.GetComponent<RectTransform>();

        // RawImage 引用：优先用 Inspector 拖的 gameRTDisplay；否则尝试自动查找
        if (gameRTDisplay != null)
        {
            _rtRawImageRect = gameRTDisplay.rectTransform;
        }
        else
        {
            // 兜底：找 CameraRTAdapter 上的 displayImage
            var adapter = FindObjectOfType<CameraRTAdapter>();
            if (adapter != null && adapter.displayImage != null)
            {
                gameRTDisplay = adapter.displayImage;
                _rtRawImageRect = gameRTDisplay.rectTransform;
            }
            else
            {
                Debug.LogWarning("[BossAnime] gameRTDisplay 未设置且 CameraRTAdapter 也没找到 —— 血条位置可能不准！");
            }
        }

        // 渲染到 RT 的摄像机 = Camera.main（Game1 场景的主摄像机）
        _rtCamera = Camera.main;
    }

    void FixedUpdate()
    {
        // 处理动画
        if(Global_GameManager.Instance.state == State.Pause ||
           Global_GameManager.Instance.state == State.TimeStop)
        {
            return;
        }
        PlayAnime();
    }
    
    void LateUpdate()
    {
        // 更新血条位置，将世界坐标转换为UI坐标
        // 使用LateUpdate确保在所有Update执行完毕后执行，且不受时间缩放影响
        UpdateHPBarPosition();
        UpdateMarkPosition();
    }
    
    /// <summary>
    /// 更新血条位置 —— RT + RawImage 管线专用
    /// 
    /// 正确坐标链路：
    ///   1. Boss 世界坐标 → 渲染 RT 的摄像机 → RT 内部 UV (0~1, 0~1)
    ///   2. RT UV → RawImage 的世界空间矩形四角 → 双线性插值
    ///   3. 结果 = HP 应该放在屏幕世界坐标的位置
    /// </summary>
    private void UpdateHPBarPosition()
    {
        if (_hpRect == null || _rtRawImageRect == null || _rtCamera == null) return;

        // 步骤1：Boss 世界坐标 → RT 内部的归一化视口坐标 (0~1, 0~1)
        Vector3 viewportPoint = _rtCamera.WorldToViewportPoint(transform.position);
        float vx = Mathf.Clamp01(viewportPoint.x);
        float vy = Mathf.Clamp01(viewportPoint.y);

        // 步骤2：RawImage 在世界空间中的四个角点（GetWorldCorners 自动处理
        //   Canvas scaleFactor / 锚点 / pivot / 父物体变换 —— 一步到位）
        Vector3[] corners = new Vector3[4];
        _rtRawImageRect.GetWorldCorners(corners);
        // Unity 约定：corners[0]=左下, [1]=左上, [2]=右上, [3]=右下

        // 步骤3：用 RT UV 在四角之间双线性插值 → HP 应在的世界坐标
        Vector3 bottomEdge = Vector3.Lerp(corners[0], corners[3], vx); // 左→右 at y=0
        Vector3 topEdge    = Vector3.Lerp(corners[1], corners[2], vx); // 左→右 at y=1
        Vector3 worldPos   = Vector3.Lerp(bottomEdge, topEdge, vy);    // 下→上 at x=vx

        _hpRect.position = worldPos;
    }

    /// <summary>
    /// 更新标记物相对位置，不受时间缩放影响
    /// </summary>
    private void UpdateMarkPosition()
    {
        Vector3 markPos = Mark.transform.position;
        markPos.x = transform.position.x;
        Mark.transform.position = markPos;
    }
     
    private void PlayAnime()
    {
        TimeClock += SimClock.FixedTickDt;
        if(TimeClock >= 1f/AnimeSpeed)
        {
            TimeClock = 0;
            
            // 根据当前状态更新帧索引
            switch(currentState)
            {
                case BossAnimeType.Idle:
                    CurrentAnimeIndex = (CurrentAnimeIndex + 1) % 4; // 0-3帧循环
                    break;
                case BossAnimeType.Right:
                    CurrentAnimeIndex = 4 + (CurrentAnimeIndex - 4 + 1) % 4; // 4-7帧循环
                    break;
                case BossAnimeType.Left:
                    CurrentAnimeIndex = 8 + (CurrentAnimeIndex - 8 + 1) % 4; // 8-11帧循环
                    break;
            }
            
            // 更新精灵
            if(CurrentAnimeIndex < sprites.Count)
            {
                spriteRenderer.sprite = sprites[CurrentAnimeIndex];
            }
        }
    }
    
    /// <summary>
    /// 设置Boss的动画状态
    /// </summary>
    /// <param name="newState">新的状态</param>
    public void SetState(BossAnimeType newState)
    {
        if(currentState != newState)
        {
            currentState = newState;
            
            // 切换状态时，将帧索引重置为对应状态的第一张帧图片
            switch(newState)
            {
                case BossAnimeType.Idle:
                    CurrentAnimeIndex = 0;
                    break;
                case BossAnimeType.Right:
                    CurrentAnimeIndex = 4;
                    break;
                case BossAnimeType.Left:
                    CurrentAnimeIndex = 8;
                    break;
            }
            
            // 立即更新精灵
            if(CurrentAnimeIndex < sprites.Count)
            {
                spriteRenderer.sprite = sprites[CurrentAnimeIndex];
            }
        }
    }

    public void SetLeft()
    {
        SetState(BossAnimeType.Left);
    }

    public void SetRight()
    {
        SetState(BossAnimeType.Right);
    }
    
    public void SetIdle()
    {
        SetState(BossAnimeType.Idle);
    }

    public void ShowHP()
    {
        HP.SetActive(true);
        StartCoroutine(SmoothHPFill());
    }
    
    /// <summary>
    /// 平滑填充血条协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator SmoothHPFill()
    {
        if (HP != null)
        {
            Image hpImage = HP.GetComponent<Image>();
            if (hpImage != null)
            {
                float duration = 1f;
                float elapsedTime = 0f;
                float startFill = 0f;
                float targetFill = 1f;
                
                while (elapsedTime < duration)
                {
                    float t = elapsedTime / duration;
                    float fillAmount = Mathf.Lerp(startFill, targetFill, t);
                    hpImage.fillAmount = fillAmount;
                    
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return null;
                }
                
                // 确保最终填充度为1
                hpImage.fillAmount = targetFill;
            }
        }
    }

    public void HideHP()
    {
        HP.SetActive(false);
    }
    
    /// <summary>
    /// 设置血条填充比例
    /// </summary>
    /// <param name="currenthp">当前血量</param>
    /// <param name="maxhp">最大血量</param>
    public void SetHpBar(float currenthp, float maxhp)
    {
        if(HP != null)
        {
            Image hpImage = HP.GetComponent<Image>();
            if(hpImage != null)
            {
                // 计算血量比例，确保在0-1之间
                float fillAmount = Mathf.Clamp01(currenthp / maxhp);
                // 设置填充总数
                hpImage.fillAmount = fillAmount;
            }
        }
    }

    public void PlayShowAnime()
    {
        ChrinoAnimator.SetBool("IsAppear", true);
    }

    public void PlayAroundAnime()
    {
        ChrinoAnimator.SetBool("IsAround", true);
    }

    public void PlayRotateAnime()
    {
        ChrinoAnimator.enabled = false;
        CircleAnimator.SetBool("IsShow", true);
        CircleAnimator.SetBool("IsRotate", true);
    }
    
    /// <summary>
    /// 隐藏Boss方法
    /// 在1秒内将Boss对象的透明度平滑淡出为0.5f，淡出完成后隐藏血条
    /// </summary>
    public void Conceal()
    {
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(ConcealCoroutine());
    }
    
    private IEnumerator ConcealCoroutine()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        
        if (sprite != null)
        {
            Color startColor = sprite.color;
            Color targetColor = startColor;
            targetColor.a = 0.5f;
            
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                sprite.color = Color.Lerp(startColor, targetColor, t);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            
            sprite.color = targetColor;
        }
        
        // 淡出完成后隐藏血条
        HideHP();
    }

    public void OnDieEnd()
    {
        bossBeheve.OnDieEnd();
    }
}

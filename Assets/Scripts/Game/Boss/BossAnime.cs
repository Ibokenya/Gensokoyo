using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BossAnimeType
{
    Idle,
    Left,
    Right
}

public class BossAnime : MonoBehaviour
{
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
    
    private BossAnimeType currentState = BossAnimeType.Idle;

    void OnEnable()
    {
        ChrinoAnimator.SetBool("IsAppear", true);
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetState(BossAnimeType.Idle);
    }

    void Update()
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
    }
    
    /// <summary>
    /// 更新血条相对位置，不受时间缩放影响
    /// </summary>
    private void UpdateHPBarPosition()
    {
        if(HP != null)
        {
            // 获取主相机
            Camera mainCamera = Camera.main;
            if(mainCamera != null)
            {
                // 将boss的世界坐标转换为屏幕坐标
                Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
                
                // 获取血条所在的Canvas
                Canvas canvas = HP.GetComponentInParent<Canvas>();
                if(canvas != null)
                {
                    // 将屏幕坐标转换为Canvas局部坐标
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                    Vector2 localPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle
                    (canvasRect, screenPos, canvas.worldCamera, out localPos);
                    
                    // 设置血条的局部坐标
                    HP.GetComponent<RectTransform>().localPosition = localPos;
                }
            }
        }
    }
     
    private void PlayAnime()
    {
        TimeClock += Time.deltaTime;
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
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            sprite.color = targetColor;
        }
        
        // 淡出完成后隐藏血条
        HideHP();
    }
}

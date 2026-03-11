using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum AnimeType
{
    Idle,
    Left,
    Right,
}

public class PlayerAnime : MonoBehaviour
{

    public List<Sprite> ReimuIdleSprites;
    public List<Sprite> ReimuLeftSprites;
    public List<Sprite> ReimuRightSprites;
    public List<Sprite> MarisaIdleSprites;
    public List<Sprite> MarisaLeftSprites;
    public List<Sprite> MarisaRightSprites;
    [SerializeField]
    private List<Sprite> IdleSprites;
    [SerializeField]
    private List<Sprite> LeftSprites;
    [SerializeField]
    private List<Sprite> RightSprites;

    [SerializeField]    
    private int _currentIndex = 0;// 动画索引
    private float TimeClock;// 时钟，用来记录过了多长时间

    [Header("动画速度（每隔多少帧切换一次动画）")]
    public int AnimeSpeed = 5;// 每隔多少帧切换一次动画

    private AnimeType _currentAnimeType = AnimeType.Idle;// 当前动画类型

    private SpriteRenderer spriteRenderer;// 精灵渲染器组件

    private bool leftKeyPressed = false;// 左键是否按下
    private bool rightKeyPressed = false;// 右键是否按下

    void OnEnable()
    {
        if(Global_GameManager.Instance.character == Character.Reimu)
        {
            IdleSprites = ReimuIdleSprites;
            LeftSprites = ReimuLeftSprites;
            RightSprites = ReimuRightSprites;
        }
        else
        {
            IdleSprites = MarisaIdleSprites;
            LeftSprites = MarisaLeftSprites;
            RightSprites = MarisaRightSprites;
        }
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 初始化显示第一帧
        spriteRenderer.sprite = IdleSprites[0];
    }

    // Update is called once per frame
    void Update()
    {
        // 检测左键状态
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (!leftKeyPressed)
            {
                // 左键刚按下，切换到左移动状态
                SetLeftAnime();
                leftKeyPressed = true;
            }
        }
        else if (leftKeyPressed)
        {
            // 左键抬起，检查右键是否仍然按下
            leftKeyPressed = false;
            if (rightKeyPressed)
            {
                // 右键仍然按下，切换到右移动状态
                SetRightAnime();
            }
            else
            {
                // 没有按键按下，恢复 Idle 状态
                SetIdleAnime();
            }
        }

        // 检测右键状态
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (!rightKeyPressed)
            {
                // 右键刚按下，切换到右移动状态
                SetRightAnime();
                rightKeyPressed = true;
            }
        }
        else if (rightKeyPressed)
        {
            // 右键抬起，检查左键是否仍然按下
            rightKeyPressed = false;
            if (leftKeyPressed)
            {
                // 左键仍然按下，切换到左移动状态
                SetLeftAnime();
            }
            else
            {
                // 没有按键按下，恢复 Idle 状态
                SetIdleAnime();
            }
        }

        // 根据当前动画类型播放对应动画
        switch (_currentAnimeType)
        {
            case AnimeType.Idle:
                PlayIdleAnime();
                break;
            case AnimeType.Left:
                PlayLeftAnime();
                break;
            case AnimeType.Right:
                PlayRightAnime();
                break;
        }
    }

    /// <summary>
    /// 播放Idle动画
    /// 按帧检测时间，每隔AnimeSpeed帧切换一次动画
    /// </summary>
    private void PlayIdleAnime()
    {
        // 增加时钟计数
        TimeClock += Time.deltaTime;

        // 计算当前应该显示的帧数
        // 假设游戏运行在60帧，Time.deltaTime约为1/60秒
        // 我们使用帧数来控制动画速度
        int currentFrame = Mathf.FloorToInt(TimeClock * 60f);

        // 检查是否需要切换动画帧
        if (currentFrame >= AnimeSpeed)
        {
            // 切换到下一帧
            _currentIndex = (_currentIndex + 1) % IdleSprites.Count;
            
            // 更新精灵
            if (spriteRenderer != null && IdleSprites.Count > 0)
            {
                spriteRenderer.sprite = IdleSprites[_currentIndex];
            }

            // 重置时钟，保留余数以保持动画流畅
            TimeClock -= (float)AnimeSpeed / 60f;
        }
    }

    /// <summary>
    /// 播放左移动画
    /// 按帧检测时间，每隔AnimeSpeed帧切换一次动画
    /// </summary>
    private void PlayLeftAnime()
    {
        // 增加时钟计数
        TimeClock += Time.deltaTime;

        // 计算当前应该显示的帧数
        int currentFrame = Mathf.FloorToInt(TimeClock * 60f);

        // 检查是否需要切换动画帧
        if (currentFrame >= AnimeSpeed)
        {
            // 切换到下一帧
            _currentIndex ++;
            if(_currentIndex >= LeftSprites.Count)
            {
                _currentIndex = LeftSprites.Count - 3;
            }
            spriteRenderer.sprite = LeftSprites[_currentIndex];
            // 重置时钟，保留余数以保持动画流畅
            TimeClock -= (float)AnimeSpeed / 60f;
        }
    }

    /// <summary>
    /// 播放右移动画
    /// 按帧检测时间，每隔AnimeSpeed帧切换一次动画
    /// </summary>
    private void PlayRightAnime()
    {
        // 增加时钟计数
        TimeClock += Time.deltaTime;

        // 计算当前应该显示的帧数
        int currentFrame = Mathf.FloorToInt(TimeClock * 60f);

        // 检查是否需要切换动画帧
        if (currentFrame >= AnimeSpeed)
        {
            // 切换到下一帧
            _currentIndex ++;
            if(_currentIndex >= RightSprites.Count)
            {
                _currentIndex = RightSprites.Count - 3;
            }
            spriteRenderer.sprite = RightSprites[_currentIndex];
            // 重置时钟，保留余数以保持动画流畅
            TimeClock -= (float)AnimeSpeed / 60f;
        }
    }

    /// <summary>
    /// 设置Idle动画（供外部调用切换到Idle状态）
    /// </summary>
    public void SetIdleAnime()
    {
        // 如果当前不是Idle状态，切换到Idle状态
        if (_currentAnimeType != AnimeType.Idle)
        {
            _currentAnimeType = AnimeType.Idle;
            _currentIndex = 0;// 重置动画索引
            TimeClock = 0f;// 重置时钟
            
            // 立即显示第一帧
            if (spriteRenderer != null && IdleSprites.Count > 0)
            {
                spriteRenderer.sprite = IdleSprites[0];
            }
        }
    }

    /// <summary>
    /// 设置左移动画（供外部调用切换到Left状态）
    /// </summary>
    public void SetLeftAnime()
    {
        // 检查LeftSprites列表是否为空
        if (LeftSprites == null || LeftSprites.Count == 0)
        {
            Debug.LogWarning("PlayerAnime: LeftSprites列表为空，无法切换到Left状态！");
            return;
        }

        // 如果当前不是Left状态，切换到Left状态
        if (_currentAnimeType != AnimeType.Left)
        {
            _currentAnimeType = AnimeType.Left;
            _currentIndex = 0;// 重置动画索引
            TimeClock = 0f;// 重置时钟
            
            // 立即显示第一帧
            if (spriteRenderer != null && LeftSprites.Count > 0)
            {
                spriteRenderer.sprite = LeftSprites[0];
            }
        }
    }

    /// <summary>
    /// 设置右移动画（供外部调用切换到Right状态）
    /// </summary>
    public void SetRightAnime()
    {
        // 检查RightSprites列表是否为空
        if (RightSprites == null || RightSprites.Count == 0)
        {
            Debug.LogWarning("PlayerAnime: RightSprites列表为空，无法切换到Right状态！");
            return;
        }

        // 如果当前不是Right状态，切换到Right状态
        if (_currentAnimeType != AnimeType.Right)
        {
            _currentAnimeType = AnimeType.Right;
            _currentIndex = 0;// 重置动画索引
            TimeClock = 0f;// 重置时钟
            
            // 立即显示第一帧
            if (spriteRenderer != null && RightSprites.Count > 0)
            {
                spriteRenderer.sprite = RightSprites[0];
            }
        }
    }
}

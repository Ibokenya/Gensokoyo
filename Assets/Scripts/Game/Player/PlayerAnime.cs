using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

enum AnimeType
{
    Idle,
    Left,
    Right,
}

public class PlayerAnime : MonoBehaviour
{
    [Header("精灵列表")]
    public List<Sprite> ReimuIdleSprites;
    public List<Sprite> ReimuLeftSprites;
    public List<Sprite> ReimuRightSprites;
    public List<Sprite> MarisaIdleSprites;
    public List<Sprite> MarisaLeftSprites;
    public List<Sprite> MarisaRightSprites;
    
    [Header("当前使用的精灵列表")]
    [SerializeField]
    private List<Sprite> IdleSprites;
    [SerializeField]
    private List<Sprite> LeftSprites;
    [SerializeField]
    private List<Sprite> RightSprites;

    [Header("动画参数")]
    [SerializeField]    
    private int _currentIndex = 0;// 动画索引
    private float TimeClock;// 时钟，用来记录过了多长时间
    [Header("动画速度（每隔多少帧切换一次动画）")]
    public int AnimeSpeed = 4;// 每隔多少帧切换一次动画

    [Header("移动参数")]
    [Header("玩家移动速度")]
    public float MoveSpeed = 5f;// 玩家移动速度

    [Header("状态")]
    private AnimeType _currentAnimeType = AnimeType.Idle;// 当前动画类型

    [Header("组件")]
    private SpriteRenderer spriteRenderer;// 精灵渲染器组件
    [Header("判定点动画")]
    public GameObject Pandingdian;// 玩家判定点
    private Vector3 PandingdianRotation = Vector3.forward;// 玩家判定点旋转角度
    public float PandingdianSpeed = 360f;// 玩家判定点旋转速度
    public Animator PandingdianAnimator;// 判定点动画组件


    [Header("虹人环动画")]
    public List<Sprite> ReimuCircles;
    public List<Sprite> MarisaCircles;

    public GameObject Circle1;// 内层虹人环
    public GameObject Circle2;// 中层虹人环
    public GameObject Circle3;// 外层虹人环
    public Animator CircleAnimator;// 虹人环动画组件


    private bool isPandingAnimePlaying = false;// 判定点动画是否正在播放
    private bool isCircleAnimePlaying = false;// 虹人环动画是否正在播放

    [Header("按键状态")]
    private bool leftKeyPressed = false;// 左键是否按下
    private bool rightKeyPressed = false;// 右键是否按下
    private bool upKeyPressed = false;// 上键是否按下
    private bool downKeyPressed = false;// 下键是否按下

    // 引用碰撞脚本
    public PlayerCollision playerCollision;

    void OnEnable()
    {
        // 初始化精灵列表
        InitSpriteLists();
        
        // 获取组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 初始化显示第一帧
        spriteRenderer.sprite = IdleSprites[0];
        
        // 获取碰撞脚本引用
        if (playerCollision == null)
        {
            playerCollision = GetComponent<PlayerCollision>();
        }
        Global_GameManager.Instance.OnReincarnation += ReincarnationAnime;
    }

    void OnDisable()
    {
        Global_GameManager.Instance.OnReincarnation -= ReincarnationAnime;
    }

    /// <summary>
    /// 初始化精灵列表
    /// </summary>
    private void InitSpriteLists()
    {
        if(Global_GameManager.Instance.character == Character.Reimu)
        {
            IdleSprites = ReimuIdleSprites;
            LeftSprites = ReimuLeftSprites;
            RightSprites = ReimuRightSprites;

            Circle1.GetComponent<SpriteRenderer>().sprite = ReimuCircles[0];
            Circle2.GetComponent<SpriteRenderer>().sprite = ReimuCircles[1];
            Circle3.GetComponent<SpriteRenderer>().sprite = ReimuCircles[2];
        }
        else
        {
            IdleSprites = MarisaIdleSprites;
            LeftSprites = MarisaLeftSprites;
            RightSprites = MarisaRightSprites;
            
            Circle1.GetComponent<SpriteRenderer>().sprite = MarisaCircles[0];
            Circle2.GetComponent<SpriteRenderer>().sprite = MarisaCircles[1];
            Circle3.GetComponent<SpriteRenderer>().sprite = MarisaCircles[2];
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 处理动画
        HandleAnimation();
        
        // 只有在游戏状态和无敌状态时才处理输入
        if(Global_GameManager.Instance.state == State.Gaming || 
           Global_GameManager.Instance.state == State.NoDead)
        {
            // 检查输入
            CheckInput();
        }
    }

    /// <summary>
    /// 检查输入
    /// </summary>
    private void CheckInput()
    {
        // 检测左键状态
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (!leftKeyPressed)
            {
                leftKeyPressed = true;
                SetLeftAnime();
            }
        }
        else if (leftKeyPressed)
        {
            leftKeyPressed = false;
            if (rightKeyPressed)
            {
                SetRightAnime();
            }
            else
            {
                SetIdleAnime();
            }
        }

        // 检测右键状态
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (!rightKeyPressed)
            {
                rightKeyPressed = true;
                SetRightAnime();
            }
        }
        else if (rightKeyPressed)
        {
            rightKeyPressed = false;
            if (leftKeyPressed)
            {
                SetLeftAnime();
            }
            else
            {
                SetIdleAnime();
            }
        }

        // 检测上键状态
        if (Input.GetKey(KeyCode.UpArrow))
        {
            upKeyPressed = true;
        }
        else if (!Input.GetKey(KeyCode.UpArrow))
        {
            upKeyPressed = false;
        }

        // 检测下键状态
        if (Input.GetKey(KeyCode.DownArrow))
        {
            downKeyPressed = true;
        }
        else if (!Input.GetKey(KeyCode.DownArrow))
        {
            downKeyPressed = false;
        }

        // 检测shift键状态
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // 移动速度减半
            MoveSpeed = 2.5f;
            // 显示判定点并开始动画
            StartPandingAnime();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            // 恢复正常移动速度
            MoveSpeed = 5f;
            // 隐藏判定点并停止动画
            StopPandingAnime();
        }
        // 将移动状态传递给碰撞脚本
        if (playerCollision != null)
        {
            playerCollision.UpdateMovement(leftKeyPressed, rightKeyPressed, upKeyPressed, downKeyPressed, MoveSpeed);
        }
    }

    /// <summary>
    /// 处理动画
    /// </summary>
    private void HandleAnimation()
    {
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
        
        // 处理判定点动画
        if (isPandingAnimePlaying)
        {
            PanDingAnime();
        }
        // 处理虹人环动画
        if (isCircleAnimePlaying)
        {
            CircleAnime();
        }
    }

    /// <summary>
    /// 开始判定点动画
    /// </summary>
    private void StartPandingAnime()
    {
        // 播放判定点动画
        PandingdianAnimator.SetBool("IsShift", true);
        isPandingAnimePlaying = true;
        CircleAnimator.SetBool("IsShift", true);
        isCircleAnimePlaying = true;
    }

    /// <summary>
    /// 停止判定点动画
    /// </summary>
    private void StopPandingAnime()
    {
        // 停止判定点动画
        PandingdianAnimator.SetBool("IsShift", false);
        isPandingAnimePlaying = false;
        CircleAnimator.SetBool("IsShift", false);
        isCircleAnimePlaying = false;
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

    private void PanDingAnime()
    {
        Pandingdian.transform.Rotate(PandingdianRotation, PandingdianSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 播放虹人环动画
    /// </summary>
    private void CircleAnime()
    {
        CircleAnimator.SetBool("IsShift", isCircleAnimePlaying);
    }

    private void ReincarnationAnime(State state)
    {
        StopPandingAnime();
        SetIdleAnime();
        
        // 立刻将玩家坐标设置为(-3,-6)，透明度设为0
        this.transform.position = new Vector3(-3f, -6f, 0f);
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
        
        // 开始重生动画协程
        StartCoroutine(ReincarnationAnimation());
    }
    
    /// <summary>
    /// 重生动画协程
    /// </summary>
    private IEnumerator ReincarnationAnimation()
    {
        float elapsedTime = 0f;
        float duration = 1f;
        Vector3 startPosition = new Vector3(-3f, -6f, 0f);
        Vector3 endPosition = new Vector3(-3f, -4f, 0f);
        float startAlpha = 0f;
        float endAlpha = 1f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            // 移动位置
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            // 渐变透明度
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                spriteRenderer.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终位置和透明度正确
        transform.position = endPosition;
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = endAlpha;
            spriteRenderer.color = color;
        }
        
        // 1秒后切换到无敌状态
        ReincarnationEnd();
    }

    private void ReincarnationEnd()
    {
        Global_GameManager.Instance.state = State.NoDead;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            MoveSpeed = 2.5f;
        }
        else
        {
            MoveSpeed = 5f;
        }
        Invoke(nameof(NoDeadEnd),1f);
    }

    private void NoDeadEnd()
    {
        Global_GameManager.Instance.state = State.Gaming;
    }
}

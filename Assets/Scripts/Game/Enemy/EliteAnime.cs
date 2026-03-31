using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteAnime : Enemy
{
    public List<Sprite> EnemySprites;// 精英敌人精灵

    [Header("动画参数")]
    [SerializeField]
    private int _currentIndex = 0;
    private float TimeClock;
    public int AnimeSpeed = 4;

    [Header("贝塞尔曲线参数")]
    private float t = 0f;
    public float bezierSpeed = 0.5f;
    private Vector2 startPoint;
    private Vector2 controlPoint;
    private Vector2 endPoint;
    private bool isMovingAlongBezier = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        bezierSpeed = MoveSpeed / 10;

        if (spriteRenderer != null && EnemySprites != null && EnemySprites.Count > 0)
        {
            spriteRenderer.sprite = EnemySprites[_currentIndex];
            spriteRenderer.color = new Color(1, 1, 1, 1);
        }

        t = 0f;

        if (moveMode == MoveMode.Gravity)
        {
            InitializeGravity();
        }

        if (moveMode == MoveMode.Flicker)
        {
            InitializeFlicker();
        }

        InitializeFadeIn();
    }

    protected override void Update()
    {
        // 播放动画
        PlayAnimation();

        // 处理淡入效果
        HandleFadeIn();

        // 调用基类的Update处理移动逻辑
        base.Update();
    }

    public override void SetMovePoints(List<GameObject> movePoints)
    {
        base.SetMovePoints(movePoints);

        if (MovePoints != null && MovePoints.Count > 0)
        {
            startPoint = transform.position;
            endPoint = MovePoints[0].transform.position;
            controlPoint = (startPoint + endPoint) * 0.5f;
            Vector2 direction = (endPoint - startPoint).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            float offset = Mathf.Min(Vector2.Distance(startPoint, endPoint) * 0.3f, 1f);
            controlPoint += perpendicular * offset;
            t = 0f;
            isMovingAlongBezier = true;
        }
    }

    protected override void MoveToNextPoint()
    {
        if (MovePoints == null || MovePoints.Count == 0 || currentPointIndex >= MovePoints.Count)
        {
            return;
        }

        if (isMovingAlongBezier)
        {
            t += Time.deltaTime * bezierSpeed;
            t = Mathf.Clamp01(t);

            Vector2 currentPosition = Mathf.Pow(1 - t, 2) * startPoint + 2 * (1 - t) * t * controlPoint + Mathf.Pow(t, 2) * endPoint;

            if (rb2D != null)
            {
                rb2D.MovePosition(currentPosition);
            }
            else
            {
                transform.position = currentPosition;
            }

            if (t >= 1f)
            {
                currentPointIndex++;

                if (currentPointIndex < MovePoints.Count)
                {
                    startPoint = endPoint;
                    endPoint = MovePoints[currentPointIndex].transform.position;
                    controlPoint = (startPoint + endPoint) * 0.5f;
                    Vector2 direction = (endPoint - startPoint).normalized;
                    Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                    float offset = Mathf.Min(Vector2.Distance(startPoint, endPoint) * 0.3f, 1f);
                    controlPoint += perpendicular * offset;
                    t = 0f;
                }
                else
                {
                    isMovingAlongBezier = false;
                    if (rb2D != null)
                    {
                        rb2D.velocity = Vector2.zero;
                    }
                    SwitchToSecondaryMoveMode();
                    isFirstMoveCompleted = true;
                }
            }
        }
    }

    /// <summary>
    /// 初始化淡入效果
    /// </summary>
    private void InitializeFadeIn()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        }
        fadeTimer = 0f;
    }

    /// <summary>
    /// 处理淡入效果
    /// </summary>
    private void HandleFadeIn()
    {
        if (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(fadeTimer / fadeTime);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            }
        }
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    private void PlayAnimation()
    {
        TimeClock += Time.deltaTime;
        int currentFrame = Mathf.FloorToInt(TimeClock * 60f);

        if (currentFrame >= AnimeSpeed && EnemySprites != null && EnemySprites.Count > 0)
        {
            _currentIndex = (_currentIndex + 1) % EnemySprites.Count;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = EnemySprites[Mathf.Min(_currentIndex, EnemySprites.Count - 1)];
            }

            TimeClock -= (float)AnimeSpeed / 60f;
        }
    }
}

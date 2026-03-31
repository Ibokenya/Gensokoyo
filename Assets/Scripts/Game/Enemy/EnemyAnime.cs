using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Dir
{
    Idle,
    Left,
    Right,
}

public class EnemyAnime : Enemy
{
    [Header("不同方向敌人的精灵图")]
    public List<Sprite> enemySprites1;
    public List<Sprite> enemySprites2;
    public List<Sprite> enemySprites3;
    public List<Sprite> enemySprites4;
    private List<Sprite> currentEnemySprites;

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

    [SerializeField]
    private Dir _currentDir = Dir.Idle;

    protected override void OnEnable()
    {
        base.OnEnable();

        bezierSpeed = MoveSpeed / 10;
        int randomIndex = Random.Range(0, 4);
        switch (randomIndex)
        {
            case 0:
                currentEnemySprites = enemySprites1;
                break;
            case 1:
                currentEnemySprites = enemySprites2;
                break;
            case 2:
                currentEnemySprites = enemySprites3;
                break;
            case 3:
                currentEnemySprites = enemySprites4;
                break;
        }

        if (spriteRenderer != null && currentEnemySprites != null && currentEnemySprites.Count > 0)
        {
            spriteRenderer.sprite = currentEnemySprites[_currentIndex];
            spriteRenderer.color = new Color(1, 1, 1, 1);
        }

        t = 0f;
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
            controlPoint += new Vector2(0, 0.5f);
            t = 0f;
            isMovingAlongBezier = true;
        }
    }

    /// <summary>
    /// 设置敌人的移动方向
    /// </summary>
    public void SetDirection(Dir newDir)
    {
        if (_currentDir != newDir)
        {
            _currentDir = newDir;
            _currentIndex = 0;
            TimeClock = 0f;
            UpdateSprite();
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

        if (currentFrame >= AnimeSpeed && currentEnemySprites != null && currentEnemySprites.Count > 0)
        {
            _currentIndex = (_currentIndex + 1) % currentEnemySprites.Count;
            UpdateSprite();
            TimeClock -= (float)AnimeSpeed / 60f;
        }
    }

    /// <summary>
    /// 更新精灵图
    /// </summary>
    private void UpdateSprite()
    {
        if (spriteRenderer != null && currentEnemySprites != null && currentEnemySprites.Count > 0)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.sprite = currentEnemySprites[Mathf.Min(_currentIndex, currentEnemySprites.Count - 1)];
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 小球沿着路径移动，平滑移动，点之间直接移动
public class BallsAnime : Enemy
{
    public List<Sprite> ballsSprites;// 小球精灵
    private Sprite currentBallSprite;// 当前小球

    [SerializeField]
    private float RotateSpeed = 360f;// 小球旋转速度

    protected override void OnEnable()
    {
        base.OnEnable();

        int randomIndex = Random.Range(0, ballsSprites.Count);
        currentBallSprite = ballsSprites[randomIndex];

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = currentBallSprite;
            spriteRenderer.color = new Color(1, 1, 1, 1);
        }

        // 闪烁模式下需要设置路径点
        if (moveMode == MoveMode.Flicker && MovePoints != null)
        {
            // 路径点已在基类SetMovePoints中设置
        }
    }

    protected override void Update()
    {
        // 旋转小球
        RotateBall();

        // 调用基类的Update处理移动逻辑
        base.Update();
    }

    /// <summary>
    /// 旋转小球
    /// </summary>
    private void RotateBall()
    {
        transform.Rotate(Vector3.forward, RotateSpeed * Time.deltaTime);
    }
}

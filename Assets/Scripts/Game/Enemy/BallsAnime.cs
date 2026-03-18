using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallsAnime : MonoBehaviour
{
    public List<Sprite> ballsSprites;// 球精灵变体
    private Sprite currentBallSprite;// 当前球精灵

    [Header("移动参数")]
    public float MoveSpeed = 5f;// 妖精移动速度
    [SerializeField]
    private float RotateSpeed = 45f;// 球旋转速度

    [Header("组件")]
    private SpriteRenderer spriteRenderer;// 精灵渲染器组件
    private Rigidbody2D rb2D;// 刚体组件

    void OnEnable()
    {
        int randomIndex = Random.Range(0, ballsSprites.Count);
        currentBallSprite = ballsSprites[randomIndex];// 随机选择一个球的变体精灵
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D= GetComponent<Rigidbody2D>();
        spriteRenderer.sprite = currentBallSprite;// 初始化精灵渲染器的精灵为当前球的变体精灵
    }
}

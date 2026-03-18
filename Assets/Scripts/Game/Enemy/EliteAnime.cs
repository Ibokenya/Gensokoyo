using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteAnime : MonoBehaviour
{
    public List<Sprite> EnemySprites;// 当前妖精精灵

    [Header("动画参数")]
    [SerializeField]    
    private int _currentIndex = 0;// 动画索引
    private float TimeClock;// 时钟，用来记录过了多长时间
    [Header("动画速度（每隔多少帧切换一次动画）")]
    public int AnimeSpeed = 4;// 每隔多少帧切换一次动画

    [Header("移动参数")]
    public float MoveSpeed = 5f;// 妖精移动速度

    [Header("组件")]
    private SpriteRenderer spriteRenderer;// 精灵渲染器组件
    private Rigidbody2D rb2D;// 刚体组件

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D= GetComponent<Rigidbody2D>();
        spriteRenderer.sprite = EnemySprites[_currentIndex];// 初始化精灵渲染器的精灵为当前妖精的变体精灵
    }
}

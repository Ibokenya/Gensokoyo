using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Dir{// 标记妖精的左右移动方向
// 由于左右移动涉及动画，所以每次更换方向至少需要间隔一定时间（否则动画会抽搐）
    Idle,// 大多情况下使用Idle，哪怕上下左右飞
    Left,// 只会当妖精从屏幕左右出现时转为左或右
    Right,
}
public class EnemyAnime : MonoBehaviour
{
    [Header("不同类型妖精的变体精灵")]
    public List<Sprite> enemySprites1;// 妖精的变体精灵
    public List<Sprite> enemySprites2;// 妖精的变体精灵
    public List<Sprite> enemySprites3;// 妖精的变体精灵
    public List<Sprite> enemySprites4;// 妖精的变体精灵
    private List<Sprite> currentEnemySprites;// 当前妖精的变体精灵

    [Header("动画参数")]
    [SerializeField]    
    private int _currentIndex = 0;// 动画索引
    private float TimeClock;// 时钟，用来记录过了多长时间
    [Header("动画速度（每隔多少帧切换一次动画）")]
    public int AnimeSpeed = 4;// 每隔多少帧切换一次动画

    [Header("移动参数")]
    public float MoveSpeed = 5f;// 妖精移动速度
    [SerializeField]
    private Dir _currentDir = Dir.Idle;// 当前移动方向

    [Header("组件")]
    private SpriteRenderer spriteRenderer;// 精灵渲染器组件
    private Rigidbody2D rb2D;// 刚体组件

    void OnEnable()
    {
        int randomIndex = Random.Range(0, 4);
        switch (randomIndex)// 随机选择一个妖精的变体精灵
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
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2D= GetComponent<Rigidbody2D>();
        spriteRenderer.sprite = currentEnemySprites[_currentIndex];// 初始化精灵渲染器的精灵为当前妖精的变体精灵
    }
}
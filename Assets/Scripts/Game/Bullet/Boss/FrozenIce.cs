using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenIce : MonoBehaviour
{
    public GameObject ParentOb; // 父物体
    public int hp = 240; // 生命值
    public GameObject normalIcePrefab; // 普通冰子弹预制件（用于破裂攻击）
    public BossShootSystem bossShootSystem; // Boss射击系统引用
    private Rigidbody2D rb2D;
    private float timer = 0f; // 计时器
    
    // 边界范围
    private readonly float minX = -13f;
    private readonly float maxX = 7f;
    private readonly float minY = -9f;
    private readonly float maxY = 9f;

    
    void Update()
    {
        if (ParentOb != null)
        {
            transform.position = ParentOb.transform.position;
        }
        
        CheckBounds();
        
        // 每秒流失20点HP
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            hp -= 20;
            timer = 0f;
            if (hp <= 0)
            {
                Recycle();
            }
        }
    }
    
    /// <summary>
    /// 受伤方法
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Recycle();
        }
    }
    
    /// <summary>
    /// 检查边界，超出范围则回收
    /// </summary>
    private void CheckBounds()
    {
        Vector2 position = transform.position;
        if (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY)
        {
            Recycle();
        }
    }
    
    /// <summary>
    /// 回收子弹
    /// </summary>
    private void Recycle()
    {
        // 保存当前位置用于发射破裂子弹
        Vector3 destroyedPosition = transform.position;
        bossShootSystem.Shake(0.5f);
        // 通知BossShootSystem增加随机射击的子弹数量
        if (bossShootSystem != null)
        {
            bossShootSystem.OnFrozenIceDestroyed();
            if(Global_GameManager.Instance.state != State.SpellCard)
            {
                bossShootSystem.FrozenIceExplode(destroyedPosition, normalIcePrefab);
            }
        }
        
        if (Global_ObjectPool.Instance != null)
        {
            Global_ObjectPool.Instance.Recycle(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnDisable()
    {
        ParentOb = null;
        normalIcePrefab = null;
        bossShootSystem = null;
        hp = 240;
        timer = 0f;
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenBall : MonoBehaviour
{
    public int hp = 300; // 生命值
    public BossShootSystem bossShootSystem; // Boss射击系统引用
    public GameObject icePearl; // 对应的冰珠引用
    private Rigidbody2D rb2D;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
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
    /// 回收子弹
    /// </summary>
    private void Recycle()
    {
        // 通知BossShootSystem释放冰珠和生成冰锥
        if (bossShootSystem != null)
        {
            // 摇动镜头
            bossShootSystem.Shake(0.5f);
            // 释放对应的冰珠
            bossShootSystem.ReleaseFrozenPearl(icePearl);
            // 生成冰锥
            bossShootSystem.SpawnIceSpikes(9);
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
        bossShootSystem = null;
        icePearl = null;
        hp = 300;
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
        }
    }
}

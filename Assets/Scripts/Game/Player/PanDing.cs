using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanDing : MonoBehaviour
{
    public ClearAllBullet clearAllBullet;// 清除所有子弹组件
    public SpellCardEffect spellCardEffect;// 符卡效果组件

    /// <summary>
    /// 触发器检测
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(Global_GameManager.Instance.state == State.Gaming)
        {
            // 确保只对敌人和敌人子弹生效
            if(collision.CompareTag("Enemy") || collision.CompareTag("EnemyBullet"))
            {
                // 检查是否处于无敌状态
                if (spellCardEffect != null && Global_GameManager.Instance.state == State.NoDead)
                {
                    // 无敌状态下不处理受击
                    clearAllBullet.ClearScreenBullet();
                    return;
                }
                
                if (spellCardEffect != null)
                {
                    // 开始受击延迟
                    spellCardEffect.StartHitDelay();
                }
                else
                {
                    // 没有符卡效果组件时，执行正常死亡逻辑
                    Global_GameManager.Instance.SubLeftLife();
                }
                clearAllBullet.ClearScreenBullet();
            }
        }  
    }
}

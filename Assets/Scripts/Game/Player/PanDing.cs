using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanDing : MonoBehaviour
{
    public FreezeSystem freezeSystem; // 冻结系统引用
    public ClearAllBullet clearAllBullet;// 清除所有子弹组件
    public SpellCardEffect spellCardEffect;// 符卡效果组件
    public Graze graze; // 擦弹组件引用

    private const float ICE_CLOUD_FROZEN_DEGREE_INCREASE = 0.01f; // 每次碰撞冰云增加的冻结度

    /// <summary>
    /// 触发器检测
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(Global_GameManager.Instance.state == State.Gaming)
        {
            // 确保只对敌人和敌人子弹生效
            if(collision.CompareTag("Enemy") || collision.CompareTag("EnemyBullet") ||
             collision.CompareTag("BossBullet") || collision.CompareTag("Terrain"))
            {
                Debug.Log($"玩家碰撞到{collision.name}");
                // 检查是否开启作弊模式
                if(Global_GameManager.Instance.isCheheat)
                {
                    // 作弊模式下不处理受击
                    return;
                }
                // 检查是否处于无敌状态
                if (spellCardEffect != null && Global_GameManager.Instance.state == State.NoDead)
                {
                    // 无敌状态下不处理受击
                    return;
                }
                
                // 停止擦弹音效并清空擦弹列表（防止玩家复活后继续播放擦弹音效）
                StopGrazeSound();
                
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
            // 处理冰云碰撞（玩家不会受伤，但冻结度会增加）
            if(collision.CompareTag("IceCloud"))
            {
                // 增加冻结度
                if (freezeSystem != null)
                {
                    freezeSystem.IncreaseFrozenDegree(ICE_CLOUD_FROZEN_DEGREE_INCREASE);
                }
            }
        }  
    }
    
    /// <summary>
    /// 停止擦弹音效并清空擦弹列表
    /// 防止玩家复活后继续播放擦弹音效
    /// </summary>
    private void StopGrazeSound()
    {
        if (graze != null)
        {
            graze.ForceStopGrazeSound();
        }
    }
}

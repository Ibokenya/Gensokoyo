using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySystem;

public class PanDing : MonoBehaviour
{
    public FreezeSystem freezeSystem;
    public ClearAllBullet clearAllBullet;
    public SpellCardEffect spellCardEffect;
    public Graze graze;

    private const float ICE_CLOUD_FROZEN_DEGREE_INCREASE = 0.01f;

    // 帧内去重标记：Tail 拖尾弹等多物体在同一 tick 内会触发多次 OnTriggerEnter2D，
    // 用 SimTick 去重保证每 tick 只处理一次命中。
    // 初始值用 ulong.MaxValue 确保第一个 tick（SimTick=0）不会被误判为"已处理"。
    private ulong lastHitProcessedTick = ulong.MaxValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 回放模式：物理中弹完全忽略 —— 由 HitFlag 位驱动
        // 这样物理偏差不会导致意外死亡
        if (ReplayManager.Instance != null && ReplayManager.Instance.CurrentMode == ReplayManager.Mode.Playback) return;

        if(Global_GameManager.Instance.state == State.Gaming || 
        Global_GameManager.Instance.state == State.Frozen)
        {
            // 同一物理 tick 内只处理一次命中 —— 防多弹重合重复触发决死/掉血
            if (lastHitProcessedTick == SimClock.SimTick) return;

            if(collision.CompareTag("Enemy") || collision.CompareTag("EnemyBullet") ||
             collision.CompareTag("BossBullet") || collision.CompareTag("Terrain") ||
             collision.CompareTag("MiniBall") || collision.CompareTag("Boss"))
            {
                // 标记本 tick 已处理 —— 后续同一 tick 的其他 OnTriggerEnter2D 直接跳过
                lastHitProcessedTick = SimClock.SimTick;

                Debug.Log($"玩家碰撞到{collision.name}");
                StopGrazeSound();

                //  设置本帧 HitFlag（写入回放文件的 bit7）
                // 回放时 ReplayInputProvider.GetKey(HitFlag) 返回 true → ForceHit 被触发
                ReplayManager.MarkHitThisTick();

                if (spellCardEffect != null)
                    spellCardEffect.StartHitDelay();
                else
                    Global_GameManager.Instance.SubLeftLife();
                clearAllBullet.ClearScreenBullet();
            }
            if(collision.CompareTag("IceCloud"))
            {
                if (freezeSystem != null)
                    freezeSystem.IncreaseFrozenDegree(ICE_CLOUD_FROZEN_DEGREE_INCREASE);
            }
        }  
    }

    // 逻辑和 OnTriggerEnter2D 里完全相同，但绕过物理碰撞</summary>
    public void ForceHit()
    {
        StopGrazeSound();
        if (spellCardEffect != null)
            spellCardEffect.StartHitDelay();
        else
            Global_GameManager.Instance.SubLeftLife();
        if (clearAllBullet != null)
            clearAllBullet.ClearScreenBullet();
    }

    private void StopGrazeSound()
    {
        if (graze != null)
            graze.ForceStopGrazeSound();
    }
}
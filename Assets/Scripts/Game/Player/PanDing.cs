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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 回放模式：物理中弹完全忽略 —— 由 HitFlag 位驱动
        // 这样物理偏差不会导致意外死亡
        if (ReplayManager.Instance != null && ReplayManager.Instance.CurrentMode == ReplayManager.Mode.Playback) return;

        if(Global_GameManager.Instance.state == State.Gaming || 
        Global_GameManager.Instance.state == State.Frozen)
        {
            if(collision.CompareTag("Enemy") || collision.CompareTag("EnemyBullet") ||
             collision.CompareTag("BossBullet") || collision.CompareTag("Terrain"))
            {
                Debug.Log($"玩家碰撞到{collision.name}");
                StopGrazeSound();

                // 🔴 录制关键：设置本帧 HitFlag（写入回放文件的 bit7）
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

    /// <summary>🔴 回放时由 ReplayManager 强制调用 —— 模拟一次中弹
    /// 逻辑和 OnTriggerEnter2D 里完全相同，但绕过物理碰撞</summary>
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
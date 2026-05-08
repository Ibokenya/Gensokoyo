using UnityEngine;
using System.Collections.Generic;

public class Graze : MonoBehaviour
{
    public AudioClip grazeSound;
    private List<Collider2D> currentBullets = new(); // 存储当前在判定区域内的弹幕
    private bool isPlaying = false; // 标记是否正在播放擦弹音效
    public FreezeSystem FreezeSystem;

    void OnEnable()
    {
        currentBullets.Clear();
    }
    void OnDisable()
    {
        currentBullets.Clear();
        if (isPlaying)
        {
            Global_AudioManager.Instance.StopLoopSFX(grazeSound);
            isPlaying = false;
        }
    }

    private void Update()
    {
        if (currentBullets.Count > 0)
        {
            currentBullets.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy);
            if (currentBullets.Count == 0 && isPlaying)
            {
                Global_AudioManager.Instance.StopLoopSFX(grazeSound);
                isPlaying = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(Global_GameManager.Instance.state == State.Gaming)
        {
            // 确保只对敌人和敌人子弹和Boss子弹生效
            if (collision.CompareTag("Enemy") || collision.CompareTag("EnemyBullet") || collision.CompareTag("BossBullet"))
            {
                // 增加擦弹数
                Global_GameManager.Instance.AddGraze(1);
                
                // 擦弹时减少冻结进度
                FreezeSystem.ReduceFrozenDegree();
                
                // 如果当前列表为空，且当前弹幕不在列表中，播放擦弹音效
                if (currentBullets.Count == 0 && !isPlaying)
                {
                    Global_AudioManager.Instance.PlaySFX(grazeSound, true);
                    isPlaying = true;
                }
                
                // 如果当前弹幕不在列表中，添加到列表
                if (!currentBullets.Contains(collision))
                {
                    currentBullets.Add(collision);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(Global_GameManager.Instance.state == State.Gaming)
        {
            // 确保只对敌人和敌人子弹和Boss子弹生效
            if (collision.CompareTag("Enemy") || collision.CompareTag("EnemyBullet") ||
            collision.CompareTag("BossBullet"))
            {
                // 从列表中移除弹幕
                if (currentBullets.Contains(collision))
                {
                    currentBullets.Remove(collision);
                }

                // 如果列表为空，停止播放音效
                if (currentBullets.Count == 0 && isPlaying)
                {
                    Global_AudioManager.Instance.StopLoopSFX(grazeSound);
                    isPlaying = false;
                }
            }
        }
    }

    /// <summary>
    /// 强制停止擦弹音效
    /// 在时停动画结束后调用，确保擦弹音效被正确停止
    /// </summary>
    public void ForceStopGrazeSound()
    {
        currentBullets.Clear();
        if (isPlaying)
        {
            Global_AudioManager.Instance.StopLoopSFX(grazeSound);
            isPlaying = false;
        }
    }
}

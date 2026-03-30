using UnityEngine;
using System.Collections.Generic;

public class Graze : MonoBehaviour
{
    public AudioClip grazeSound;
    private List<Collider2D> currentBullets = new(); // 存储当前在判定区域内的弹幕
    private bool isPlaying = false; // 标记是否正在播放擦弹音效

    void OnDisable()
    {
        // 清理
        currentBullets.Clear();
        if (isPlaying)
        {
            Global_AudioManager.Instance.StopLoopSFX(grazeSound);
            isPlaying = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 确保只对敌人子弹生效
        if (collision.CompareTag("EnemyBullet"))
        {
            // 添加擦弹数
            Global_GameManager.Instance.AddGraze(1);
            
            // 如果是第一个进入的弹幕，开始播放音效
            if (currentBullets.Count == 0 && !isPlaying)
            {
                Global_AudioManager.Instance.PlaySFX(grazeSound, true);
                isPlaying = true;
            }
            
            // 将弹幕添加到列表中
            if (!currentBullets.Contains(collision))
            {
                currentBullets.Add(collision);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 确保只对敌人子弹生效
        if (collision.CompareTag("EnemyBullet"))
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

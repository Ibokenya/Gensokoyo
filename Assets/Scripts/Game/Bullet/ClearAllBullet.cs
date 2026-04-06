using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearAllBullet : MonoBehaviour
{
    [SerializeField] 
    private List<float> clearTimes = new ();
    public AudioClip clearClip;//清屏音效clip
    public AudioClip BeHitClip;//中弹音效clip
    private float currentMusicTime = 0f;

    void Update()
    {
        // 更新当前音乐时间
        // currentMusicTime = Global_AudioManager.Instance.CurrentBGMTime;
        currentMusicTime += Time.deltaTime;

        // 检查是否到达清屏时间点
        CheckClearTimes();
    }

    /// <summary>
    /// 检查是否到达清屏时间点
    /// </summary>
    private void CheckClearTimes()
    {
        // 遍历时间点列表
        for (int i = clearTimes.Count - 1; i >= 0; i--)
        {
            float targetTime = clearTimes[i];
            // 当音乐时间达到或超过目标时间时，执行清屏操作
            if (currentMusicTime >= targetTime - 0.01f && currentMusicTime <= targetTime + 1f)
            {
                // 执行清屏操作
                ClearEnemyBullet();
                // 移除已执行的时间点，避免重复执行
                clearTimes.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 清空屏幕中的所有子弹
    /// </summary>
    public void ClearScreenBullet(bool isPlaySound = true)
    {
        // 播放清屏音效
        if(isPlaySound)
        {
            Global_AudioManager.Instance.PlaySFX(BeHitClip);
        }
        // 找到所有敌人子弹
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        GameObject[] playerBullets = GameObject.FindGameObjectsWithTag("PlayerBullet");

        // 回收所有敌人子弹
        foreach (GameObject bullet in enemyBullets)
        {
            if (bullet != null)
            {
                Global_ObjectPool.Instance.Recycle(bullet);
            }
        }
        // 回收所有玩家子弹
        foreach (GameObject bullet in playerBullets)
        {
            if (bullet != null)
            {
                Global_ObjectPool.Instance.Recycle(bullet);
            }
        }
    }

    public void ClearEnemyBullet()
    {
         // 播放清屏音效
        Global_AudioManager.Instance.PlaySFX(clearClip);
        // 找到所有敌人子弹
        GameObject[] enemyBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        // 回收所有敌人子弹
        foreach (GameObject bullet in enemyBullets)
        {
            if (bullet != null)
            {
                Global_ObjectPool.Instance.Recycle(bullet);
            }
        }
    }
}

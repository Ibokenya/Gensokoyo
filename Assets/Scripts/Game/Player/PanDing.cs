using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanDing : MonoBehaviour
{
    public ClearAllBullet clearAllBullet;// 清除所有子弹组件

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
                Global_GameManager.Instance.SubLeftLife();
                clearAllBullet.ClearScreenBullet();
            }
        }  
    }
}

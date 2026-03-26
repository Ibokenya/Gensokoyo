using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootNormal : MonoBehaviour
{
    public GameObject ReimuNormal;
    public GameObject MarisaNormal;
    private GameObject Normal;

    [Header("枪管配置")]
    [Tooltip("第一个枪管位置")]
    public Transform GunLeft;
    [Tooltip("第二个枪管位置")]
    public Transform GunRight;

    [Header("射击配置")]
    [Tooltip("射击间隔（秒）")]
    public float shootInterval = 0.12f; // 射击间隔
    private float shootTimer; // 射击冷却计时器

    private bool IsLimited = false; // 是否限制射击(七曜攻击期间停止)

    void OnEnable()
    {
        // 初始化对应角色的弹幕预制体
        UpdateNormalPrefab();

        // 初始化计时器（确保游戏开始即可射击）
        shootTimer = shootInterval;
        
        // 初始化弹幕池
        if (Normal != null)
        {
            Global_ObjectPool.Instance.InitPool(Normal, 0);
        }
        else
        {
            Debug.LogWarning("Normal prefab is null, bullet pool not initialized");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Global_GameManager.Instance == null || Global_GameManager.Instance.state != State.Gaming) return;
        
        // 确保Normal预制体已初始化
        if (Normal == null)
        {
            UpdateNormalPrefab();
        }
        
        // 计时器持续累加
        shootTimer += Time.deltaTime;
        if(IsLimited)
            return;
        Shoot();
    }

    /// <summary>
    /// 更新Normal预制体引用
    /// </summary>
    private void UpdateNormalPrefab()
    {
        if (Global_GameManager.Instance != null)
        {
            if (Global_GameManager.Instance.character == Character.Reimu)
            {
                Normal = ReimuNormal;
            }
            else if (Global_GameManager.Instance.character == Character.Marisa)
            {
                Normal = MarisaNormal;
            }
        }
    }

    private void Shoot()
    {
        // 只有按下Z键 + 计时器达到间隔时间 + Normal预制体不为null，才允许射击
        if (Input.GetKey(KeyCode.Z) && shootTimer >= shootInterval && Normal != null)
        {
            try
            {
                // 如果枪管不为空，则从对应枪管位置发射子弹
                if (GunLeft != null && GunRight != null)
                {
                    // 从对象池获取弹幕
                    Global_ObjectPool.Instance.GetObject
                    (Normal, GunLeft.position, Normal.transform.rotation);
                    Global_ObjectPool.Instance.GetObject
                    (Normal, GunRight.position, Normal.transform.rotation);
                    shootTimer = 0; // 射击后重置计时器，开始冷却
                }
                else
                {
                    Debug.LogWarning("枪管位置未设置，无法发射子弹");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("发射普通子弹失败: " + e.Message);
            }
        }
    }

    public void SetLimited(bool isLimited)
    {
        IsLimited = isLimited;
    }
}

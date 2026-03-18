using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这里是灵梦的子机射击脚本
/// 子机种包含封魔针与追踪阴阳玉
/// 因此需要初始化两种弹幕池
/// </summary>
public class ReimuShoot : MonoBehaviour
{
    public GameObject NeedlePrefab;// 封魔针预制体
    public GameObject TrackedPrefab;// 追踪阴阳玉预制体

    [Header("射击配置")]
    [Tooltip("封魔针射击间隔（秒）")]
    public float NeedleInterval = 0.12f; // 封魔针射击间隔
    [Tooltip("阴阳玉射击间隔（秒）")]
    public float TrackedInterval = 0.12f; // 追踪阴阳玉射击间隔

    private float shootTimer = 0f; // 射击冷却计时器

    public GunAnime GunAnime;// 子机武器动画脚本

    void OnEnable()
    {
        // 初始化封魔针弹幕池
        Global_ObjectPool.Instance.InitPool(NeedlePrefab,30);
        // 初始化追踪阴阳玉弹幕池
        Global_ObjectPool.Instance.InitPool(TrackedPrefab,0);
    }

    // Update is called once per frame
    void Update()
    {
        // 计时器持续累加
        shootTimer += Time.deltaTime;
        CheckShift();
    }

    /// <summary>
    /// 检查是否按下了Shift键
    /// </summary>
    private void CheckShift()
    {
        if (GunAnime.IsShiftedNow)
        {
            ShootNeedle();
        }
        else
        {
            ShootTracked();
        }
    }

    private void ShootNeedle()
    {
        if(Input.GetKey(KeyCode.Z) && shootTimer >= NeedleInterval)
        {
            Global_ObjectPool.Instance.GetBullet(NeedlePrefab, transform.position, NeedlePrefab.transform.rotation);
            // 重置计时器
            shootTimer = 0f;
        }
    }

    private void ShootTracked()
    {
        if(Input.GetKey(KeyCode.Z) && shootTimer >= TrackedInterval)
        {
            Global_ObjectPool.Instance.GetBullet(TrackedPrefab, transform.position, TrackedPrefab.transform.rotation);
            // 重置计时器
            shootTimer = 0f;
        }
    }
}

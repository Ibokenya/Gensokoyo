using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 激光类
/// 魔理沙使用激光作为高速副手攻击
/// 使用LineRenderer+激光材质实现
/// 激光连线固定为(0,0,0)-(0,12,0)，通过父物体旋转控制方向
/// </summary>
public class Laser : MonoBehaviour
{
    [Header("激光配置")]
    public float MaxLength = 12f; // 激光长度（固定12）
    public float LaserWidth = 0.02f; // 激光宽度（public，由Inspector赋值）
    public int damage = 1; // 激光伤害(每帧)
    public LayerMask HitLayer;// 激光可攻击目标层

    [Header("激光动画纹理")]
    public List<Texture2D> LaserTextures = new();// 激光纹理列表
    public int AnimeSpeed = 4;
    private int CurrentIndex = 0;// 当前纹理索引

    private LineRenderer lineRenderer;// 激光连线渲染器
    private Material laserMaterial;// 激光材质实例
    private bool isActive = false;// 激光是否激活

    private float TimeClock = 0f;// 动画时钟

    private int LaserInterval = 5;// 激光伤害间隔帧（每秒12帧出伤）

    void Awake()
    {
        // 确保获取到 LineRenderer 组件
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component not found on Laser object!");
        }
        else
        {
            lineRenderer.enabled = false;
            // 设置起始和结束宽度为 LaserWidth，避免受基础宽度影响
            lineRenderer.startWidth = LaserWidth;
            lineRenderer.endWidth = LaserWidth;
            lineRenderer.widthMultiplier = 1f;
            // 创建材质实例
            if (lineRenderer.material != null)
            {
                laserMaterial = new Material(lineRenderer.material);
                lineRenderer.material = laserMaterial;
            }
            // 设置固定的激光连线点（本地坐标）
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(0, MaxLength, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!isActive || lineRenderer == null)
        {
            return;
        }
        
        UpdateLaserAnime();
        LaserInterval-=1;
        if(LaserInterval == 0)
        {
            UpdateLaserDamage();
            LaserInterval = 5;
        }   
    }

    /// <summary>
    /// 激光持续伤害判定
    /// </summary>
    private void UpdateLaserDamage()
    {
        // 使用RaycastAll检测路径上的所有物体
        // 激光是发射器的子物体，从transform.position（世界坐标）开始检测
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.up, MaxLength, HitLayer);
        
        // 对所有命中的物体造成伤害
        foreach (RaycastHit2D hit in hits)
        {
            switch (hit.collider.tag)
            {
                case "Enemy":
                    var enemy = hit.collider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.Damage(damage);
                    }
                    break;
                case "Boss":
                    var boss = hit.collider.GetComponent<BossBase>();
                    if (boss != null)
                    {
                        int tempDamage = (int)(damage * 3);// 对Boss造成伤害值翻3倍
                        // 对Boss造成伤害
                        boss.TakeDamage(tempDamage);
                    }
                    break;
                case "FrozenIce":
                    var frozenIce = hit.collider.GetComponent<FrozenIce>();
                    if (frozenIce != null)
                    {
                        frozenIce.TakeDamage(damage);
                    }
                    break;
                case "FrozenBall":
                    var frozenBall = hit.collider.GetComponent<FrozenBall>();
                    if (frozenBall != null)
                    {
                        frozenBall.TakeDamage(damage);
                    }
                    break;
                case "MiniBall":
                    var miniBall = hit.collider.GetComponent<miniIceBall>();
                    if (miniBall != null)
                    {
                        miniBall.TakeDamage(damage);
                    }
                    break;
                default:
                    break;
            }
            Global_GameManager.Instance.AddScore(2);
        }
    }

    /// <summary>
    /// 激活激光
    /// </summary>
    public void ActivateLaser()
    {
        isActive = true;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    /// <summary>
    /// 停用激光
    /// </summary>
    public void StopLaser()
    {
        isActive = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    /// <summary>
    /// 更新激光动画（通过切换材质纹理实现）
    /// </summary>
    private void UpdateLaserAnime()
    {
        if (LaserTextures.Count == 0 || laserMaterial == null)
            return;
            
        TimeClock += Time.deltaTime;
        if (TimeClock >= 1f / AnimeSpeed)
        {
            TimeClock -= 1f / AnimeSpeed;
            CurrentIndex++;
            if (CurrentIndex >= LaserTextures.Count)
                CurrentIndex = 0;
                
            // 更新材质的Particle Texture属性
            laserMaterial.SetTexture("_MainTex", LaserTextures[CurrentIndex]);
        }
    }

    private void OnDestroy()
    {
        // 销毁材质实例，避免内存泄漏
        if (laserMaterial != null)
        {
            Destroy(laserMaterial);
        }
    }
}

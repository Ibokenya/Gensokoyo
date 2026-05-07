using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AimPointAttack : MonoBehaviour
{
    [Header("旋转参数")]
    public float rotationSpeed = 180f; // 旋转速度，单位：度/秒
    
    [Header("魔法珠参数")]
    public GameObject SwordPrefab; // 魔法珠预制体
    public float xMin = 20f; // x轴最小值
    public float xMax = 30f; // x轴最大值
    public float yMin = 21.8f; // y轴最小值
    public float yMax = 32.7f; // y轴最大值
    public float zMin = 34.4f; // z轴最小值
    public float zMax = 51.6f; // z轴最大值
    public float SwordSpeed = 20f; // 魔法珠飞行速度
    public float spawnDelayMin = 1f; // 生成魔法珠的最小延迟时间
    public float spawnDelayMax = 2f; // 生成魔法珠的最大延迟时间
    public float reuseDelayMin = 0.1f; // 再次召唤魔法珠的最小延迟时间
    public float reuseDelayMax = 0.5f; // 再次召唤魔法珠的最大延迟时间
    
    [Header("爆炸动画")]
    public Sprite[] markerSprites; // 瞄准点精灵数组（4张：1张瞄准点 + 3张爆炸动画）
    public float explosionFrameInterval = 10f; // 爆炸动画帧间隔
    
    public GameObject Sword; // 当前魔法珠
    private int frameCounter = 0; // 帧计数器
    private readonly Vector3 CAMERA_POSITION = new (0, 0, -10f); // 摄像头位置（常量）
    private readonly float fadeInDuration = 0.5f; // 魔法珠的淡入时间
    private SpriteRenderer spriteRenderer; // 瞄准点的SpriteRenderer组件
    
    void OnDisable()
    {
        // 取消所有Invoke调用
        CancelInvoke();
        
        // 当瞄准点被回收时，回收神秘珠到对象池
        if (Sword != null && Global_ObjectPool.Instance != null)
        {
            Global_ObjectPool.Instance.Recycle(Sword);
            Sword = null;
        }
    }
    
    void OnEnable()
    {
        // 初始化SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && markerSprites != null && markerSprites.Length > 0)
        {
            // 设置初始精灵为第一张瞄准点图片
            spriteRenderer.sprite = markerSprites[0];
        }
    }
    
    /// <summary>
    /// 从对象池获取魔法珠
    /// </summary>
    private void SpawnSword()
    {
        // 检查游戏对象是否激活
        if (!gameObject.activeInHierarchy)
        {
            // Debug.LogWarning($"[AimPointAttack] 游戏对象未激活，跳过生成魔法珠");
            return;
        }
        
        if (SwordPrefab != null && Global_ObjectPool.Instance != null)
        {
            // 计算随机生成位置
            Vector3 spawnPosition = GetRandomSpawnPosition();
            Sword = Global_ObjectPool.Instance.GetObject(SwordPrefab, spawnPosition, Quaternion.identity);
            
            if (Sword != null && gameObject.activeInHierarchy)
            {
                // 开始淡入效果
                StartCoroutine(FadeInSword(Sword));
            }
            else
            {
                Debug.LogWarning("[AimPointAttack] 无法从对象池获取魔法珠！");
            }
        }
        else
        {
            Debug.LogWarning("[AimPointAttack] SwordPrefab或Global_ObjectPool.Instance未设置！");
        }
    }
    
    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    /// <returns>随机生成位置</returns>
    private Vector3 GetRandomSpawnPosition()
    {
        // 随机选择x轴的区间：-20~-15 或 15~20
        float x;
        if (Random.value > 0.5f)
        {
            // 左半区间：-20~-15
            x = Random.Range(xMin, xMax);
        }
        else
        {
            // 右半区间：15~20
            x = Random.Range(-xMax, -xMin);
        }
        
        // 随机y轴位置：15~20
        float y = Random.Range(yMin, yMax);
        
        // 随机z轴位置：15~20
        float z = Random.Range(zMin, zMax);
        
        return new Vector3(x, y, z);
    }
    
    /// <summary>
    /// 魔法珠的淡入协程
    /// </summary>
    /// <param name="sword">魔法珠对象</param>
    private IEnumerator FadeInSword(GameObject sword)
    {
        if (sword == null) yield break;
        
        if (!sword.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) yield break;
        
        // 初始透明度设为0
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        
        // 淡入效果
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // 确保最终透明度为1
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }
    
    void Update()
    {
        // 持续旋转瞄准点
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        
        // 每6帧更新一次神秘珠状态
        frameCounter++;
        if (frameCounter >= 6)
        {
            UpdateSword();
            frameCounter = 0;
        }
    }
    
    /// <summary>
    /// 更新魔法珠状态
    /// </summary>
    private void UpdateSword()
    {
        if (Sword == null)
        {
            // 魔法珠为空时不输出警告，避免日志刷屏
            return;
        }
        
        // 计算魔法珠到瞄准点的方向和距离
        Vector3 direction = (transform.position - Sword.transform.position).normalized;
        float distance = Vector3.Distance(Sword.transform.position, transform.position);
        
        // 检查魔法珠是否到达目的地
        if (distance < 0.5f) // 到达阈值
        {
            // 计算伤害
            int damage = CalculateDamage();
            
            // 对敌人造成伤害
            DealDamageToEnemy(damage);
            
            // 回收魔法珠到对象池
            if (Global_ObjectPool.Instance != null)
            {
                Global_ObjectPool.Instance.Recycle(Sword);
                Sword = null;
            }
            
            // 播放爆炸动画
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(PlayExplosionAnimation());
            }
            
            return;
        }
        
        // 魔法珠旋转
        Sword.transform.Rotate(0f, 0f, 360f * Time.deltaTime); // 每秒旋转360度
        
        // 魔法珠向瞄准点飞行
        Vector3 moveAmount = direction * SwordSpeed * Time.deltaTime * 10f; // 乘以10以补偿每10帧更新一次
        Sword.transform.position += moveAmount;
    }
    
    /// <summary>
    /// 计算伤害
    /// </summary>
    /// <returns>计算后的伤害值</returns>
    private int CalculateDamage()
    {
        // 基础伤害60，加上根据GameManager.power计算的额外伤害
        int baseDamage = 60;
        int powerBonus = 0;
        
        // 尝试获取GameManager实例并计算额外伤害
        if (Global_GameManager.Instance != null)
        {
            powerBonus = Global_GameManager.Instance.Power / 100 * 25;
        }
        
        return baseDamage + powerBonus;
    }
    
    /// <summary>
    /// 对敌人造成伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    private void DealDamageToEnemy(int damage)
    {
        // 检查父对象是否是敌人
        if (transform.parent != null)
        {
            Enemy enemy = transform.parent.GetComponent<Enemy>();
            if (enemy != null)
            {
                // 调用敌人的damage方法
                enemy.Damage(damage);
            }
            if (transform.parent.CompareTag("Boss"))
            {
                // 对Boss造成伤害
                transform.parent.GetComponent<BossBase>().TakeDamage(damage);
            }
        }
    }
    
    /// <summary>
    /// 播放爆炸动画
    /// </summary>
    private IEnumerator PlayExplosionAnimation()
    {
        // 确保 spriteRenderer 已初始化
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 检查精灵数组和SpriteRenderer是否有效
        if (spriteRenderer != null && markerSprites != null && markerSprites.Length >= 4)
        {
            // 播放爆炸动画（第2、3、4帧）
            for (int i = 1; i < 4; i++)
            {
                spriteRenderer.sprite = markerSprites[i];
                // 等待指定的帧间隔
                for (int j = 0; j < explosionFrameInterval; j++)
                {
                    yield return null;
                }
            }
            
            // 切回第1帧（瞄准点）
            spriteRenderer.sprite = markerSprites[0];
        }
        
        // 延迟一段时间后再次召唤魔法珠
        float randomDelay = Random.Range(reuseDelayMin, reuseDelayMax);
        yield return new WaitForSeconds(randomDelay);
        
        // 再次召唤魔法珠
        if (gameObject.activeInHierarchy)
        {
            SpawnSword();
        }
    }

    /// <summary>
    /// 设置魔法珠预制件
    /// </summary>
    /// <param name="prefab">魔法珠预制件</param>
    public void SetSwordPrefab(GameObject prefab)
    {
        // 取消之前的Invoke调用，避免重复生成魔法珠
        CancelInvoke(nameof(SpawnSword));
        
        // 确保 spriteRenderer 已初始化
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        SwordPrefab = prefab;
        // 如果魔法珠还未生成，立即生成
        if (Sword == null)
        {
            SpawnSword();
        }
    }

    public void SetSword(GameObject sword)
    {
        Sword = sword;
    }
}

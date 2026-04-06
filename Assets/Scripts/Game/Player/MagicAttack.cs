using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魔理沙的瞄准攻击
/// 当完全进入魔法态时，搜寻场景中尚未被标记的敌人并标记它们
/// </summary>
public class MagicAttack : MonoBehaviour
{
    [Header("标记预制体")]
    public GameObject markerPrefab; // 瞄准标记预制体
    
    [Header("神秘珠预制体")]
    public GameObject swordPrefab; // 神秘珠预制体
    
    [Header("场景对象")]
    public GameObject evilEye; // 场景中的恶魔之眼对象
    public GameObject evilShadow; // 场景中的 EvilShadow 对象
    
    [Header("生成参数")]
    public float spawnDelayMin = 1f; // 生成瞄准点的最小延迟时间
    public float spawnDelayMax = 2f; // 生成瞄准点的最大延迟时间
    public float markerSpawnChance = 0.4f; // 为敌人添加瞄准点的概率（40%）
    public float switchToEvilEyeTime = 13f; // 切换到恶魔之眼攻击的时间（秒）
    public float evilEyeFadeDuration = 1f; // 恶魔之眼淡入淡出时间
    public float evilShadowFadeOutDuration = 3f; // EvilShadow 淡出时间
    
    private readonly float fadeInDuration = 1f; // 标记淡入时间
    
    private bool isMagicActive = false; // 魔法态是否激活
    private bool isEvilEyeActive = false; // 恶魔之眼是否激活
    private float magicTimer = 0f; // 魔法态计时器
    private List<GameObject> activeMarkers = new (); // 当前活跃的标记列表
    private int frameCounter = 0; // 帧计数器，用于每10帧扫描一次敌人

    public EvilEyeAttack evilEyeAttack; // 恶魔之眼攻击脚本

    void Awake()
    {
        InitPool();
    }

    void OnEnable()
    {
        isMagicActive = false;
        isEvilEyeActive = false;
        magicTimer = 0f;
        frameCounter = 0;
        
        // 确保恶魔之眼和 EvilShadow 初始透明度为0
        if (evilEye != null)
        {
            // 确保初始透明度为0
            if (evilEye.TryGetComponent<SpriteRenderer>(out var evilEyeRenderer))
            {
                Color color = evilEyeRenderer.color;
                color.a = 0f;
                evilEyeRenderer.color = color;
            }
        }
        if (evilShadow != null)
        {
            // 确保初始透明度为0
            if (evilShadow.TryGetComponent<SpriteRenderer>(out var evilShadowRenderer))
            {
                Color color = evilShadowRenderer.color;
                color.a = 0f;
                evilShadowRenderer.color = color;
            }
        }
        
        // 2秒后进入魔法态
        Invoke(nameof(EnterMagicState), 2f);
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        // 取消Invoke调用
        CancelInvoke(nameof(EnterMagicState));
        
        // 清理所有活跃的标记
        ClearAllMarkers();
        
        // 调用恶魔之眼的淡出方法
        if (evilEye != null)
        {
            if (evilEye.TryGetComponent<EvilEyeAttack>(out var evilEyeAttack))
            {
                evilEyeAttack.StartFadeOut();
                evilEyeAttack.isFadeInComplete = false;
                evilEyeAttack.ClearAllLasers();
            }
        }
        
        // 调用 EvilShadow 的淡出方法
        if (evilShadow != null)
        {
            EvilShadow evilShadowScript = evilShadow.GetComponent<EvilShadow>();
            if (evilShadowScript != null)
            {
                evilShadowScript.StartFadeOut();
            }
        }
    }
    
    void Update()
    {
        // 当魔法态激活且未切换到恶魔之眼时，更新计时器
        if (isMagicActive && !isEvilEyeActive)
        {
            magicTimer += Time.deltaTime;
            
            // 检查是否达到切换时间
            if (magicTimer >= switchToEvilEyeTime)
            {
                SwitchToEvilEyeAttack();
            }
            
            // 每10帧扫描一次敌人
            if (Input.GetKey(KeyCode.Z))
            {
                frameCounter++;
                if (frameCounter >= 10)
                {
                    MarkAllEnemies();
                    frameCounter = 0;
                }
            }
        }
    }

    private void InitPool()
    {
        // 初始化标记对象池和神秘珠对象池
        if (Global_ObjectPool.Instance != null)
        {
            // 初始化标记对象池
            if (markerPrefab != null)
            {
                Global_ObjectPool.Instance.InitPool(markerPrefab, 10);
            }
            else
            {
                Debug.LogError("MagicAttack: markerPrefab 未设置，无法初始化标记对象池！");
            }
            
            // 初始化神秘珠对象池，数量与标记对象池相同
            if (swordPrefab != null)
            {
                Global_ObjectPool.Instance.InitPool(swordPrefab, 10);
            }
            else
            {
                Debug.LogError("MagicAttack: swordPrefab 未设置，无法初始化神秘珠对象池！");
            }
        }
        else
        {
            Debug.LogError("MagicAttack: Global_ObjectPool 实例未找到，无法初始化对象池！");
        }
    }
    
    /// <summary>
    /// 进入魔法态
    /// </summary>
    private void EnterMagicState()
    {
        Time.timeScale = 0.9f;
        isMagicActive = true;
    }
    
    /// <summary>
    /// 从对象池获取标记
    /// </summary>
    private GameObject GetMarkerFromPool()
    {
        if (Global_ObjectPool.Instance != null && markerPrefab != null)
        {
            return Global_ObjectPool.Instance.GetObject(markerPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogError("MagicAttack: 无法从对象池获取标记，Global_ObjectPool 实例或 markerPrefab 未设置！");
            return null;
        }
    }
    
    /// <summary>
    /// 回收标记到对象池
    /// </summary>
    public void RecycleMarker(GameObject marker)
    {
        if (marker != null && Global_ObjectPool.Instance != null)
        {
            // 解除父子关系
            if (marker.transform.parent != null)
            {
                marker.transform.parent = null;
            }
            
            // 重置标记状态
            if (marker.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                spriteRenderer.color = new Color(1, 1, 1, 0f);
            }
            
            // 回收标记到对象池
            Global_ObjectPool.Instance.Recycle(marker);
        }
    }
    
    /// <summary>
    /// 标记所有未被标记的敌人
    /// </summary>
    private void MarkAllEnemies()
    {
        
        if (Global_GameManager.Instance == null)
        {
            Debug.LogWarning("[MagicAttack] Global_GameManager 实例未找到！");
            return;
        }
        
        // 获取所有敌人
        List<GameObject> enemies = Global_GameManager.Instance.EnemyList;
        
        if (enemies == null || enemies.Count == 0)
        {
            return;
        }
        
        // 遍历所有敌人，标记未被标记的
        int markedCount = 0;
        foreach (GameObject enemyObj in enemies)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (!enemy.isMarked)
                {
                    CreateMarkerForEnemy(enemy);
                    markedCount++;
                }
            }
            else
            {
                Debug.LogWarning($"[MagicAttack] 敌人对象 {enemyObj.name} 没有Enemy组件");
            }
        }
    }
    
    /// <summary>
    /// 为敌人创建标记
    /// </summary>
    private void CreateMarkerForEnemy(Enemy enemy)
    {
        // 60%的概率不为敌人添加瞄准点
        if (Random.value > markerSpawnChance)
        {
            return;
        }
        
        // 从对象池获取标记
        GameObject marker = GetMarkerFromPool();
        if (marker == null)
        {
            Debug.LogError("[MagicAttack] 无法从对象池获取标记！");
            return;
        }
        
        // 设置标记位置和父物体
        marker.transform.position = enemy.transform.position;
        marker.transform.SetParent(enemy.transform);
        
        // 重置标记状态
        if (marker.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            spriteRenderer.color = new Color(1, 1, 1, 0f);
        }
        
        // 标记敌人
        enemy.aimMarker = marker;
        enemy.isMarked = true;
        
        // 添加到活跃标记列表
        activeMarkers.Add(marker);
        
        // 随机延迟开始淡入动画
        float randomDelay = Random.Range(spawnDelayMin, spawnDelayMax);
        StartCoroutine(DelayedFadeIn(marker, randomDelay, swordPrefab));
    }
    
    /// <summary>
    /// 延迟淡入协程
    /// </summary>
    /// <param name="marker">标记对象</param>
    /// <param name="delay">延迟时间</param>
    /// <param name="swordPrefab">神秘珠预制件</param>
    private IEnumerator DelayedFadeIn(GameObject marker, float delay, GameObject swordPrefab)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(FadeInMarker(marker));
        
        // 淡入完成后设置神秘珠预制件并生成神秘珠
        if (swordPrefab != null)
        {
            if (marker.TryGetComponent<AimPointAttack>(out var aimPointAttack))
            {
                aimPointAttack.SetSwordPrefab(swordPrefab);
            }
            else
            {
                Debug.LogWarning("[MagicAttack] 标记预制体没有AimPointAttack组件！");
            }
        }
    }
    
    /// <summary>
    /// 标记淡入协程
    /// </summary>
    private IEnumerator FadeInMarker(GameObject marker)
    {
        if (!marker.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            Debug.LogWarning("MagicAttack: 标记预制体没有SpriteRenderer组件！");
            yield break;
        }
        
        float elapsedTime = 0f;
        
        // 从透明度0渐入到1
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        // 确保最终透明度为1
        spriteRenderer.color = new Color(1, 1, 1, 1f);
    }
    
    /// <summary>
    /// 清理所有标记
    /// </summary>
    private void ClearAllMarkers()
    {
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null)
            {
                // 回收标记到对象池
                RecycleMarker(marker);
            }
        }
        activeMarkers.Clear();
    }
    
    /// <summary>
    /// 切换到恶魔之眼攻击方式
    /// </summary>
    private void SwitchToEvilEyeAttack()
    {
        // 清理所有活跃的标记
        ClearAllMarkers();
        
        // 淡入恶魔之眼
        if (evilEye != null)
        {
            if (evilEye.TryGetComponent<EvilEyeAttack>(out var evilEyeAttack))
            {
                evilEyeAttack.StartFadeIn();
            }
            else{
                Debug.LogWarning($"[MagicAttack] 恶魔之眼对象 {evilEye.name} 没有EvilEyeAttack组件");
            }
        }
        else{
            Debug.LogWarning("[MagicAttack] 恶魔之眼对象未设置");
        }
        
        // 激活 EvilShadow
        if (evilShadow != null)
        {
            EvilShadow evilShadowScript = evilShadow.GetComponent<EvilShadow>();
            if (evilShadowScript != null)
            {
                evilShadowScript.StartFadeIn();
            }
        }
        
        // 切换状态
        isEvilEyeActive = true;
    }
}

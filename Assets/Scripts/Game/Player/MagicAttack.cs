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
    
    [Header("标记参数")]
    public float fadeInDuration = 1f; // 标记淡入时间
    
    private bool isMagicActive = false; // 魔法态是否激活
    private bool hasMarkedEnemies = false; // 是否已经标记过敌人
    private List<GameObject> activeMarkers = new List<GameObject>(); // 当前活跃的标记列表
    private Queue<GameObject> markerPool = new Queue<GameObject>(); // 标记对象池
    private bool isPoolInitialized = false; // 对象池是否已初始化

    void Awake()
    {
        // 初始化标记对象池（仅调用一次）
        if (!isPoolInitialized)
        {
            InitializeMarkerPool();
            isPoolInitialized = true;
        }
    }
    
    void OnEnable()
    {
        isMagicActive = false;
        hasMarkedEnemies = false;
        
        // 2秒后进入魔法态
        Invoke(nameof(EnterMagicState), 2f);
    }
    
    void Update()
    {
        // 当魔法态激活且尚未标记敌人时，执行标记逻辑
        if (isMagicActive && !hasMarkedEnemies)
        {
            MarkAllEnemies();
            hasMarkedEnemies = true;
        }
    }
    
    /// <summary>
    /// 进入魔法态
    /// </summary>
    private void EnterMagicState()
    {
        isMagicActive = true;
    }
    
    /// <summary>
    /// 初始化标记对象池
    /// </summary>
    private void InitializeMarkerPool()
    {
        if (markerPrefab == null)
        {
            Debug.LogError("MagicAttack: markerPrefab 未设置，无法初始化对象池！");
            return;
        }
        
        // 生成10个标记对象到对象池
        for (int i = 0; i < 10; i++)
        {
            GameObject marker = Instantiate(markerPrefab);
            marker.SetActive(false);
            markerPool.Enqueue(marker);
        }
        
        Debug.Log("MagicAttack: 标记对象池初始化完成，数量: " + markerPool.Count);
    }
    
    /// <summary>
    /// 从对象池获取标记
    /// </summary>
    private GameObject GetMarkerFromPool()
    {
        if (markerPool.Count > 0)
        {
            GameObject marker = markerPool.Dequeue();
            marker.SetActive(true);
            return marker;
        }
        else
        {
            // 对象池不足时，创建新的标记
            if (markerPrefab == null)
            {
                Debug.LogError("MagicAttack: markerPrefab 未设置，无法创建新标记！");
                return null;
            }
            
            GameObject marker = Instantiate(markerPrefab);
            marker.SetActive(true);
            return marker;
        }
    }
    
    /// <summary>
    /// 回收标记到对象池
    /// </summary>
    public void RecycleMarker(GameObject marker)
    {
        if (marker != null)
        {
            marker.SetActive(false);
            marker.transform.parent = null;
            markerPool.Enqueue(marker);
        }
    }
    
    /// <summary>
    /// 标记所有未被标记的敌人
    /// </summary>
    private void MarkAllEnemies()
    {
        if (Global_GameManager.Instance == null)
        {
            Debug.LogWarning("MagicAttack: Global_GameManager 实例未找到！");
            return;
        }
        
        // 获取所有敌人
        List<GameObject> enemies = Global_GameManager.Instance.EnemyList;
        
        if (enemies == null || enemies.Count == 0)
        {
            Debug.Log("MagicAttack: 当前场景中没有敌人！");
            return;
        }
        
        // 遍历所有敌人，标记未被标记的
        foreach (GameObject enemyObj in enemies)
        {
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null && !enemy.isMarked)
            {
                CreateMarkerForEnemy(enemy);
            }
        }
    }
    
    /// <summary>
    /// 为敌人创建标记
    /// </summary>
    private void CreateMarkerForEnemy(Enemy enemy)
    {
        // 从对象池获取标记
        GameObject marker = GetMarkerFromPool();
        if (marker == null)
        {
            return;
        }
        
        // 设置标记位置和父物体
        marker.transform.position = enemy.transform.position;
        marker.transform.SetParent(enemy.transform);
        
        // 重置标记状态
        SpriteRenderer spriteRenderer = marker.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            spriteRenderer.color = new Color(color.r, color.g, color.b, 0f);
        }
        
        // 标记敌人
        enemy.aimMarker = marker;
        enemy.isMarked = true;
        
        // 添加到活跃标记列表
        activeMarkers.Add(marker);
        
        // 开始淡入动画
        StartCoroutine(FadeInMarker(marker));
    }
    
    /// <summary>
    /// 标记淡入协程
    /// </summary>
    private IEnumerator FadeInMarker(GameObject marker)
    {
        SpriteRenderer spriteRenderer = marker.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("MagicAttack: 标记预制体没有SpriteRenderer组件！");
            yield break;
        }
        
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        
        // 从透明度0渐入到1
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
    
    void OnDisable()
    {
        // 取消Invoke调用
        CancelInvoke(nameof(EnterMagicState));
        
        // 清理所有活跃的标记
        ClearAllMarkers();
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
                // 解除父子关系
                if (marker.transform.parent != null)
                {
                    marker.transform.parent = null;
                }
                
                // 回收标记到对象池
                RecycleMarker(marker);
            }
        }
        activeMarkers.Clear();
    }
}
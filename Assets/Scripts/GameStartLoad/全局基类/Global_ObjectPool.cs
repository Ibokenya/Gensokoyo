using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局"池"型单例类 —— 已改为确定性实现。
/// 所有活跃对象集合用 List（顺序稳定），不碰 HashSet（遍历顺序不保证）。
/// </summary>
public class Global_ObjectPool : Singleton<Global_ObjectPool>   
{
    // 存储不同类型的物品池
    private readonly Dictionary<string, Queue<GameObject>> ObjectPool = new();
    // 存储每个对象池的初始容量
    private readonly Dictionary<string, int> PoolInitialCapacities = new();
    // 存储当前活跃的对象 —— 改为 List，遍历顺序稳定（Get/Recycle 的时间顺序）
    private readonly Dictionary<string, List<GameObject>> ActiveObjects = new();

    [Header("预生成数量")]
    public int ObjectsInPool_Count = 40;

    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效
    }

    /// <summary>
    /// 初始化物品池
    /// </summary>
    public void InitPool(GameObject itemPrefab, int count)   
    {
        if(count == 0) count = ObjectsInPool_Count;
        string poolKey = itemPrefab.name;
        if (ObjectPool.ContainsKey(poolKey)) return;
        Queue<GameObject> pool = new();
        ObjectPool.Add(poolKey, pool);
        PoolInitialCapacities.Add(poolKey, count);
        for (int i = 0; i < count; i++)
        {
            GameObject item = Instantiate(itemPrefab);
            item.SetActive(false);
            pool.Enqueue(item);
        }
    }

    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    public GameObject GetObject(GameObject itemPrefab, Vector3 position, Quaternion rotation)
    {
        if (itemPrefab == null) {
            Debug.LogError("GetObject: itemPrefab is null");
            return null;
        }
        
        string poolKey = itemPrefab.name;
        
        GameObject item;
        if (!ObjectPool.ContainsKey(poolKey))
        {
            InitPool(itemPrefab, 10);
        }
        
        CheckAndExpandPool(itemPrefab);
        
        if(ObjectPool[poolKey].Count > 0)
        {
            item = ObjectPool[poolKey].Dequeue();
        }
        else
        {
            item = Instantiate(itemPrefab,transform);
        }
        item.transform.SetPositionAndRotation(position, rotation);
        item.SetActive(true);
        
        // 添加到活跃 List（顺序 = GetObject 调用顺序，稳定）
        if (!ActiveObjects.ContainsKey(poolKey))
        {
            ActiveObjects[poolKey] = new List<GameObject>();
        }
        ActiveObjects[poolKey].Add(item);
        
        return item;
    }
    
    /// <summary>
    /// 检查并动态扩容对象池
    /// </summary>
    private void CheckAndExpandPool(GameObject itemPrefab)
    {
        string poolKey = itemPrefab.name;
        if (!ObjectPool.ContainsKey(poolKey) || !PoolInitialCapacities.ContainsKey(poolKey)) return;
        
        Queue<GameObject> pool = ObjectPool[poolKey];
        int idleCount = pool.Count;
        int totalCapacity = PoolInitialCapacities[poolKey];
        
        if (idleCount < totalCapacity * 0.1f)
        {
            int expandCount = Mathf.CeilToInt(totalCapacity * 0.5f);
            int newTotalCapacity = totalCapacity + expandCount;
            
            for (int i = 0; i < expandCount; i++)
            {
                GameObject item = Instantiate(itemPrefab, transform);
                item.SetActive(false);
                pool.Enqueue(item);
            }
            
            PoolInitialCapacities[poolKey] = newTotalCapacity;
        }
    }

    /// <summary>
    /// 回收物品到池子里
    /// </summary>
    public void Recycle(GameObject item)
    {
        if (item == null || !item) return;
        bool wasActive;
        try
        {
            wasActive = item.activeSelf;
        }
        catch
        {
            StartCoroutine(DelayedRecycle(item));
            return;
        }
        
        string poolKey = item.name.Replace("(Clone)", "");
        wasActive = item.activeSelf;
        
        if (wasActive) item.SetActive(false);
        
        try
        {
            if (item.transform.parent != null) item.transform.SetParent(null);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"回收物品时解除父物体关系失败：{e.Message}");
        }
        
        try
        {
            if (gameObject != null && gameObject.scene != null && gameObject.scene.isLoaded && gameObject.activeInHierarchy)
                item.transform.SetParent(transform);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"回收物品时设置父物体失败：{e.Message}");
            try { item.transform.SetParent(null); } catch { }
        }

        // 从活跃 List 中移除
        if (ActiveObjects.TryGetValue(poolKey, out var list))
        {
            list.Remove(item);
        }
        
        if (ObjectPool.ContainsKey(poolKey))
        {
            ObjectPool[poolKey].Enqueue(item);
        }
        else
        {
            Destroy(item);
        }
    }
    
    private IEnumerator DelayedRecycle(GameObject item)
    {
        yield return null;
        if (item != null && item) Recycle(item);
    }

    /// <summary>
    /// 公共查询：遍历某类型的所有活跃对象（List 顺序稳定）
    /// 用于 ClearAllBullet / BossShootSystem 等脚本，替代 FindGameObjectsWithTag（顺序不保证）。
    /// </summary>
    public IReadOnlyList<GameObject> GetActiveObjects(string poolKey)
    {
        return ActiveObjects.TryGetValue(poolKey, out var list) ? list : System.Array.Empty<GameObject>();
    }

    /// <summary>遍历所有活跃对象类型的 key（供遍历所有活跃池用）</summary>
    public IEnumerable<string> GetAllActivePoolKeys() => ActiveObjects.Keys;

    /// <summary>是否有任意活跃对象</summary>
    public bool HasActiveObjects => ActiveObjects.Count > 0;

    /// <summary>
    /// 禁用并回收所有活跃对象（场景切换时使用）
    /// </summary>
    public void DisableAndRecycleAllActiveObjects()
    {
        foreach (var kvp in ActiveObjects)
        {
            string poolKey = kvp.Key;
            List<GameObject> activeList = kvp.Value;
            
            // List 可以直接遍历，不必先拷贝
            for (int i = 0; i < activeList.Count; i++)
            {
                GameObject item = activeList[i];
                if (item != null)
                {
                    try
                    {
                        item.SetActive(false);
                        if (item.transform.parent != null) item.transform.SetParent(null);
                        if (gameObject != null && gameObject.scene != null && gameObject.scene.isLoaded)
                            item.transform.SetParent(transform);
                        if (ObjectPool.ContainsKey(poolKey))
                            ObjectPool[poolKey].Enqueue(item);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"回收活跃对象失败：{e.Message}");
                    }
                }
            }
            
            activeList.Clear();
        }
        
        Debug.Log("已禁用并回收所有活跃对象");
    }
    
    /// <summary>
    /// 清空所有物品池
    /// </summary>
    public void ClearAllPools()
    {
        DisableAndRecycleAllActiveObjects();
        
        foreach (var pool in ObjectPool.Values)
        {
            while (pool.Count > 0)
            {
                GameObject item = pool.Dequeue();
                Destroy(item);
            }
        }
        ObjectPool.Clear();
        PoolInitialCapacities.Clear();
        ActiveObjects.Clear();
        Debug.Log("清空所有物品池");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局“池”型单例类
/// 管理妖精池，弹幕池，得点道具池
/// 都是为了优化性能
/// </summary>
public class Global_ObjectPool : Singleton<Global_ObjectPool>   
{
    // 存储不同类型的物品池
    private readonly Dictionary<string, Queue<GameObject>> ObjectPool = new();
    // 存储每个对象池的初始容量
    private readonly Dictionary<string, int> PoolInitialCapacities = new();
    // 存储当前活跃的对象（已从池中取出但尚未回收）
    private readonly Dictionary<string, HashSet<GameObject>> ActiveObjects = new();
    [Header("预生成数量")]
    public int ObjectsInPool_Count = 40;

    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效
    }

    /// <summary>
    /// 初始化物品池
    /// </summary>
    /// <param name="itemPrefab">对象预制体</param>
    /// <param name="count">预生成数量</param>
    public void InitPool(GameObject itemPrefab,int count)   
    {
        if(count == 0) count = ObjectsInPool_Count;
        string poolKey = itemPrefab.name;
        if (ObjectPool.ContainsKey(poolKey)) return;// 如果已经初始化过，直接返回
        Queue<GameObject> pool = new();
        ObjectPool.Add(poolKey, pool);
        PoolInitialCapacities.Add(poolKey, count); // 记录初始容量
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
    /// <param name="itemPrefab">对象预制体</param>
    /// <param name="position">生成位置</param>
    /// <param name="rotation">生成旋转</param>
    /// <returns>获取到的对象</returns>
    public GameObject GetObject(GameObject itemPrefab, Vector3 position, Quaternion rotation)
    {
        if (itemPrefab == null) {
            Debug.LogError("GetObject: itemPrefab is null");
            return null;
        }
        
        string poolKey = itemPrefab.name;
        
        GameObject item;
        // 检查是否有初始化过该类型的池
        if (!ObjectPool.ContainsKey(poolKey))
        {
            InitPool(itemPrefab,10); // 未初始化则自动初始化
        }
        
        // 检查池容量并动态扩容
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
        
        // 将对象添加到活跃对象集合
        if (!ActiveObjects.ContainsKey(poolKey))
        {
            ActiveObjects[poolKey] = new HashSet<GameObject>();
        }
        ActiveObjects[poolKey].Add(item);
        
        return item;
    }
    
    /// <summary>
    /// 检查并动态扩容对象池
    /// </summary>
    /// <param name="itemPrefab">对象预制体</param>
    private void CheckAndExpandPool(GameObject itemPrefab)
    {
        string poolKey = itemPrefab.name;
        if (!ObjectPool.ContainsKey(poolKey) || !PoolInitialCapacities.ContainsKey(poolKey)) return;
        
        Queue<GameObject> pool = ObjectPool[poolKey];
        int idleCount = pool.Count; // 当前空闲对象数量
        int totalCapacity = PoolInitialCapacities[poolKey]; // 总容量
        
        // 检查空闲容量是否小于10%
        if (idleCount < totalCapacity * 0.1f)
        {
            // Debug.Log($"对象池 {poolKey} 空闲容量小于10%{idleCount}/{totalCapacity}，触发扩容");
            // 扩容50%
            int expandCount = Mathf.CeilToInt(totalCapacity * 0.5f);
            int newTotalCapacity = totalCapacity + expandCount;
            
            // 向池中添加新对象
            for (int i = 0; i < expandCount; i++)
            {
                GameObject item = Instantiate(itemPrefab, transform);
                item.SetActive(false);
                pool.Enqueue(item);
            }
            
            // 更新总容量
            PoolInitialCapacities[poolKey] = newTotalCapacity;
            
            //Debug.Log($"对象池 {poolKey} 已扩容，新增 {expandCount} 个对象，总容量: {newTotalCapacity}，当前空闲: {pool.Count}");
        }
    }

    /// <summary>
    /// 回收物品到池子里
    /// </summary>
    /// <param name="item">要回收的物品</param>
    public void Recycle(GameObject item)
    {
        if (item == null || !item) return; // 安全检查：确保物品存在
        bool wasActive;
        // 检查物品是否正在被激活或禁用
        try
        {
            // 尝试访问物品的activeSelf属性，如果正在被激活/禁用会抛出异常
            wasActive = item.activeSelf;
        }
        catch
        {
            // 物品正在被激活或禁用，延迟一帧再处理
            StartCoroutine(DelayedRecycle(item));
            return;
        }
        
        string poolKey = item.name.Replace("(Clone)", ""); // 移除克隆后缀，匹配预制体名
        
        // 先保存当前状态
        wasActive = item.activeSelf;
        
        // 禁用物品
        if (wasActive)
        {
            item.SetActive(false);
        }
        
        // 先解除当前父物体关系
        try
        {
            if (item.transform.parent != null)
            {
                item.transform.SetParent(null);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"回收物品时解除父物体关系失败：{e.Message}");
        }
        
        // 再设置为对象池的子物体
        try
        {
            // 确保对象池游戏对象是活跃的且场景已加载
            if (gameObject != null && gameObject.scene != null && gameObject.scene.isLoaded && gameObject.activeInHierarchy)
            {
                item.transform.SetParent(transform); // 归位到对象池父物体
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"回收物品时设置父物体失败：{e.Message}");
            // 发生异常时，确保父物体为null
            try
            {
                item.transform.SetParent(null);
            }
            catch { }
        }

        // 从活跃对象集合中移除
        if (ActiveObjects.ContainsKey(poolKey))
        {
            ActiveObjects[poolKey].Remove(item);
        }
        
        // 确保池存在，再放回
        if (ObjectPool.ContainsKey(poolKey))
        {
            ObjectPool[poolKey].Enqueue(item);
        }
        else
        {
            // 未初始化的池：直接销毁
            Destroy(item);
            //  Debug.LogWarning($"回收一个不在物品池的物品：{poolKey}");
        }
    }
    
    /// <summary>
    /// 延迟回收物品，避免在激活/禁用过程中修改父物体
    /// </summary>
    private IEnumerator DelayedRecycle(GameObject item)
    {
        yield return null; // 等待一帧
        if (item != null && item)
        {
            Recycle(item);
        }
    }

    /// <summary>
    /// 禁用并回收所有活跃对象（场景切换时使用）
    /// </summary>
    public void DisableAndRecycleAllActiveObjects()
    {
        // 遍历所有活跃对象类型
        foreach (var kvp in ActiveObjects)
        {
            string poolKey = kvp.Key;
            HashSet<GameObject> activeSet = kvp.Value;
            
            // 创建临时列表避免在遍历过程中修改集合
            List<GameObject> activeList = new List<GameObject>(activeSet);
            
            foreach (GameObject item in activeList)
            {
                if (item != null)
                {
                    try
                    {
                        // 禁用对象
                        item.SetActive(false);
                        
                        // 解除父物体关系
                        if (item.transform.parent != null)
                        {
                            item.transform.SetParent(null);
                        }
                        
                        // 设置为对象池的子物体
                        if (gameObject != null && gameObject.scene != null && gameObject.scene.isLoaded)
                        {
                            item.transform.SetParent(transform);
                        }
                        
                        // 放回对象池
                        if (ObjectPool.ContainsKey(poolKey))
                        {
                            ObjectPool[poolKey].Enqueue(item);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"回收活跃对象失败：{e.Message}");
                    }
                }
            }
            
            // 清空该类型的活跃对象集合
            activeSet.Clear();
        }
        
        Debug.Log("已禁用并回收所有活跃对象");
    }
    
    /// <summary>
    /// 清空所有物品池
    /// </summary>
    public void ClearAllPools()
    {
        // 先禁用并回收所有活跃对象
        DisableAndRecycleAllActiveObjects();
        
        // 销毁池中所有对象
        foreach (var pool in ObjectPool.Values)
        {
            while (pool.Count > 0)
            {
                GameObject item = pool.Dequeue();
                Destroy(item);
            }
        }
        ObjectPool.Clear();
        PoolInitialCapacities.Clear(); // 清空初始容量字典
        ActiveObjects.Clear(); // 清空活跃对象集合
        Debug.Log("清空所有物品池");
    }
}

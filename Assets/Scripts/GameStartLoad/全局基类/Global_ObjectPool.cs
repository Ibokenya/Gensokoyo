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
        string poolKey = itemPrefab.name;
        GameObject item;
        // 检查是否有初始化过该类型的池
        if (!ObjectPool.ContainsKey(poolKey))
        {
            InitPool(itemPrefab,10); // 未初始化则自动初始化
            Debug.Log($"创建一个不在物品池的物品：{poolKey}");
        }
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
        return item;
    }

    /// <summary>
    /// 回收物品到池子里
    /// </summary>
    /// <param name="item">要回收的物品</param>
    public void Recycle(GameObject item)
    {
        string poolKey = item.name.Replace("(Clone)", ""); // 移除克隆后缀，匹配预制体名
        item.SetActive(false);
        item.transform.SetParent(transform); // 归位到对象池父物体

        // 确保池存在，再放回
        if (ObjectPool.ContainsKey(poolKey))
        {
            ObjectPool[poolKey].Enqueue(item);
        }
        else
        {
            // 未初始化的池：直接销毁
            Destroy(item);
            Debug.Log($"回收一个不在物品池的物品：{poolKey}");
        }
    }

    /// <summary>
    /// 清空所有物品池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in ObjectPool.Values)
        {
            while (pool.Count > 0)
            {
                GameObject item = pool.Dequeue();
                Destroy(item);
            }
        }
        ObjectPool.Clear();
        Debug.Log("清空所有物品池");
    }
}

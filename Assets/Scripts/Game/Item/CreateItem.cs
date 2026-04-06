using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 掉落物类型枚举
public enum ItemType
{
    HPPlus = 0,     // HP+
    HP = 1,         // HP
    PowerPlus = 2,  // Power+
    Power = 3,      // Power
    BombPlus = 4,   // Bomb+
    Bomb = 5,       // Bomb
    Grade = 6,      // Grade
    GradeMinus = 7, // Grade-
    GradeMinusMinus = 8 // Grade--
}

// 掉落物配置类
[System.Serializable]
public class ItemDropConfig
{
    public ItemType itemType; // 掉落物类型
    public int count;         // 掉落数量
}

public class CreateItem : MonoBehaviour
{
    [Header("物品预制体列表")]
    public List<GameObject> ItemPrefabs;
    [Header("初始化物品数量列表（对应物品预制体列表）")]
    public List<int> ItemCount;
    
    [Header("掉落物生成参数")]
    public float spawnOffset = 0.5f; // 掉落物生成位置的随机偏移范围
    public GameObject player;
    [Header("收集音效")]
    public AudioClip collectClip;

    void OnEnable()
    {
        InitItemPool();
    }
    
    /// <summary>
    /// 初始化物品池
    /// </summary>
    private void InitItemPool()
    {
        for (int i = 0; i < ItemPrefabs.Count; i++)
        {
            Global_ObjectPool.Instance.InitPool(ItemPrefabs[i], ItemCount[i]);
        }
    }
    
    /// <summary>
    /// 生成掉落物
    /// </summary>
    /// <param name="position">生成位置</param>
    /// <param name="dropConfigs">掉落物配置列表</param>
    public void SpawnItems(Vector3 position, List<ItemDropConfig> dropConfigs)
    {
        if (dropConfigs == null || dropConfigs.Count == 0)
        {
            return;
        }
        
        if (Global_ObjectPool.Instance == null)
        {
            Debug.LogError("Global_ObjectPool instance not found!");
            return;
        }
        
        // 遍历所有掉落物配置
        foreach (ItemDropConfig config in dropConfigs)
        {
            // 根据掉落物类型获取对应的预制体索引
            int prefabIndex = (int)config.itemType;
            
            // 检查预制体索引是否有效
            if (prefabIndex < 0 || prefabIndex >= ItemPrefabs.Count)
            {
                Debug.LogWarning($"ItemType {config.itemType} 对应的预制体索引 {prefabIndex} 超出范围!");
                continue;
            }
            
            GameObject prefab = ItemPrefabs[prefabIndex];
            if (prefab == null)
            {
                Debug.LogWarning($"ItemType {config.itemType} 对应的预制体为空!");
                continue;
            }
            
            // 生成指定数量的掉落物
            for (int i = 0; i < config.count; i++)
            {
                // 计算随机偏移位置
                Vector3 spawnPosition = position + GetRandomOffset();
                
                // 从对象池获取物品
                GameObject item = Global_ObjectPool.Instance.GetObject(prefab, spawnPosition, Quaternion.identity);
                item.GetComponent<AboutItem>().player = player;
                item.GetComponent<AboutItem>().SetCollectClip(collectClip);
                if (item == null)
                {
                    Debug.LogWarning($"无法从对象池获取物品: {prefab.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取随机偏移向量
    /// </summary>
    /// <returns>随机偏移向量</returns>
    private Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(-spawnOffset, spawnOffset);
        float offsetY = Random.Range(-spawnOffset, spawnOffset);
        return new Vector3(offsetX, offsetY, 0f);
    }
}

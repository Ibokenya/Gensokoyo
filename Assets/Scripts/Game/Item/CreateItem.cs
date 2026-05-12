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
    private static CreateItem _instance;

    public static CreateItem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CreateItem>();
                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject("CreateItem");
                    _instance = singletonObj.AddComponent<CreateItem>();
                }
            }
            return _instance;
        }
    }

    [Header("物品预制体列表")]
    public List<GameObject> ItemPrefabs;
    [Header("初始化物品数量列表（对应物品预制体列表）")]
    public List<int> ItemCount;

    [Header("掉落物生成参数")]
    public float spawnOffset = 0.8f; // 掉落物生成位置的随机偏移范围
    [Header("收集音效")]
    public AudioClip collectClip;

    [Header("得分点参数")]
    public float scoreItemFlySpeed = 15f; // 得分点飞向玩家的速度
    public GameObject player;

    private float lastPowerSpawnTime = -1f; // 上次生成Power道具的时间
    private const float powerSpawnInterval = 0.5f; // 生成间隔

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as CreateItem;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
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
                Debug.LogWarning($"ItemType {config.itemType} 对应的预制体索引 {prefabIndex} 超出了范围!");
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
                AboutItem aboutItem = item.GetComponent<AboutItem>();
                aboutItem.player = player;
                aboutItem.SetCollectClip(collectClip);

                // 判断是否为得分点类型（GradeMinus或GradeMinusMinus），自动飞向玩家
                if (config.itemType == ItemType.GradeMinus || config.itemType == ItemType.GradeMinusMinus)
                {
                    aboutItem.SetAutoFlyToPlayer(true, scoreItemFlySpeed);
                }

                if (item == null)
                {
                    Debug.LogWarning($"无法从对象池获取物品: {prefab.name}");
                }
            }
        }
    }

    /// <summary>
    /// 生成得分点（8~20随机整数，商个GradeMinus + 余数个GradeMinusMinus）
    /// </summary>
    /// <param name="position">生成位置</param>
    public void SpawnScoreItems(Vector3 position)
    {
        if (Global_ObjectPool.Instance == null)
        {
            Debug.LogError("Global_ObjectPool instance not found!");
            return;
        }

        if (player == null)
        {
            return;
        }

        // 生成8~20的随机整数
        int randomValue = Random.Range(8, 21);
        int gradeMinusCount = randomValue / 10;
        int gradeMinusMinusCount = randomValue % 10;

        // 生成GradeMinus
        if (gradeMinusCount > 0)
        {
            SpawnScoreItemType(position, ItemType.GradeMinus, gradeMinusCount, player);
        }

        // 生成GradeMinusMinus
        if (gradeMinusMinusCount > 0)
        {
            SpawnScoreItemType(position, ItemType.GradeMinusMinus, gradeMinusMinusCount, player);
        }

        Debug.Log($"生成得分点: 总值={randomValue}, GradeMinus={gradeMinusCount}, GradeMinusMinus={gradeMinusMinusCount}");
    }

    public void SpawnPowerItems(Vector3 position)
    {
        if (Global_ObjectPool.Instance == null)
        {
            Debug.LogError("Global_ObjectPool instance not found!");
            return;
        }

        // 检查player是否已设置
        if (player == null)
        {
            Debug.LogError("CreateItem.player is not assigned! Please assign it in Inspector.");
            // 尝试自动查找玩家对象
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Cannot find player object with tag 'Player'!");
                return;
            }
        }

        float currentTime = Time.time;
        bool canSpawnPower = (lastPowerSpawnTime < 0f) || (currentTime - lastPowerSpawnTime >= powerSpawnInterval);

        ItemType spawnType = canSpawnPower ? ItemType.Power : ItemType.GradeMinusMinus;
        bool shouldFlyToPlayer = !canSpawnPower; // GradeMinusMinus需要飞向玩家

        int prefabIndex = (int)spawnType;
        if (prefabIndex < 0 || prefabIndex >= ItemPrefabs.Count)
        {
            Debug.LogWarning($"ItemType {spawnType} 对应的预制体索引 {prefabIndex} 超出了范围!");
            return;
        }

        GameObject prefab = ItemPrefabs[prefabIndex];
        if (prefab == null)
        {
            Debug.LogWarning($"ItemType {spawnType} 对应的预制体为空!");
            return;
        }

        Vector3 spawnPosition = position + GetRandomOffset();
        GameObject item = Global_ObjectPool.Instance.GetObject(prefab, spawnPosition, Quaternion.identity);
        
        if (item != null)
        {
            AboutItem aboutItem = item.GetComponent<AboutItem>();
            aboutItem.player = player;
            aboutItem.SetCollectClip(collectClip);

            if (shouldFlyToPlayer)
            {
                aboutItem.SetAutoFlyToPlayer(true, scoreItemFlySpeed);
            }

            if (canSpawnPower)
            {
                lastPowerSpawnTime = currentTime;
            }
        }
        else
        {
            Debug.LogWarning($"无法从对象池获取物品: {prefab.name}");
            // 即使获取失败，也更新时间戳，避免连续请求
            if (canSpawnPower)
            {
                lastPowerSpawnTime = currentTime;
            }
        }
    }

    /// <summary>
    /// 生成指定类型的得分点
    /// </summary>
    private void SpawnScoreItemType(Vector3 position, ItemType itemType, int count, GameObject player)
    {
        int prefabIndex = (int)itemType;
        if (prefabIndex < 0 || prefabIndex >= ItemPrefabs.Count)
        {
            return;
        }

        GameObject prefab = ItemPrefabs[prefabIndex];
        if (prefab == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = position + GetRandomOffset();
            GameObject item = Global_ObjectPool.Instance.GetObject(prefab, spawnPosition, Quaternion.identity);
            AboutItem aboutItem = item.GetComponent<AboutItem>();
            aboutItem.player = player;
            aboutItem.SetCollectClip(collectClip);
            // 得分点自动飞向玩家
            aboutItem.SetAutoFlyToPlayer(true, scoreItemFlySpeed);
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
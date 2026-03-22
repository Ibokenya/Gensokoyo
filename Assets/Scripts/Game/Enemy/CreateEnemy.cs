using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    [Header("生成时间")]
    public int spawnTime;// 基于音乐时间
    
    [Header("生成数量")]
    public int spawnCount = 1;// 生成敌人的数量
    public float spawnInterval = 0.5f;// 生成间隔（秒）
    
    // 敌人类型
    public enum EnemyType
    {
        Normal,    // 普通敌人
        Ball,      // 球体敌人
        Elite      // 精英敌人
    }
    public EnemyType enemyType;
    
    // 移动模式
    public enum MoveMode
    {
        Path,       // 路径点移动
        Track,      // 追踪式移动
        Stationary, // 不移动
        Flicker,    // 闪烁模式（仅球体敌人）
        Gravity     // 重力移动
    }
    public MoveMode moveMode;
    
    // 二段移动模式
    public MoveMode secondaryMoveMode = MoveMode.Stationary;// 二段移动模式
    
    [Header("路径点列表")]
    public List<GameObject> movePoints;// 移动点列表
    
    [Header("闪烁参数")]
    public float flickerLifeTime = 2f;// 闪烁模式下的生存时间
}

public class CreateEnemy : MonoBehaviour
{
    [Header("敌人生成配置")]
    public List<EnemySpawnConfig> spawnConfigs;// 敌人生成配置列表
    
    [Header("敌人预制体")]
    public GameObject normalEnemyPrefab;// 普通敌人预制体
    public GameObject ballEnemyPrefab;   // 球敌人预制体
    public GameObject eliteEnemyPrefab;  // 精英敌人预制体
    
    [Header("对象池设置")]
    public int normalEnemyPoolSize = 10;// 普通敌人对象池大小
    public int ballEnemyPoolSize = 10;  // 球敌人对象池大小
    public int eliteEnemyPoolSize = 5;  // 精英敌人对象池大小
    
    [Header("音频管理")]
    private Global_AudioManager audioManager;// 音频管理单例
    
    [Header("生成状态")]
    private List<bool> hasSpawned;// 记录每个配置是否已生成
    private float currentMusicTime = 0f;// 当前音乐播放时间

    void OnEnable()
    {
        // 初始化对象池
        InitializeObjectPools();
        
        // 初始化生成状态
        hasSpawned = new List<bool>();
        for (int i = 0; i < spawnConfigs.Count; i++)
        {
            hasSpawned.Add(false);
        }
        
        // 获取音频管理单例
        audioManager = Global_AudioManager.Instance;
    }

    void Update()
    {
        // 获取当前音乐播放时间
        if (audioManager != null)
        {
            currentMusicTime = Global_AudioManager.Instance.CurrentBGMTime;
        }
        
        // 检查是否需要生成敌人
        CheckSpawnEnemies();
    }
    
    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializeObjectPools()
    {
        if (Global_ObjectPool.Instance == null)
        {
            Debug.LogError("Global_ObjectPool instance not found!");
            return;
        }
        
        // 初始化普通敌人对象池
        if (normalEnemyPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(normalEnemyPrefab, normalEnemyPoolSize);
        }
        
        // 初始化球敌人对象池
        if (ballEnemyPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(ballEnemyPrefab, ballEnemyPoolSize);
        }
        
        // 初始化精英敌人对象池
        if (eliteEnemyPrefab != null)
        {
            Global_ObjectPool.Instance.InitPool(eliteEnemyPrefab, eliteEnemyPoolSize);
        }
    }
    
    /// <summary>
    /// 检查是否需要生成敌人
    /// </summary>
    private void CheckSpawnEnemies()
    {
        for (int i = 0; i < spawnConfigs.Count; i++)
        {
            if (!hasSpawned[i] && currentMusicTime >= spawnConfigs[i].spawnTime)
            {
                SpawnEnemy(spawnConfigs[i]);
                hasSpawned[i] = true;
            }
        }
    }
    
    /// <summary>
    /// 生成敌人
    /// </summary>
    /// <param name="config">生成配置</param>
    private void SpawnEnemy(EnemySpawnConfig config)
    {
        // 开始生成多个敌人
        StartCoroutine(SpawnEnemiesCoroutine(config));
    }
    
    /// <summary>
    /// 生成多个敌人的协程
    /// </summary>
    /// <param name="config">生成配置</param>
    private IEnumerator SpawnEnemiesCoroutine(EnemySpawnConfig config)
    {
        for (int i = 0; i < config.spawnCount; i++)
        {
            GameObject enemyPrefab = null;
            
            // 根据敌人类型选择预制体
            switch (config.enemyType)
            {
                case EnemySpawnConfig.EnemyType.Normal:
                    enemyPrefab = normalEnemyPrefab;
                    break;
                case EnemySpawnConfig.EnemyType.Ball:
                    enemyPrefab = ballEnemyPrefab;
                    break;
                case EnemySpawnConfig.EnemyType.Elite:
                    enemyPrefab = eliteEnemyPrefab;
                    break;
            }
            
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab not assigned!");
                yield break;
            }
            
            // 从对象池获取敌人
            GameObject enemy = Global_ObjectPool.Instance.GetObject(enemyPrefab, transform.position, transform.rotation);
            if (enemy == null)
            {
                // 对象池不足，实例化新敌人
                enemy = Instantiate(enemyPrefab);
            }
            
            // 设置敌人位置（使用第一个路径点或当前位置）
            if (config.movePoints != null && config.movePoints.Count > 0)
            {
                enemy.transform.position = config.movePoints[0].transform.position;
            }
            else
            {
                enemy.transform.position = transform.position;
            }
            
            // 激活敌人
            enemy.SetActive(true);
            
            // 将敌人添加到Global_GameManager的EnemyList中
            if (Global_GameManager.Instance != null)
            {
                Global_GameManager.Instance.AddEnemy(enemy);
            }
            
            // 根据敌人类型和移动模式设置参数
            switch (config.enemyType)
            {
                case EnemySpawnConfig.EnemyType.Normal:
                    SetupNormalEnemy(enemy, config);
                    break;
                case EnemySpawnConfig.EnemyType.Ball:
                    SetupBallEnemy(enemy, config);
                    break;
                case EnemySpawnConfig.EnemyType.Elite:
                    SetupEliteEnemy(enemy, config);
                    break;
            }
            
            // 如果不是最后一个敌人，等待生成间隔
            if (i < config.spawnCount - 1 && config.spawnInterval > 0f)
            {
                yield return new WaitForSeconds(config.spawnInterval);
            }
        }
    }
    
    /// <summary>
    /// 设置普通敌人
    /// </summary>
    private void SetupNormalEnemy(GameObject enemy, EnemySpawnConfig config)
    {
        EnemyAnime enemyAnime = enemy.GetComponent<EnemyAnime>();
        if (enemyAnime != null)
        {
            // 设置移动模式
            switch (config.moveMode)
            {
                case EnemySpawnConfig.MoveMode.Path:
                    enemyAnime.moveMode = EnemyMoveMode.Path;
                    // 设置路径点
                    if (config.movePoints != null)
                    {
                        enemyAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case EnemySpawnConfig.MoveMode.Track:
                    enemyAnime.moveMode = EnemyMoveMode.Track;
                    break;
                case EnemySpawnConfig.MoveMode.Stationary:
                    enemyAnime.moveMode = EnemyMoveMode.Stationary;
                    break;
                case EnemySpawnConfig.MoveMode.Flicker:
                    enemyAnime.moveMode = EnemyMoveMode.Flicker;
                    // 设置路径点（闪烁模式需要一个路径点）
                    if (config.movePoints != null)
                    {
                        enemyAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case EnemySpawnConfig.MoveMode.Gravity:
                    enemyAnime.moveMode = EnemyMoveMode.Gravity;
                    break;
            }
            
            // 设置二段移动模式
            switch (config.secondaryMoveMode)
            {
                case EnemySpawnConfig.MoveMode.Path:
                    enemyAnime.secondaryMoveMode = EnemyMoveMode.Path;
                    break;
                case EnemySpawnConfig.MoveMode.Track:
                    enemyAnime.secondaryMoveMode = EnemyMoveMode.Track;
                    break;
                case EnemySpawnConfig.MoveMode.Stationary:
                    enemyAnime.secondaryMoveMode = EnemyMoveMode.Stationary;
                    break;
                case EnemySpawnConfig.MoveMode.Flicker:
                    enemyAnime.secondaryMoveMode = EnemyMoveMode.Flicker;
                    break;
                case EnemySpawnConfig.MoveMode.Gravity:
                    enemyAnime.secondaryMoveMode = EnemyMoveMode.Gravity;
                    break;
            }
        }
    }
    
    /// <summary>
    /// 设置球敌人
    /// </summary>
    private void SetupBallEnemy(GameObject enemy, EnemySpawnConfig config)
    {
        BallsAnime ballsAnime = enemy.GetComponent<BallsAnime>();
        if (ballsAnime != null)
        {
            // 设置移动模式
            switch (config.moveMode)
            {
                case EnemySpawnConfig.MoveMode.Path:
                    ballsAnime.moveMode = BallsMoveMode.Path;
                    // 设置路径点
                    if (config.movePoints != null)
                    {
                        ballsAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case EnemySpawnConfig.MoveMode.Track:
                    ballsAnime.moveMode = BallsMoveMode.Track;
                    break;
                case EnemySpawnConfig.MoveMode.Stationary:
                    ballsAnime.moveMode = BallsMoveMode.Stationary;
                    break;
                case EnemySpawnConfig.MoveMode.Flicker:
                    ballsAnime.moveMode = BallsMoveMode.Flicker;
                    // 设置路径点（闪烁模式需要一个路径点）
                    if (config.movePoints != null)
                    {
                        ballsAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case EnemySpawnConfig.MoveMode.Gravity:
                    ballsAnime.moveMode = BallsMoveMode.Gravity;
                    break;
            }
            
            // 设置二段移动模式
            switch (config.secondaryMoveMode)
            {
                case EnemySpawnConfig.MoveMode.Path:
                    ballsAnime.secondaryMoveMode = BallsMoveMode.Path;
                    break;
                case EnemySpawnConfig.MoveMode.Track:
                    ballsAnime.secondaryMoveMode = BallsMoveMode.Track;
                    break;
                case EnemySpawnConfig.MoveMode.Stationary:
                    ballsAnime.secondaryMoveMode = BallsMoveMode.Stationary;
                    break;
                case EnemySpawnConfig.MoveMode.Flicker:
                    ballsAnime.secondaryMoveMode = BallsMoveMode.Flicker;
                    break;
                case EnemySpawnConfig.MoveMode.Gravity:
                    ballsAnime.secondaryMoveMode = BallsMoveMode.Gravity;
                    break;
            }
        }
    }
    
    /// <summary>
    /// 设置精英敌人
    /// </summary>
    private void SetupEliteEnemy(GameObject enemy, EnemySpawnConfig config)
    {
        EliteAnime eliteAnime = enemy.GetComponent<EliteAnime>();
        if (eliteAnime != null)
        {
            // 设置移动模式
            switch (config.moveMode)
            {
                case EnemySpawnConfig.MoveMode.Path:
                    eliteAnime.moveMode = EliteMoveMode.Path;
                    // 设置路径点
                    if (config.movePoints != null)
                    {
                        eliteAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case EnemySpawnConfig.MoveMode.Stationary:
                    eliteAnime.moveMode = EliteMoveMode.Stationary;
                    break;
                case EnemySpawnConfig.MoveMode.Gravity:
                    eliteAnime.moveMode = EliteMoveMode.Gravity;
                    break;
            }
            
            // 设置二段移动模式
            switch (config.secondaryMoveMode)
            {
                case EnemySpawnConfig.MoveMode.Path:
                    eliteAnime.secondaryMoveMode = EliteMoveMode.Path;
                    break;
                case EnemySpawnConfig.MoveMode.Stationary:
                    eliteAnime.secondaryMoveMode = EliteMoveMode.Stationary;
                    break;
                case EnemySpawnConfig.MoveMode.Gravity:
                    eliteAnime.secondaryMoveMode = EliteMoveMode.Gravity;
                    break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShootMode;

// 移动模式枚举
public enum MoveMode
{
    Path,       // 路径点移动
    Track,      // 追踪式移动
    Stationary, // 不移动
    Flicker,    // 闪烁模式
    Gravity     // 重力移动
}

// 二段移动模式枚举
public enum SecondaryMode
{
    Track,      // 追踪式移动
    FlickerOut, // 闪烁淡出模式
    Stationary, // 不移动
    Gravity,    // 重力移动
    Disappear   // 直接回收自身
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
    public float currentMusicTime = 0f;// 当前音乐播放时间
    private int CurrentSpawn = 0;//第0波次
    public GameObject player;// 玩家对象引用

    void OnEnable()
    {
        // 初始化对象池
        InitializeObjectPools();
        
        // 获取音频管理单例
        audioManager = Global_AudioManager.Instance;
    }

    void Update()
    {
        // // 获取当前音乐播放时间
        // if (audioManager.CurrentBGMTime != 0)
        // {
        //     currentMusicTime = audioManager.CurrentBGMTime;
        // }
        currentMusicTime += Time.deltaTime;// 临时的
        
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
        while(CurrentSpawn < spawnConfigs.Count && 
        currentMusicTime >= spawnConfigs[CurrentSpawn].spawnTime)
        {
            if (currentMusicTime >= spawnConfigs[CurrentSpawn].spawnTime)
            {
                SpawnEnemy(spawnConfigs[CurrentSpawn]);
            }
            CurrentSpawn++;
        }
    }
    
    /// <summary>
    /// 生成敌人
    /// </summary>
    /// <param name="config">生成配置</param>
    private void SpawnEnemy(EnemySpawnConfig config)
    {
        Debug.Log($"生成敌人: {config.enemyType} {config.spawnCount} {config.moveMode} {config.secondaryMoveMode}");
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
            
            // 确保敌人对象处于未激活状态，以便在设置参数后再触发OnEnable
            enemy.SetActive(false);
            
            // 检查路径点数量
            CheckMovePointsCount(config);
            
            // 设置敌人位置（使用第一个路径点或当前位置）
            if (config.movePoints != null && config.movePoints.Count > 0)
            {
                enemy.transform.position = config.movePoints[0].transform.position;
            }
            else
            {
                enemy.transform.position = transform.position;
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
            
            // 激活敌人
            enemy.SetActive(true);
            
            // 将敌人添加到Global_GameManager的EnemyList中
            if (Global_GameManager.Instance != null)
            {
                Global_GameManager.Instance.AddEnemy(enemy);
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
            // 设置移动速度
            enemyAnime.MoveSpeed = config.moveSpeed;
            
            // 设置闪烁参数
            enemyAnime.FlickerLifeTime = config.flickerLifeTime;
            enemyAnime.fadeTime = config.fadeTime;
            
            // 设置重力参数
            enemyAnime.gravityScale = config.gravityScale;
            
            // 设置移动模式
            switch (config.moveMode)
            {
                case MoveMode.Path:
                    enemyAnime.moveMode = MoveMode.Path;
                    // 设置路径点
                    if (config.movePoints != null)
                    {
                        enemyAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case MoveMode.Track:
                    enemyAnime.moveMode = MoveMode.Track;
                    break;
                case MoveMode.Stationary:
                    enemyAnime.moveMode = MoveMode.Stationary;
                    break;
                case MoveMode.Flicker:
                    enemyAnime.moveMode = MoveMode.Flicker;
                    // 设置路径点（闪烁模式需要一个路径点）
                    if (config.movePoints != null)
                    {
                        enemyAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case MoveMode.Gravity:
                    enemyAnime.moveMode = MoveMode.Gravity;
                    // 初始化重力模式
                    enemyAnime.InitializeGravity();
                    break;
            }
            
            // 设置二段移动模式
            switch (config.secondaryMoveMode)
            {
                case SecondaryMode.Track:
                    enemyAnime.secondaryMoveMode = SecondaryMode.Track;
                    break;
                case SecondaryMode.FlickerOut:
                    enemyAnime.secondaryMoveMode = SecondaryMode.FlickerOut;
                    break;
                case SecondaryMode.Stationary:
                    enemyAnime.secondaryMoveMode = SecondaryMode.Stationary;
                    break;
                case SecondaryMode.Gravity:
                    enemyAnime.secondaryMoveMode = SecondaryMode.Gravity;
                    break;
                case SecondaryMode.Disappear:
                    enemyAnime.secondaryMoveMode = SecondaryMode.Disappear;
                    break;
            }
            
            // 设置射击配置
            EnemyShoot enemyShoot = enemy.GetComponent<EnemyShoot>();
            if (enemyShoot != null)
            {
                enemyShoot.SetShootConfig(config.shootConfig);
            }
            
            // 设置玩家对象
            if (player != null)
            {
                enemyAnime.SetPlayer(player);
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
            // 设置移动速度
            ballsAnime.MoveSpeed = config.moveSpeed;
            
            // 设置闪烁参数
            ballsAnime.FlickerLifeTime = config.flickerLifeTime;
            ballsAnime.fadeTime = config.fadeTime;
            
            // 设置重力参数
            ballsAnime.gravityScale = config.gravityScale;
            
            // 设置移动模式
            switch (config.moveMode)
            {
                case MoveMode.Path:
                    ballsAnime.moveMode = MoveMode.Path;
                    // 设置路径点
                    if (config.movePoints != null)
                    {
                        ballsAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case MoveMode.Track:
                    ballsAnime.moveMode = MoveMode.Track;
                    break;
                case MoveMode.Stationary:
                    ballsAnime.moveMode = MoveMode.Stationary;
                    break;
                case MoveMode.Flicker:
                    ballsAnime.moveMode = MoveMode.Flicker;
                    // 设置路径点（闪烁模式需要一个路径点）
                    if (config.movePoints != null)
                    {
                        ballsAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case MoveMode.Gravity:
                    ballsAnime.moveMode = MoveMode.Gravity;
                    // 初始化重力模式
                    ballsAnime.InitializeGravity();
                    break;
            }
            
            // 设置二段移动模式
            switch (config.secondaryMoveMode)
            {
                case SecondaryMode.Track:
                    ballsAnime.secondaryMoveMode = SecondaryMode.Track;
                    break;
                case SecondaryMode.FlickerOut:
                    ballsAnime.secondaryMoveMode = SecondaryMode.FlickerOut;
                    break;
                case SecondaryMode.Stationary:
                    ballsAnime.secondaryMoveMode = SecondaryMode.Stationary;
                    break;
                case SecondaryMode.Gravity:
                    ballsAnime.secondaryMoveMode = SecondaryMode.Gravity;
                    break;
                case SecondaryMode.Disappear:
                    ballsAnime.secondaryMoveMode = SecondaryMode.Disappear;
                    break;
            }
            
            // 设置射击配置
            EnemyShoot enemyShoot = enemy.GetComponent<EnemyShoot>();
            if (enemyShoot != null)
            {
                enemyShoot.SetShootConfig(config.shootConfig);
            }
            
            // 设置玩家对象
            if (player != null)
            {
                ballsAnime.SetPlayer(player);
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
            // 设置移动速度
            eliteAnime.MoveSpeed = config.moveSpeed;
            
            // 设置重力参数
            eliteAnime.gravityScale = config.gravityScale;
            
            // 设置渐入时间
            eliteAnime.fadeTime = config.fadeTime;
            
            // 设置闪烁参数
            eliteAnime.FlickerLifeTime = config.flickerLifeTime;
            
            // 设置移动模式
            switch (config.moveMode)
            {
                case MoveMode.Path:
                    eliteAnime.moveMode = MoveMode.Path;
                    // 设置路径点
                    if (config.movePoints != null)
                    {
                        eliteAnime.SetMovePoints(config.movePoints);
                    }
                    break;
                case MoveMode.Stationary:
                    eliteAnime.moveMode = MoveMode.Stationary;
                    break;
                case MoveMode.Gravity:
                    eliteAnime.moveMode = MoveMode.Gravity;
                    break;
                case MoveMode.Flicker:
                    eliteAnime.moveMode = MoveMode.Flicker;
                    break;
                default:
                    Debug.LogWarning($"大妖精不支持的移动模式: {config.moveMode}");
                    break;
            }
            
            // 设置二段移动模式
            switch (config.secondaryMoveMode)
            {
                case SecondaryMode.Stationary:
                    eliteAnime.secondaryMoveMode = SecondaryMode.Stationary;
                    break;
                case SecondaryMode.Gravity:
                    eliteAnime.secondaryMoveMode = SecondaryMode.Gravity;
                    break;
                case SecondaryMode.FlickerOut:
                    eliteAnime.secondaryMoveMode = SecondaryMode.FlickerOut;
                    break;
                case SecondaryMode.Disappear:
                    eliteAnime.secondaryMoveMode = SecondaryMode.Disappear;
                    break;
            }
            
            // 设置射击配置
            EnemyShoot enemyShoot = enemy.GetComponent<EnemyShoot>();
            if (enemyShoot != null)
            {
                enemyShoot.SetShootConfig(config.shootConfig);
            }
        }
    }
    
    /// <summary>
    /// 检查路径点数量是否正确
    /// </summary>
    /// <param name="config">生成配置</param>
    private void CheckMovePointsCount(EnemySpawnConfig config)
    {
        int movePointsCount = config.movePoints != null ? config.movePoints.Count : 0;
        
        switch (config.moveMode)
        {
            case MoveMode.Path:
                if (movePointsCount < 1)
                {
                    Debug.LogWarning($"路径点移动模式需要至少1个路径点，当前数量: {movePointsCount}");
                }
                break;
            case MoveMode.Track:
            case MoveMode.Flicker:
            case MoveMode.Stationary:
            case MoveMode.Gravity:
                if (movePointsCount != 1)
                {
                    Debug.LogWarning($"{config.moveMode}模式需要有且仅有1个路径点，当前数量: {movePointsCount}");
                }
                break;
        }
    }
}

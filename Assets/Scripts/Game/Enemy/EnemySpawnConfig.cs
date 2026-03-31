using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    [Header("生成时间")]
    public float spawnTime;// 基于音乐时间
    
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
    public MoveMode moveMode;
    
    // 二段移动模式
    public SecondaryMode secondaryMoveMode = SecondaryMode.Stationary;// 二段移动模式
    
    [Header("路径点列表")]
    public List<GameObject> movePoints;// 移动点列表
    
    [Header("移动参数")]
    public float moveSpeed = 5f;// 移动速度
    
    [Header("闪烁参数")]
    public float flickerLifeTime = 2f;// 闪烁模式下的生存时间
    public float fadeTime = 1f;// 淡入时间
    
    [Header("重力参数")]
    public float gravityScale = 1f;// 重力缩放
    
    [Header("基础属性")]
    public int hp = 100;// 敌人生命值
    
    [Header("射击配置列表")]
    public List<ShootMode> shootConfigs = new List<ShootMode>();
    
    [Header("掉落物配置")]
    public List<ItemDropConfig> itemDrops = new List<ItemDropConfig>();
}

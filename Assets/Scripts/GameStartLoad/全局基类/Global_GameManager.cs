using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    Easy,Normal,Hard,Lunatic,Extra
}
public enum Character
{
    Reimu,Marisa
}
public enum State
{
    Menu,CharacterChoose,ModeChoose,Gaming,Pause,Loading,Over,
    Replay,Option,MusicRoom,Manual,Reincarnation,NoDead,TimeStop,
    SpellCard,Dialog,Frozen
}

/// <summary>
/// 游戏重置配置类
/// </summary>
[System.Serializable]
public class GameResetConfig
{
    public int ResetBomb;
    public int Hp;
    public int HpPiece;
    public int BombCount;
    public int BombPiece;
    public int Power;
    public int Grade;
    public int Graze;
}

/// <summary>
/// 全局游戏管理单例
/// 存储机体，难度，残机，得点等数据
/// 以及状态机，还有生成敌人相关的波次管理
/// </summary>
public class Global_GameManager : Singleton<Global_GameManager>
{
    public GameMode gameMode;    // 游戏难度
    public Character character;  // 机体
    public int ResetBomb;        // 每命回复B数（随难度）
    public int Hp;               // 残机数
    public int HpPiece;          // 残机碎片数
    public int BombCount;        // 残B数
    public int BombPiece;        // 残B碎片数
    public int Power = 100;        // 灵力（火力等级）
    public int Grade;            // 得点
    public int Graze;            // 擦弹数
    public int Score;            // 得分数
    public int HighestScore;     // 最高得分数
    public int SceneLevel;       // 关卡等级
    public float SpeedScale = 1f;//速度缩放比例（冰冻系统相关）

    public State state;      // 状态机
    private State previousState; // 用于记录设置无敌前的状态

    [Header("敌人管理")]
    public List<GameObject> EnemyList = new(); // 存储当前场景中的敌人
    [Header("成长音效")]
    public AudioClip PowerUpClip;//灵力增加音效
    public AudioClip HpUpClip;//残机数增加音效
    public AudioClip BombUpClip;//残B数增加音效

    int pastPower = 0;//上一次灵力值，用于判断是否需要播放音效
    public bool isCheheat = false;//是否开启作弊模式

/// <summary>
/// 事件系统
/// </summary>
#region 事件系统
    public event Action<int> OnScoreChanged;        // 得分数改变事件
    public event Action<int> OnPowerChanged;        // 灵力值改变事件
    public event Action<int> OnGradeChanged;        // 得点改变事件
    public event Action<int,int> OnLeftLifeChanged; // 残机改变事件
    public event Action<int,int> OnBombChanged;     // 符卡碎片改变事件
    public event Action<int> OnGrazeChanged;        // 擦弹数改变事件
    public event Action<State> OnReincarnation;     // 重生事件
#endregion

    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效
    }

    // Start is called before the first frame update
    void Start()
    {
        gameMode = GameMode.Easy;     // 游戏难度（根据难度调整残机与灵力）
        character = Character.Reimu;  // 机体
        ResetBomb = 2;                // 每命回复B数（随难度）
        Hp = 2;                       // 残机数
        HpPiece = 0;                  // 残机碎片数
        BombCount = 2;                // 残B数
        BombPiece = 0;                // 残B碎片数
        Power = Mathf.Clamp(100,100,400); // 灵力值
        Grade = 0;                    // 得点
        Graze = 0;                    // 擦弹数
        Score = 0;                    // 得分数
        HighestScore = PlayerPrefs.GetInt("HighestScore", 0); // 最高得分数
        SceneLevel = 1;               // 关卡等级（第几面）
        SpeedScale = 1f;              //速度缩放比例（冰冻系统相关）
        state = State.Gaming;         // 状态机（初始为Loading）
        isCheheat = false;            //是否开启作弊模式
    }

    public void AddScore(int score = 1)
    {
        Score += score;
        OnScoreChanged?.Invoke(Score);
    }

    public void AddPower(int count=1)
    {
        pastPower = Power/100;
        if(Power<400)
        {
            Power += count;
            if(Power/100 > pastPower && PowerUpClip != null)
            {
                Global_AudioManager.Instance.PlaySFX(PowerUpClip);
            }
            Power = Mathf.Clamp(Power,0,400);
            OnPowerChanged?.Invoke(Power);
        }
    }

    public void SubPower(int count=1)
    {
        if(Power>100)
        {
            Power -= count;
            Power = Mathf.Clamp(Power,100,400);
            OnPowerChanged?.Invoke(Power);
        }
    }

    public void AddGrade(int grade = 1)
    {
        Grade += grade;
        OnGradeChanged?.Invoke(Grade);
    }

    public void AddLeftLife(int life = 0 , int piece = 0)
    {
        HpPiece += piece;
        if(HpPiece==3)
        {
            HpPiece = 0;
            life++;
            if(HpUpClip != null)
            {
                Global_AudioManager.Instance.PlaySFX(HpUpClip);
            }
        }
        Hp += life;
        if(Hp>7)
        {
            Hp = 7;
        }
        OnLeftLifeChanged?.Invoke(Hp,HpPiece);
    }

    public void SubLeftLife()
    {
        if(Hp>0)
        {
            Hp--;
            state = State.Reincarnation;
            SubPower(80);
            OnReincarnation?.Invoke(state);
            OnLeftLifeChanged?.Invoke(Hp,HpPiece);
            if(BombCount < ResetBomb)
            {
                SetBomb(ResetBomb,BombPiece);
            }
        }
        else
        {
            AddLeftLife(7,0);
            //state = State.Over;   // 游戏结束，满目疮痍（用广播事件）
        }
    }

    public void AddBomb(int bomb = 0 , int piece = 0)
    {
        BombPiece += piece;
        if(BombPiece==3)
        {
            BombPiece = 0;
            bomb++;
            if(BombUpClip != null)
            {
                Global_AudioManager.Instance.PlaySFX(BombUpClip);
            }
        }
        BombCount += bomb;
        if(BombCount>7)
        {
            BombCount = 7;
        }
        OnBombChanged?.Invoke(BombCount,BombPiece);
    }

    public void SubBomb(int bomb = 1)
    {
        if(BombCount>0)
        {
            BombCount -= bomb;
            OnBombChanged?.Invoke(BombCount,BombPiece);
        }
    }

    public void SetBomb(int bomb, int cardPiece)
    {
        BombCount = bomb;
        BombPiece = cardPiece;
        OnBombChanged?.Invoke(BombCount,BombPiece);
    }

    public void AddGraze(int graze = 1)
    {
        Graze += graze;
        OnGrazeChanged?.Invoke(Graze);
    }

    public void ResetGraze()
    {
        Graze = 0;
        OnGrazeChanged?.Invoke(Graze);
    }

    public void SetSpeedScale(float scale)
    {
        SpeedScale = scale;
    }

    public float GetSpeedScale()
    {
        return SpeedScale;
    }

    /// <summary>
    /// 重置游戏数据
    /// 从JSON配置文件读取初始数据，保留当前机体和难度
    /// </summary>
    public void ResetGameDate()
    {  
        // 从JSON配置文件读取初始数据
        string jsonFilePath = System.IO.Path.Combine(Application.dataPath, "Resources/Touho/JSON", "Game1_ResetConfig.json");
        if (System.IO.File.Exists(jsonFilePath))
        {
            try
            {
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                GameResetConfig config = JsonUtility.FromJson<GameResetConfig>(jsonContent);
                if (config != null)
                {
                    // 设置初始数据
                    ResetBomb = config.ResetBomb;
                    Hp = config.Hp;
                    HpPiece = config.HpPiece;
                    BombCount = config.BombCount;
                    BombPiece = config.BombPiece;
                    Power = Mathf.Clamp(config.Power, 0, 400);
                    Grade = config.Grade;
                    Graze = config.Graze;
                    Score = 0; // 每次重开游戏得分重置为0
                    SceneLevel = 1; // 关卡等级重置为1（第一关）
                    state = State.Gaming; // 状态重置为游戏中
                }
            }
            catch (Exception e)
            {
                Debug.LogError("读取游戏配置文件失败: " + e.Message);
                // 如果读取失败，使用默认值
                SetDefaultValues();
            }
        }
        else
        {
            Debug.LogWarning("游戏配置文件不存在，使用默认值");
            SetDefaultValues();
        }
        
        // 读取最高得分
        HighestScore = PlayerPrefs.GetInt("HighestScore", 0);
        
        // 触发相关事件
        OnScoreChanged?.Invoke(Score);
        OnPowerChanged?.Invoke(Power);
        OnGradeChanged?.Invoke(Grade);
        OnLeftLifeChanged?.Invoke(Hp, HpPiece);
        OnBombChanged?.Invoke(BombCount, BombPiece);
        OnGrazeChanged?.Invoke(Graze);
    }
    
    /// <summary>
    /// 设置默认游戏数据
    /// </summary>
    private void SetDefaultValues()
    {
        ResetBomb = 2;
        Hp = 2;
        HpPiece = 0;
        BombCount = 2;
        BombPiece = 0;
        Power = 100;
        Grade = 0;
        Graze = 0;
        Score = 0;
        SceneLevel = 1;
        SpeedScale = 1f;
        state = State.Gaming;
    }

    /// <summary>
    /// 添加敌人到敌人列表
    /// </summary>
    /// <param name="enemy">敌人对象</param>
    public void AddEnemy(GameObject enemy)
    {
        if (enemy != null && !EnemyList.Contains(enemy))
        {
            EnemyList.Add(enemy);
        }
    }

    /// <summary>
    /// 从敌人列表中移除敌人
    /// </summary>
    /// <param name="enemy">敌人对象</param>
    public void RemoveEnemy(GameObject enemy)
    {
        if (enemy != null && EnemyList.Contains(enemy))
        {
            EnemyList.Remove(enemy);
        }
    }

    /// <summary>
    /// 回收所有敌人
    /// </summary>
    public void RecycleAllEnemies()
    {
        foreach (GameObject enemy in EnemyList)
        {
            if (enemy != null)
            {
                Global_ObjectPool.Instance.Recycle(enemy);
            }
        }
        EnemyList.Clear();
    }
    
    /// <summary>
    /// 设置无敌状态
    /// </summary>
    /// <param name="time">无敌持续时间（秒）</param>
    public void SetNoDead(float time,State thestate)
    {
        // 记录当前状态
        previousState = thestate;
        // 设置为无敌状态
        state = State.NoDead;
        // 启动协程，在指定时间后恢复之前的状态
        StartCoroutine(RecoverStateAfterTime(time));
    }
    
    /// <summary>
    /// 在指定时间后恢复之前的状态
    /// </summary>
    /// <param name="time">等待时间（秒）</param>
    private IEnumerator RecoverStateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        // 恢复之前的状态
        state = previousState;
    }
} 

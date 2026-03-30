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
    Menu,CharacterChoose,ModeChoose,Gaming,Stop,Loading,Over,Replay,Option,MusicRoom,Manual,Reincarnation
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
    public int Power = 0;        // 灵力（火力等级）
    public int Grade;            // 得点
    public int Graze;            // 擦弹数
    public int Score;            // 得分数
    public int HighestScore;     // 最高得分数
    public int SceneLevel;       // 关卡等级

    public State state;      // 状态机

    [Header("敌人管理")]
    public List<GameObject> EnemyList = new(); // 存储当前场景中的敌人

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
        Power = Mathf.Clamp(0,0,400); // 灵力值
        Grade = 0;                    // 得点
        Graze = 0;                    // 擦弹数
        Score = 0;                    // 得分数
        HighestScore = PlayerPrefs.GetInt("HighestScore", 0); // 最高得分数
        SceneLevel = 1;               // 关卡等级（第几面）
        state = State.Gaming;         // 状态机（初始为Loading）
    }

    public void AddScore(int score = 1)
    {
        Score += score;
        OnScoreChanged?.Invoke(Score);
    }

    public void AddPower(int count=1)
    {
        if(Power<400)
        {
            Power += count;
            Power = Mathf.Clamp(Power,0,400);
            OnPowerChanged?.Invoke(Power);
        }
    }

    public void SubPower(int count=1)
    {
        if(Power>0)
        {
            Power -= count;
            Power = Mathf.Clamp(Power,0,400);
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
            Invoke(nameof(Reincarnation),3f);
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

    private void Reincarnation()
    {
        state = State.Gaming;
    }
} 

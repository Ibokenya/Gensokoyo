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
    Menu,CharacterChoose,ModeChoose,Gaming,Stop,Loading,Over,Replay,Option,MusicRoom,Manual
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
    public int Hp;               // 残机数
    public int BombCount;        // 残B数
    public int Power = 0;        // 灵力（火力等级）
    public int Grade;            // 得点
    public int Graze;            // 擦弹
    public int Score;            // 得分数
    public int HighestScore;     // 最高得分数
    public int SceneLevel;       // 关卡等级

    public State state;          // 状态机

/// <summary>
/// 事件系统
/// </summary>
    public event Action<int> OnPowerChanged;

    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效
        Power = Mathf.Clamp(0,0,400);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameMode = GameMode.Easy;
        character = Character.Reimu;
        state = State.Loading;
        HighestScore = PlayerPrefs.GetInt("HighestScore", 0);
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

}

using UnityEngine;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    private int HighestScore => Global_GameManager.Instance.HighestScore;// 最高分
    private int CurrentScore => Global_GameManager.Instance.Score;// 当前分
    [Header("得分")]
    [SerializeField]
    private TextMeshProUGUI HighestScoreText;
    [SerializeField]
    private TextMeshProUGUI ScoreText;
    private int LeftLife => Global_GameManager.Instance.Hp;// 剩余生命值
    private int LifePiece => Global_GameManager.Instance.HpPiece;// 生命值碎片
    private int SpeelCard => Global_GameManager.Instance.BombCount;// 符卡数量
    private int CardPiece => Global_GameManager.Instance.BombPiece;// 符卡碎片数量
    private int Power => Global_GameManager.Instance.Power;// 灵力值
    [Header("灵力值-(当前百位，当前十个位)")]
    [SerializeField]
    private TextMeshProUGUI PowerText_Hundred;
    [SerializeField]
    private TextMeshProUGUI PowerText_Ten;
    private int MaxGrade => Global_GameManager.Instance.Grade;// 最大得点
    [Header("最大得点")]
    [SerializeField]
    private TextMeshProUGUI MaxGradeText;
    private int Graze => Global_GameManager.Instance.Graze;// 擦弹数
    [Header("擦弹数")]
    [SerializeField]
    private TextMeshProUGUI GrazeText;

    [Header("道具线")]
    public GameObject BorderLine;
    [Header("UI引用")]
    public GameObject FinalUI;
    public GameObject GameOverUI;

    public LeftLife leftLife;
    public SpeelCard speelCard;

    [Header("最终得分UI相关全部元素")]
    public bool isCard1Get; // 是否获取了符卡1
    public bool isCard2Get; // 是否获取了符卡2
    public bool isFinalCardGet; // 是否获取了最终符卡
    public bool isContinueGame; // 是否续关过
    public int ExScore; // 额外得分
    private int MissCount; // 受击次数
    
    [Header("游戏计时器")]
    public float gameTime = 0f; // 游戏时长（秒）
    private bool isTimerRunning = true; // 计时器是否运行
    
    /// <summary>
    /// 获取游戏时长字符串（格式：Xm'Ys'）
    /// </summary>
    public string GetGameTimeString()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        return $"{minutes}m'{seconds}s'";
    }
    
    /// <summary>
    /// 停止计时器
    /// </summary>
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    void Update()
    {
        // 更新游戏计时器
        if (isTimerRunning)
        {
            gameTime += Time.deltaTime;
        }
    }
    
    void OnEnable()
    {
#region 订阅广播事件
        Global_GameManager.Instance.OnScoreChanged += SetScoreText; 
        Global_GameManager.Instance.OnPowerChanged += SetPowerText;
        Global_GameManager.Instance.OnGradeChanged += SetGradeText;
        Global_GameManager.Instance.OnGrazeChanged += SetGrazeText;
        Global_GameManager.Instance.OnLeftLifeChanged += SetLeftLife;
        Global_GameManager.Instance.OnBombChanged += SetBomb;
        Global_GameManager.Instance.OnReincarnation += AddMissCount;
        Global_GameManager.Instance.OnOver += ShowGameOverUI;
#endregion
        MissCount = 0;
        ExScore = 0;
        isCard1Get = false;
        isCard2Get = false;
        isFinalCardGet = false;
        isContinueGame = false;
        SetHighestScoreText(HighestScore);
        SetScoreText(CurrentScore);
        SetPowerText(Power);
        SetGradeText(MaxGrade);
        SetGrazeText(Graze);
        SetLeftLife(LeftLife, LifePiece);
        SetBomb(SpeelCard, CardPiece);
        if (BorderLine != null)
        {
            BorderLine.SetActive(true);
        }
        Invoke(nameof(HideBorderLine), 2f);
    }

    void OnDisable()
    {
        Global_GameManager.Instance.OnScoreChanged -= SetScoreText; 
        Global_GameManager.Instance.OnPowerChanged -= SetPowerText;
        Global_GameManager.Instance.OnGradeChanged -= SetGradeText;
        Global_GameManager.Instance.OnGrazeChanged -= SetGrazeText;
        Global_GameManager.Instance.OnLeftLifeChanged -= SetLeftLife;
        Global_GameManager.Instance.OnBombChanged -= SetBomb;
        Global_GameManager.Instance.OnReincarnation -= AddMissCount;
        Global_GameManager.Instance.OnOver -= ShowGameOverUI;
        CancelInvoke();
    }

    private void SetPowerText(int power)
    {
        int hundred = power / 100;
        int ten = power % 100;
        PowerText_Hundred.text = hundred.ToString();
        PowerText_Ten.text = "." + ten.ToString("00");
    }

    private void SetHighestScoreText(int highestScore)
    {
        HighestScoreText.text = highestScore.ToString();
    }

    private void SetScoreText(int score)
    {
        ScoreText.text = score.ToString();
        if (score > HighestScore)
        {
            Global_GameManager.Instance.HighestScore = score;
            SetHighestScoreText(HighestScore);
        }
    }

    private void SetGrazeText(int graze)
    {
        GrazeText.text = graze.ToString();
    }

    private void SetGradeText(int grade)
    {
        MaxGradeText.text = grade.ToString();
    }
    private void SetLeftLife(int life, int lifePiece)
    {
        leftLife.SetLife(life, lifePiece);
    }
    private void SetBomb(int bomb, int cardPiece)
    {
        speelCard.SetBomb(bomb, cardPiece);
    }

    private void HideBorderLine()
    {
        if (BorderLine != null)
        {
            BorderLine.SetActive(false);
        }
    }

    public void ShowFinalUI()
    {
        FinalUI.SetActive(true);
    }

    public void AddMissCount(State state)
    {
        MissCount++;
    }

    public int GetMissCount()
    {
        return MissCount;
    }

    public void ShowGameOverUI(State state)
    {
        GameOverUI.SetActive(true);
    }

    public void AddExScore(int score)
    {
        ExScore+=score;
    }
}

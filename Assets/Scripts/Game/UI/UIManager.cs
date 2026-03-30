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

    public LeftLife leftLife;
    public SpeelCard speelCard;

    void OnEnable()
    {
#region 订阅广播事件
        Global_GameManager.Instance.OnScoreChanged += SetScoreText; 
        Global_GameManager.Instance.OnPowerChanged += SetPowerText;
        Global_GameManager.Instance.OnGradeChanged += SetGradeText;
        Global_GameManager.Instance.OnGrazeChanged += SetGrazeText;
        Global_GameManager.Instance.OnLeftLifeChanged += SetLeftLife;
        Global_GameManager.Instance.OnBombChanged += SetBomb;
#endregion

        HighestScoreText.text = HighestScore.ToString();
        SetScoreText(CurrentScore);
        SetPowerText(Power);
        SetGradeText(MaxGrade);
        SetGrazeText(Graze);
        SetLeftLife(LeftLife, LifePiece);
        SetBomb(SpeelCard, CardPiece);
        BorderLine.SetActive(true);
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
        CancelInvoke();
    }

    private void SetPowerText(int power)
    {
        int hundred = power / 100;
        int ten = power % 100;
        PowerText_Hundred.text = hundred.ToString();
        PowerText_Ten.text = "." + ten.ToString("00");
    }

    private void SetScoreText(int score)
    {
        ScoreText.text = score.ToString();
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
        BorderLine.SetActive(false);
    }
}

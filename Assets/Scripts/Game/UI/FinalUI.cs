using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class FinalUI : MonoBehaviour
{
    [Header("脚本引用")]
    public UIManager uiManager; // UIManager脚本引用
    [Header("CSV文件路径")]
    public string csvFilePath = "Touho/对话/FinalUI_1"; // Resources路径
    [Header("最终得分UI相关全部元素")]
    public GameObject Chrino;               // 琪露诺的大头照
    public GameObject FeatedBoss;           // 【已击败敌人】
    public GameObject ChrinoName;           // 琪露诺的姓名
    public GameObject Msg;                  // 【情报】
    public TextMeshProUGUI MsgDesc;         // 情报描述
    public GameObject FinalGradeIs;         // 【最终得分】
    public TextMeshProUGUI FinalGradeCalu;  // 得分计算过程
    public TextMeshProUGUI FinalGrade;      // 最终得分
    public GameObject Graze;                // 【擦弹数】
    public TextMeshProUGUI GrazeText;       // 擦弹数
    public GameObject Miss;                 // 【中弹数】
    public TextMeshProUGUI MissText;        // 中弹数
    public GameObject SpentTime;            // 【耗时】
    public TextMeshProUGUI SpentTimeText;   // 耗时
    public GameObject RemainRoad;           // 【剩余旅程】
    public TextMeshProUGUI RemainRoadText;  // 剩余旅程
    public TextMeshProUGUI LaterText;       // 后续预告
    public GameObject Shadow;               // 【符卡面板幕布】
    public GameObject ShadowText;           // 【已收取符卡】
    public GameObject Card1;                // 【符卡1】
    public TextMeshProUGUI Card1GetStatus;  // 符卡1获取状态
    public TextMeshProUGUI Card1Text;       // 符卡1描述
    public GameObject Card2;                // 【符卡2】
    public TextMeshProUGUI Card2GetStatus;  // 符卡2获取状态
    public TextMeshProUGUI Card2Text;       // 符卡2描述
    public GameObject Card3;                // 【符卡3】
    public TextMeshProUGUI Card3GetStatus;  // 符卡3获取状态
    public TextMeshProUGUI Card3Text;       // 符卡3描述
    [Header("琪露诺的可爱动画")]
    public Animator ChrinoAnim;             // 琪露诺的动画组件
    
    // Chrino动画控制变量
    private bool isStand = true;            // 是否处于站立状态
    private const float ANIMATION_CHECK_INTERVAL = 1f; // 每秒检测一次
    private const float ANIMATION_COOLDOWN_TIME = 2.5f; // 冷却时间（秒），不受时间缩放影响
    private float animationTimer = 0f;
    private float cooldownTimer = 0f;       // 冷却计时器（使用真实时间）
    
    // 动画触发概率 (0-1)
    [Header("动画触发概率")]
    [Range(0f, 1f)]
    public float rotateTriggerChance = 0.4f;    // 旋转触发概率
    [Range(0f, 1f)]
    public float standUpTriggerChance = 0.6f;   // 起身触发概率
    
    // 逐字输出相关变量
    private TextMeshProUGUI currentPrintTarget;
    private string currentPrintText;
    private int currentCharIndex;
    private int printFrameCounter;
    private int printInterval;
    private bool isPrinting;
    
    // 顺序显示UI相关变量
    private List<UIElement> currentUIList;
    private int currentUIIndex;
    private int uiFrameCounter;
    private int uiInterval;
    private bool isShowingUI;
    
    // 协程引用
    private Coroutine printCoroutine;
    private Coroutine showUICoroutine;
    private Coroutine calculateScoreCoroutine;
    
    // 得分计算常量
    private const int GRAZE_SCORE_CONSTANT = 325; // 擦弹得分常数
    private const int BONUS_SCORE_CONSTANT = 500; // Bonus得分常量
    
    // 得分计算中间变量
    private int finalScoreResult;
    private bool isCalculatingScore;
    
    // UI显示间隔（帧）
    private const int UI_INTERVAL = 30;
    
    // CSV文本数据存储
    private Dictionary<int, string> csvTextData = new Dictionary<int, string>();
    
    // 文本ID常量
    private const int TEXT_ID_MSG_DESC = 1;      // 情报描述
    private const int TEXT_ID_LATER_TEXT = 2;    // 后续预告
    private const int TEXT_ID_CARD1_DESC = 3;    // 符卡1描述
    private const int TEXT_ID_CARD2_DESC = 4;    // 符卡2描述
    private const int TEXT_ID_CARD3_DESC = 5;    // 符卡3描述
    private const int TEXT_ID_CARD_FAILED = 6;   // 符卡未收取描述
    
    void OnEnable()
    {
        // 获取UIManager引用
        uiManager = FindObjectOfType<UIManager>();
        
        // 停止UIManager中的计时器
        if (uiManager != null)
        {
            uiManager.StopTimer();
        }
        
        // 解析CSV文件
        ParseCSVFile();
        
        // 初始化所有UI元素为隐藏状态
        InitializeUIElements();
        
        // 开始按序显示UI元素
        StartCoroutine(ShowFinalUIElements());
    }
    
    /// <summary>
    /// 解析CSV文件
    /// </summary>
    private void ParseCSVFile()
    {
        csvTextData.Clear();
        
        TextAsset csvFile = Resources.Load<TextAsset>(csvFilePath);
        if (csvFile == null)
        {
            Debug.LogError("无法找到CSV文件: " + csvFilePath);
            return;
        }
        
        StringReader reader = new StringReader(csvFile.text);
        string line;
        bool isFirstLine = true; // 跳过表头
        
        while ((line = reader.ReadLine()) != null)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
                continue;
            }
            
            string[] parts = line.Split(',');
            if (parts.Length >= 2)
            {
                int id;
                if (int.TryParse(parts[0].Trim(), out id))
                {
                    string text = parts[1].Trim();
                    csvTextData[id] = text;
                    Debug.Log("加载文本 ID:" + id + " Text:" + text);
                }
            }
        }
        
        reader.Close();
    }
    
    /// <summary>
    /// 根据ID获取文本内容
    /// </summary>
    private string GetTextById(int id)
    {
        if (csvTextData.ContainsKey(id))
        {
            return csvTextData[id];
        }
        Debug.LogWarning("未找到文本 ID:" + id);
        return "未找到文本";
    }
    
    /// <summary>
    /// 初始化所有UI元素为隐藏状态
    /// </summary>
    private void InitializeUIElements()
    {

        // 清空所有文本
        MsgDesc?.SetText("");
        FinalGradeCalu?.SetText("");
        FinalGrade?.SetText("");
        GrazeText?.SetText("");
        MissText?.SetText("");
        SpentTimeText?.SetText("");
        RemainRoadText?.SetText("");
        LaterText?.SetText("");
        Card1GetStatus?.SetText("");
        Card1Text?.SetText("");
        Card2GetStatus?.SetText("");
        Card2Text?.SetText("");
        Card3GetStatus?.SetText("");
        Card3Text?.SetText("");

        // 隐藏所有GameObject类型的UI元素
        Chrino?.SetActive(false);
        FeatedBoss?.SetActive(false);
        ChrinoName?.SetActive(false);
        Msg?.SetActive(false);
        FinalGradeIs?.SetActive(false);
        Graze?.SetActive(false);
        Miss?.SetActive(false);
        SpentTime?.SetActive(false);
        RemainRoad?.SetActive(false);
        Shadow?.SetActive(false);
        ShadowText?.SetActive(false);
        Card1?.SetActive(false);
        Card2?.SetActive(false);
        Card3?.SetActive(false);
    }
    
    /// <summary>
    /// 按序显示所有UI元素
    /// </summary>
    private IEnumerator ShowFinalUIElements()
    {
        // 1. 激活琪露诺大头照
        yield return ActivateElementWithDelay(Chrino);
        
        // 2. 激活【已击败敌人】
        yield return ActivateElementWithDelay(FeatedBoss);
        
        // 3. 激活琪露诺姓名
        yield return ActivateElementWithDelay(ChrinoName);
        
        // 4. 激活【情报】
        yield return ActivateElementWithDelay(Msg);
        
        // 5. 逐字输出情报描述 (从CSV读取)
        yield return PrintTextWithDelay(MsgDesc, GetTextById(TEXT_ID_MSG_DESC));
        
        // 6. 激活【最终得分】
        yield return ActivateElementWithDelay(FinalGradeIs);
        
        // 7. 计算得分（逐个词条输出）
        yield return CalculateScoreWithDelay();
        
        // 8. 设置最终得分
        if (FinalGrade != null)
        {
            FinalGrade.text = finalScoreResult.ToString();
        }
        yield return WaitForFrames(UI_INTERVAL);
        
        // 9. 激活【擦弹数】
        yield return ActivateElementWithDelay(Graze);
        
        // 10. 设置擦弹数文本
        if (GrazeText != null && uiManager != null)
        {
            GrazeText.text = Global_GameManager.Instance.Graze.ToString();
        }
        yield return WaitForFrames(UI_INTERVAL);
        
        // 11. 激活【中弹数】
        yield return ActivateElementWithDelay(Miss);
        
        // 12. 设置中弹数文本（需要在UIManager中添加中弹数变量）
        if (MissText != null)
        {
            int missCount = uiManager.GetMissCount();
            MissText.text = missCount.ToString();
        }
        yield return WaitForFrames(UI_INTERVAL);
        
        // 13. 激活【耗时】
        yield return ActivateElementWithDelay(SpentTime);
        
        // 14. 设置耗时文本
        if (SpentTimeText != null && uiManager != null)
        {
            SpentTimeText.text = uiManager.GetGameTimeString();
        }
        yield return WaitForFrames(UI_INTERVAL);
        
        // 15. 激活【剩余旅程】
        yield return ActivateElementWithDelay(RemainRoad);
        
        // 16. 设置剩余旅程文本为"UnKnown"
        if (RemainRoadText != null)
        {
            RemainRoadText.text = "UnKnown";
        }
        yield return WaitForFrames(UI_INTERVAL);
        
        // 17. 逐字输出后续预告 (从CSV读取)
        yield return PrintTextWithDelay(LaterText, GetTextById(TEXT_ID_LATER_TEXT));
        
        // 18. 激活【符卡面板幕布】
        yield return ActivateElementWithDelay(Shadow);
        
        // 19. 激活【已收取符卡】
        yield return ActivateElementWithDelay(ShadowText);
        
        // 20. 显示符卡1
        yield return ShowCard(Card1, Card1GetStatus, Card1Text, uiManager?.isCard1Get ?? false, 
            GetTextById(TEXT_ID_CARD1_DESC), GetTextById(TEXT_ID_CARD_FAILED));
        
        // 21. 显示符卡2
        yield return ShowCard(Card2, Card2GetStatus, Card2Text, uiManager?.isCard2Get ?? false, 
            GetTextById(TEXT_ID_CARD2_DESC), GetTextById(TEXT_ID_CARD_FAILED));
        
        // 22. 显示符卡3（最终符卡）
        yield return ShowCard(Card3, Card3GetStatus, Card3Text, uiManager?.isFinalCardGet ?? false, 
            GetTextById(TEXT_ID_CARD3_DESC), GetTextById(TEXT_ID_CARD_FAILED));
        
        // 23. 将计算出的得分加到总分上
        AddFinalScoreToTotal();
        
        // 24. 等待5秒后跳转到Game2场景
        yield return new WaitForSecondsRealtime(5f);
        
        // 25. 重置GameManager数据并跳转场景
        TransitionToGame2();
    }
    
    /// <summary>
    /// 将最终得分加到总分上
    /// </summary>
    private void AddFinalScoreToTotal()
    {
        if (Global_GameManager.Instance != null && uiManager != null)
        {
            // 获取当前总分
            int currentScore = Global_GameManager.Instance.Score;
            
            // 将最终得分加到总分上
            int newTotalScore = currentScore + finalScoreResult;
            Global_GameManager.Instance.AddScore(finalScoreResult);
            
            Debug.Log($"得分累加完成 - 原分数: {currentScore}, 本关得分: {finalScoreResult}, 新总分: {newTotalScore}");
        }
    }
    
    /// <summary>
    /// 重置数据并跳转到Game2场景
    /// </summary>
    private void TransitionToGame2()
    {
        // 重置GameManager数据
        if (Global_GameManager.Instance != null)
        {
            Global_GameManager.Instance.ResetFor_Game2();
        }
        
        // 恢复正常时间流速
        Time.timeScale = 1f;
        
        // 跳转到Game2场景（不保留当前场景）
        if (Global_SceneManager.Instance != null)
        {
            Global_SceneManager.Instance.IntoNextScene("Game2", false);
        }
        else
        {
            Debug.LogError("Global_SceneManager.Instance 为 null，无法跳转场景");
        }
    }
    
    /// <summary>
    /// 激活元素并等待
    /// </summary>
    private IEnumerator ActivateElementWithDelay(GameObject element)
    {
        element?.SetActive(true);
        yield return WaitForFrames(UI_INTERVAL);
    }
    
    /// <summary>
    /// 逐字输出文本并等待完成
    /// </summary>
    private IEnumerator PrintTextWithDelay(TextMeshProUGUI target, string text)
    {
        if (target == null)
        {
            yield break;
        }
        
        target.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            target.text += text[i];
            yield return new WaitForEndOfFrame();
        }
        
        yield return WaitForFrames(UI_INTERVAL);
    }
    
    /// <summary>
    /// 计算得分并等待完成
    /// </summary>
    private IEnumerator CalculateScoreWithDelay()
    {
        CalculateScore(5); // 使用较短的间隔加快计算过程
        
        // 等待计算完成
        while (isCalculatingScore)
        {
            yield return new WaitForEndOfFrame();
        }
        
        yield return WaitForFrames(UI_INTERVAL);
    }
    
    /// <summary>
    /// 显示符卡信息
    /// </summary>
    private IEnumerator ShowCard(GameObject cardObj, TextMeshProUGUI statusText, TextMeshProUGUI descText, 
        bool isGet, string successDesc, string failedDesc)
    {
        // 激活符卡物体
        cardObj?.SetActive(true);
        yield return WaitForFrames(UI_INTERVAL);
        
        // 设置收取状态
        if (statusText != null)
        {
            if (isGet)
            {
                statusText.text = "【收取成功】";
                statusText.color = Color.yellow;
            }
            else
            {
                statusText.text = "【收取失败】";
                statusText.color = Color.red;
            }
        }
        yield return WaitForFrames(UI_INTERVAL);
        
        // 设置符卡描述
        if (descText != null)
        {
            string text = isGet ? successDesc : failedDesc;
            descText.text = "";
            
            for (int i = 0; i < text.Length; i++)
            {
                descText.text += text[i];
                yield return new WaitForEndOfFrame();
            }
        }
        
        yield return WaitForFrames(UI_INTERVAL);
    }
    
    /// <summary>
    /// UI元素包装类
    /// </summary>
    public class UIElement
    {
        public GameObject gameObject;
        public TextMeshProUGUI textMeshPro;
        public string text; // 当类型为TextMeshProUGUI时的文本内容
        
        public UIElement(GameObject go)
        {
            gameObject = go;
            textMeshPro = null;
            text = null;
        }
        
        public UIElement(TextMeshProUGUI tmp, string txt)
        {
            textMeshPro = tmp;
            text = txt;
            gameObject = null;
        }
    }
    
    /// <summary>
    /// 逐字输出文本
    /// </summary>
    /// <param name="target">目标TextMeshProUGUI组件</param>
    /// <param name="text">要输出的文本</param>
    /// <param name="interval">输出间隔（帧）</param>
    /// <returns>是否输出完毕</returns>
    public bool PrintText(TextMeshProUGUI target, string text, int interval)
    {
        if (!isPrinting)
        {
            // 开始新的逐字输出
            currentPrintTarget = target;
            currentPrintText = text;
            currentCharIndex = 0;
            printFrameCounter = 0;
            printInterval = interval;
            isPrinting = true;
            target.text = "";
            
            if (printCoroutine != null)
            {
                StopCoroutine(printCoroutine);
            }
            printCoroutine = StartCoroutine(PrintTextCoroutine());
        }
        
        return !isPrinting;
    }
    
    /// <summary>
    /// 逐字输出协程
    /// </summary>
    private IEnumerator PrintTextCoroutine()
    {
        while (currentCharIndex < currentPrintText.Length)
        {
            printFrameCounter++;
            
            if (printFrameCounter >= printInterval)
            {
                printFrameCounter = 0;
                currentPrintTarget.text += currentPrintText[currentCharIndex];
                currentCharIndex++;
            }
            
            yield return null;
        }
        
        // 输出完毕
        isPrinting = false;
        printCoroutine = null;
    }
    
    /// <summary>
    /// 按顺序显示UI元素
    /// </summary>
    /// <param name="lists">UI元素列表</param>
    /// <param name="interval">间隔（帧）</param>
    public void ShowUIOneByOne(List<UIElement> lists, int interval)
    {
        if (isShowingUI)
        {
            // 如果正在显示，停止当前协程
            if (showUICoroutine != null)
            {
                StopCoroutine(showUICoroutine);
            }
        }
        
        currentUIList = lists;
        currentUIIndex = 0;
        uiFrameCounter = 0;
        uiInterval = interval;
        isShowingUI = true;
        
        showUICoroutine = StartCoroutine(ShowUIOneByOneCoroutine());
    }
    
    /// <summary>
    /// 顺序显示UI协程
    /// </summary>
    private IEnumerator ShowUIOneByOneCoroutine()
    {
        while (currentUIIndex < currentUIList.Count)
        {
            uiFrameCounter++;
            
            if (uiFrameCounter >= uiInterval)
            {
                uiFrameCounter = 0;
                
                UIElement element = currentUIList[currentUIIndex];
                
                if (element.gameObject != null)
                {
                    // 激活GameObject
                    element.gameObject.SetActive(true);
                }
                else if (element.textMeshPro != null && !string.IsNullOrEmpty(element.text))
                {
                    // 逐字输出文本
                    element.textMeshPro.text = "";
                    for (int i = 0; i < element.text.Length; i++)
                    {
                        element.textMeshPro.text += element.text[i];
                        yield return new WaitForEndOfFrame();
                    }
                }
                
                currentUIIndex++;
            }
            
            yield return null;
        }
        
        // 显示完毕
        isShowingUI = false;
        showUICoroutine = null;
    }
    
    /// <summary>
    /// 检查是否正在逐字输出
    /// </summary>
    public bool IsPrinting()
    {
        return isPrinting;
    }
    
    /// <summary>
    /// 检查是否正在显示UI
    /// </summary>
    public bool IsShowingUI()
    {
        return isShowingUI;
    }
    
    /// <summary>
    /// 停止所有动画
    /// </summary>
    public void StopAllAnimations()
    {
        if (printCoroutine != null)
        {
            StopCoroutine(printCoroutine);
            printCoroutine = null;
        }
        
        if (showUICoroutine != null)
        {
            StopCoroutine(showUICoroutine);
            showUICoroutine = null;
        }
        
        if (calculateScoreCoroutine != null)
        {
            StopCoroutine(calculateScoreCoroutine);
            calculateScoreCoroutine = null;
        }
        
        isPrinting = false;
        isShowingUI = false;
        isCalculatingScore = false;
    }
    
    /// <summary>
    /// 计算最终得分并逐帧输出计算过程
    /// </summary>
    /// <param name="interval">输出间隔（帧）</param>
    public void CalculateScore(int interval)
    {
        if (isCalculatingScore)
        {
            if (calculateScoreCoroutine != null)
            {
                StopCoroutine(calculateScoreCoroutine);
            }
        }
        
        isCalculatingScore = true;
        FinalGradeCalu.text = "";
        FinalGrade.text = "";
        
        calculateScoreCoroutine = StartCoroutine(CalculateScoreCoroutine(interval));
    }
    
    /// <summary>
    /// 计算得分协程
    /// </summary>
    private IEnumerator CalculateScoreCoroutine(int interval)
    {
        // 获取数据
        int grade = Global_GameManager.Instance.Grade;
        GameMode difficulty = Global_GameManager.Instance.gameMode;
        int graze = Global_GameManager.Instance.Graze;
        
        UIManager uiManager = FindObjectOfType<UIManager>();
        bool isContinueGame = uiManager != null ? uiManager.isContinueGame : false;
        bool card1Get = uiManager != null ? uiManager.isCard1Get : false;
        bool card2Get = uiManager != null ? uiManager.isCard2Get : false;
        bool finalCardGet = uiManager != null ? uiManager.isFinalCardGet : false;
        int exScore = uiManager != null ? uiManager.ExScore : 0;
        
        // 获取难度系数
        float difficultyMultiplier = GetDifficultyMultiplier(difficulty);
        string difficultyName = GetDifficultyName(difficulty);
        
        // 获取续关系数
        float continueMultiplier = isContinueGame ? 0.6f : 1.0f;
        string continueText = isContinueGame ? "续关" : "无续关";
        
        // 计算符卡收取数量
        int cardGetCount = 0;
        if (card1Get) cardGetCount++;
        if (card2Get) cardGetCount++;
        if (finalCardGet) cardGetCount++;
        
        // 输出基础得分
        FinalGradeCalu.text = grade.ToString() + "(基础得分)";
        yield return WaitForFrames(interval);
        
        // 输出 " x "
        FinalGradeCalu.text += " x ";
        yield return WaitForFrames(interval);
        
        // 输出难度系数
        FinalGradeCalu.text += difficultyMultiplier.ToString("F1") + "(" + difficultyName + ")";
        yield return WaitForFrames(interval);
        
        // 输出 " x "
        FinalGradeCalu.text += " x ";
        yield return WaitForFrames(interval);
        
        // 输出续关系数
        FinalGradeCalu.text += continueMultiplier.ToString("F1") + "(" + continueText + ")";
        yield return WaitForFrames(interval);
        
        // 换行 + 擦弹得分
        FinalGradeCalu.text += "\n+ " + GRAZE_SCORE_CONSTANT.ToString();
        yield return WaitForFrames(interval);
        
        // 输出 " x "
        FinalGradeCalu.text += " x ";
        yield return WaitForFrames(interval);
        
        // 输出擦弹数
        FinalGradeCalu.text += graze.ToString() + "(擦弹得分)";
        yield return WaitForFrames(interval);
        
        // 换行 + Bonus得分
        FinalGradeCalu.text += "\n+ " + BONUS_SCORE_CONSTANT.ToString();
        yield return WaitForFrames(interval);
        
        // 输出 " x "
        FinalGradeCalu.text += " x ";
        yield return WaitForFrames(interval);
        
        // 输出符卡收取数量
        FinalGradeCalu.text += cardGetCount.ToString() + "(Bonus得分)";
        yield return WaitForFrames(interval);
        
        // 换行 + 额外转化得分
        FinalGradeCalu.text += "\n+ " + exScore.ToString() +"(转化得分)";
        yield return WaitForFrames(interval);
        
        // 输出 " x "
        FinalGradeCalu.text += " x ";
        yield return WaitForFrames(interval);
        
        // 输出难度系数
        FinalGradeCalu.text += difficultyMultiplier.ToString("F1") + "(" + difficultyName + ")";
        yield return WaitForFrames(interval);
        
        // 输出 " = "
        FinalGradeCalu.text += " = ";
        yield return WaitForFrames(interval);
        
        // 计算最终得分
        int baseScore = Mathf.RoundToInt((float)grade * difficultyMultiplier * continueMultiplier);
        int grazeScore = GRAZE_SCORE_CONSTANT * graze;
        int bonusScore = BONUS_SCORE_CONSTANT * cardGetCount;
        int exScoreTotal = Mathf.RoundToInt((float)exScore * difficultyMultiplier);
        
        finalScoreResult = baseScore + grazeScore + bonusScore + exScoreTotal;
         
        // 设置最终得分显示
        FinalGrade.text = finalScoreResult.ToString();
        
        isCalculatingScore = false;
        calculateScoreCoroutine = null;
    }
    
    /// <summary>
    /// 获取难度系数
    /// </summary>
    private float GetDifficultyMultiplier(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Easy:
                return 0.8f;
            case GameMode.Normal:
                return 1.0f;
            case GameMode.Hard:
                return 1.5f;
            case GameMode.Lunatic:
                return 2.0f;
            default:
                return 1.0f;
        }
    }
    
    /// <summary>
    /// 获取难度名称
    /// </summary>
    private string GetDifficultyName(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Easy:
                return "Easy";
            case GameMode.Normal:
                return "Normal";
            case GameMode.Hard:
                return "Hard";
            case GameMode.Lunatic:
                return "Lunatic";
            default:
                return "Normal";
        }
    }
    
    /// <summary>
    /// 等待指定帧数
    /// </summary>
    private IEnumerator WaitForFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
    }
    
    /// <summary>
    /// 获取最终得分结果
    /// </summary>
    public int GetFinalScore()
    {
        return finalScoreResult;
    }
    
    /// <summary>
    /// 检查是否正在计算得分
    /// </summary>
    public bool IsCalculatingScore()
    {
        return isCalculatingScore;
    }
    
    void Update()
    {
        // 更新Chrino动画状态
        UpdateChrinoAnimation();
    }
    
    /// <summary>
    /// 更新Chrino动画状态
    /// </summary>
    private void UpdateChrinoAnimation()
    {
        if (ChrinoAnim == null) return;
        
        // 如果处于冷却期，更新冷却计时器（使用真实时间）
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            return;
        }
        
        // 更新计时器（使用真实时间，不受Time.timeScale影响）
        animationTimer += Time.unscaledDeltaTime;
        
        // 每秒检测一次
        if (animationTimer >= ANIMATION_CHECK_INTERVAL)
        {
            animationTimer = 0f;
            
            if (isStand)
            {
                // 站立状态：有几率触发旋转
                TryTriggerRotate();
            }
            else
            {
                // 非站立状态：有几率触发起身
                TryTriggerStandUp();
            }
        }
    }
    
    /// <summary>
    /// 尝试触发旋转动画
    /// </summary>
    private void TryTriggerRotate()
    {
        float randomValue = Random.value;
        if (randomValue <= rotateTriggerChance)
        {
            // 触发旋转
            ChrinoAnim.SetBool("IsRotate", true);
            ChrinoAnim.SetBool("IsStandUp", false);
            isStand = false;
            cooldownTimer = ANIMATION_COOLDOWN_TIME;
        }
    }
    
    /// <summary>
    /// 尝试触发起身动画
    /// </summary>
    private void TryTriggerStandUp()
    {   
        if (!isStand)
        {
            float randomValue = Random.value;
            if (randomValue <= standUpTriggerChance)
            {
                // 触发起身
                ChrinoAnim.SetBool("IsStandUp", true);
                ChrinoAnim.SetBool("IsRotate", false);
                isStand = true;
                cooldownTimer = ANIMATION_COOLDOWN_TIME;
            }
        }
    }
    
    /// <summary>
    /// 重置Chrino动画状态为站立
    /// </summary>
    public void ResetChrinoAnimation()
    {
        if (ChrinoAnim != null)
        {
            ChrinoAnim.SetBool("IsRotate", false);
            ChrinoAnim.SetBool("IsStandUp", true);
            isStand = true;
            cooldownTimer = 0f;
            animationTimer = 0f;
        }
    }
}

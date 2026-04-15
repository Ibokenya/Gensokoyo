using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class DialogData
{
    public int ID;
    public string Role;
    public string Text;
    public int Emotion;
}

public class AboutDialog : MonoBehaviour
{
    [Header("对话框相关物体引用")]
    public GameObject RoleFace;
    public GameObject DialogBox;
    private TextMeshProUGUI dialogText;
    private Image roleFaceImage;

    [Header("角色脸列表")]
    public List<Sprite> ReimuFace;
    public List<Sprite> MarisaFace;
    public List<Sprite> ChirnoFace;

    [Header("对话文件路径")]
    public string ReimuDialogPath = "Touho/对话/ReimuDialog.csv";
    public string MarisaDialogPath = "Touho/对话/MarisaDialog.csv";

    private Vector2 Face_left = new (-442.98f, 0);
    private Vector2 Face_right = new (442.98f, 0);
    private Vector2 Dialog_right = new (137.67f, 27.668f);
    private Vector2 Dialog_left = new (-117.67f, -27.668f);

    private List<DialogData> currentDialogList;
    private int currentDialogIndex = 0;
    private bool isDialogActive = false;

    private void Awake()
    {
        dialogText = DialogBox.GetComponent<TextMeshProUGUI>();
        roleFaceImage = RoleFace.GetComponent<Image>();
    }

    private void OnEnable()
    {
        Global_GameManager.Instance.state = State.Dialog;
        currentDialogIndex = 0;
        isDialogActive = true;
        
        // 设置初始位置：角色脸左侧，对话框右侧
        SetDialogPosition(false);
        
        // 根据角色类型选择对应的对话
        LoadDialog();
        
        // 显示第一条对话
        if (currentDialogList.Count > 0)
        {
            ShowDialog(currentDialogList[currentDialogIndex]);
        }
    }

    private void Update()
    {
        if(Global_GameManager.Instance.state != State.Dialog)
        {
            return;
        }
        if (!isDialogActive)
        {
            return;
        }

        // 按Z键切换到下一条对话
        if (Input.GetKeyDown(KeyCode.Z))
        {
            NextDialog();
        }
    }

    private void LoadDialog()
    {
        string dialogPath = "";
        
        // 根据角色类型选择对话文件
        if (Global_GameManager.Instance.character == Character.Reimu)
        {
            dialogPath = ReimuDialogPath;
        }
        else if (Global_GameManager.Instance.character == Character.Marisa)
        {
            dialogPath = MarisaDialogPath;
        }

        // 读取CSV文件
        currentDialogList = ReadCSV(dialogPath);
    }

    private List<DialogData> ReadCSV(string path)
    {
        List<DialogData> dialogList = new List<DialogData>();
        
        // 从Resources文件夹加载CSV文件
        TextAsset csvFile = Resources.Load<TextAsset>(path.Replace(".csv", ""));
        
        if (csvFile == null)
        {
            Debug.LogError("无法找到对话文件: " + path);
            return dialogList;
        }

        // 读取CSV内容并正确处理多行文本
        string csvContent = csvFile.text;
        Debug.Log($"CSV内容: {csvContent}");
        List<string> dialogLines = ParseCSVContent(csvContent);
        
        // 跳过标题行
        for (int i = 1; i < dialogLines.Count; i++)
        {
            string line = dialogLines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // 解析CSV行，支持多行text
            DialogData dialog = ParseCSVLine(line);
            if (dialog != null)
            {
                dialogList.Add(dialog);
            }
        }

        return dialogList;
    }
    
    /// <summary>
    /// 解析CSV内容，支持多行文本
    /// 把每次对话的所有内容(ID,Role,Text,Emotion)解析为一个个字符串存入列表中
    /// </summary>
    /// <param name="content">CSV内容字符串</param>
    /// <returns>包含每行文本的列表</returns>
    private List<string> ParseCSVContent(string content)
    {
        List<string> lines = new List<string>();
        string currentLine = "";
        bool inQuotes = false;// 是否在引号中
        
        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == '\n' && !inQuotes)// 换行符且不在引号中，存储当前行内容
            {
                lines.Add(currentLine);
                currentLine = "";
            }
            else
            {
                currentLine += c;
            }
        }
        
        // 添加最后一行
        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }
        
        return lines;
    }

    /// <summary>
    /// 解析CSV行，支持多行文本
    /// </summary>
    /// <param name="line">CSV行字符串</param>
    /// <returns>解析后的对话数据</returns>
    private DialogData ParseCSVLine(string line)
    {
        // CSV格式：ID,Role,Text,Emotion
        // 需要正确处理包含逗号的文本
        string[] parts = SplitCSVLine(line);
        
        if (parts.Length < 4)
        {
            return null;
        }

        DialogData dialog = new DialogData();
        dialog.ID = int.Parse(parts[0]);
        dialog.Role = parts[1];
        dialog.Text = parts[2];
        dialog.Emotion = int.Parse(parts[3]);

        return dialog;
    }

    /// <summary>
    /// 分割CSV行，支持多行文本
    /// </summary>
    /// <param name="line">CSV行字符串</param>
    /// <returns>包含每个字段的字符串数组</returns>
    private string[] SplitCSVLine(string line)
    {
        // 处理CSV中的引号，支持多行text
        List<string> parts = new List<string>();
        string currentPart = "";
        bool inQuotes = false;// 是否在引号中

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                parts.Add(currentPart);
                currentPart = "";
            }
            else
            {
                currentPart += c;
            }
        }
        
        parts.Add(currentPart);
        return parts.ToArray();
    }

    private void ShowDialog(DialogData dialog)
    {
        // 设置对话框文本
        dialogText.text = dialog.Text;
        
        // 根据角色类型选择脸
        Sprite faceSprite = null;
               
        if (dialog.Role == "Reimu")
        {
            if (dialog.Emotion >= 0 && dialog.Emotion < ReimuFace.Count)
            {
                faceSprite = ReimuFace[dialog.Emotion];
            }
            SetDialogPosition(false);
        }
        else if (dialog.Role == "Marisa")
        {
            if (dialog.Emotion >= 0 && dialog.Emotion < MarisaFace.Count)
            {
                faceSprite = MarisaFace[dialog.Emotion];
            }
            SetDialogPosition(false);
        }
        else if (dialog.Role == "Chirno")
        {
            if (dialog.Emotion >= 0 && dialog.Emotion < ChirnoFace.Count)
            {
                faceSprite = ChirnoFace[dialog.Emotion];
            }
            SetDialogPosition(true);
        }
        
        // 设置角色脸
        if (faceSprite != null)
        {
            roleFaceImage.sprite = faceSprite;
        }
    }

    private void SetDialogPosition(bool isEnemy)
    {
        if (isEnemy)
        {
            // 敌人：RoleFace到右侧并x轴翻转，对话框到左侧
            RoleFace.transform.localPosition = Face_right;
            RoleFace.transform.localScale = new Vector3(-1, 1, 1);
            DialogBox.transform.localPosition = Dialog_left;
        }
        else
        {
            // 玩家：RoleFace左侧，对话框右侧
            RoleFace.transform.localPosition = Face_left;
            RoleFace.transform.localScale = new Vector3(1, 1, 1);
            DialogBox.transform.localPosition = Dialog_right;
        }
    }

    private void NextDialog()
    {
        currentDialogIndex++;
        
        if (currentDialogIndex >= currentDialogList.Count)
        {
            // 对话结束，禁用对话框
            isDialogActive = false;
            Global_GameManager.Instance.state = State.Gaming;
            gameObject.SetActive(false);
        }
        else
        {
            // 显示下一条对话
            ShowDialog(currentDialogList[currentDialogIndex]);
        }
    }

    private void OnDisable()
    {
        isDialogActive = false;
    }
}



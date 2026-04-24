using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 最终符卡宣言
/// 这是UI部分的
/// </summary>
public class FinalWords : MonoBehaviour
{
    public TextMeshProUGUI TextRight;
    public TextMeshProUGUI TextLeft;
    public GameObject DialogBalloon; // 对话框物体引用

    [Header("最终宣言文件路径")]
    public string finalWordsPath = "Touho/对话/FinalWords.csv";

    private List<FinalWordsData> finalWordsList;
    private int currentIndex = 0;
    private bool isActive = false;

    [System.Serializable]
    public class FinalWordsData
    {
        public int ID;
        public string Right;
        public string Left;
        public float Interval;
    }

    private void OnEnable()
    {
        isActive = true;
        currentIndex = 0;
        
        // 加载最终宣言数据
        LoadFinalWords();
        
        // 开始显示最终宣言
        StartCoroutine(ShowFinalWordsSequence());
    }

    private void LoadFinalWords()
    {
        finalWordsList = new List<FinalWordsData>();
        
        // 从Resources加载CSV文件
        TextAsset csvFile = Resources.Load<TextAsset>(finalWordsPath.Replace(".csv", ""));
        
        if (csvFile == null)
        {
            Debug.LogError("无法找到最终宣言文件: " + finalWordsPath);
            return;
        }

        // 解析CSV内容
        string csvContent = csvFile.text;
        List<string> lines = ParseCSVContent(csvContent);
        
        // 处理每一行数据
        for (int i = 1; i < lines.Count; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
            
            FinalWordsData data = ParseCSVLine(line);
            if (data != null)
            {
                finalWordsList.Add(data);
            }
        }
    }

    /// <summary>
    /// 解析CSV内容
    /// </summary>
    private List<string> ParseCSVContent(string content)
    {
        List<string> lines = new List<string>();
        string currentLine = "";
        
        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];
            
            if (c == '\n')
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
    /// 解析CSV行
    /// </summary>
    private FinalWordsData ParseCSVLine(string line)
    {
        string[] parts = line.Split(',');
        
        if (parts.Length < 4)
            return null;
        
        FinalWordsData data = new FinalWordsData();
        data.ID = int.Parse(parts[0]);
        data.Right = parts[1];
        data.Left = parts[2];
        
        // 解析间隔时间
        string intervalStr = parts[3].Trim();

        data.Interval = float.Parse(intervalStr);
        
        return data;
    }

    /// <summary>
    /// 显示最终宣言序列
    /// </summary>
    private IEnumerator ShowFinalWordsSequence()
    {
        yield return new WaitForSeconds(0.5f);    
        while (isActive && currentIndex < finalWordsList.Count)
        {
            FinalWordsData data = finalWordsList[currentIndex];
            
            // 显示当前宣言
            TextRight.text = data.Right;
            TextLeft.text = data.Left;
            
            // 等待指定间隔
            yield return new WaitForSeconds(data.Interval);
            
            // 移动到下一条
            currentIndex++;
        }
        
        // 所有宣言显示完毕
        EndFinalWords();
    }

    /// <summary>
    /// 结束最终宣言
    /// </summary>
    private void EndFinalWords()
    {
        // 清空文本
        TextRight.text = "";
        TextLeft.text = "";
        
        // 隐藏对话框
        if (DialogBalloon != null)
        {
            DialogBalloon.SetActive(false);
        }
        
        isActive = false;
    }

    private void OnDisable()
    {
        isActive = false;
    }
}

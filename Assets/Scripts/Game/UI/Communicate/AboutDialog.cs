using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text;

public class AboutDialog : MonoBehaviour
{
    [Header("对话框相关物体引用")]
    public GameObject RoleFace;
    public GameObject DialogBox;
    private TextMeshProUGUI dialogText;

    [Header("角色脸集合")]
    public List<Sprite> ReimuFace;
    public List<Sprite> MarisaFace;
    public List<Sprite> CirnoFace;

    [Header("对话文件路径")]
    public string dialogFilePath = "Assets/Touho/对话/MarisaDialog.csv";

    private Vector2 Face_left = new (-442.98f, 0);
    private Vector2 Face_right = new (442.98f, 0);
    private Vector2 Dialog_right = new (137.67f, 27.668f);
    private Vector2 Dialog_left = new (-137.67f, -27.668f);

    // 对话数据结构
    private class DialogData
    {
        public int ID;
        public string Role;
        public string Text;
        public int Emotion;
    }

    private List<DialogData> dialogs = new List<DialogData>();
    private int currentDialogIndex = 0;
    private SpriteRenderer roleFaceRenderer;

    private void OnEnable()
    {
        // 初始化组件
        dialogText = DialogBox.GetComponentInChildren<TextMeshProUGUI>();
        roleFaceRenderer = RoleFace.GetComponent<SpriteRenderer>();

        // 读取对话数据
        LoadDialogData();

        // 显示第一条对话
        if (dialogs.Count > 0)
        {
            ShowDialog(currentDialogIndex);
        }
    }

    private void Update()
    {
        // 按Z键切换到下一条对话
        if (Input.GetKeyDown(KeyCode.Z))
        {
            NextDialog();
        }
    }

    // 读取对话数据
    private void LoadDialogData()
    {
        dialogs.Clear();
        string fullPath = Path.Combine(Application.dataPath, dialogFilePath);

        if (File.Exists(fullPath))
        {
            // 使用UTF-8编码读取文件
            using (StreamReader reader = new StreamReader(fullPath, Encoding.UTF8))
            {
                // 跳过表头
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        DialogData dialog = new DialogData();
                        dialog.ID = int.Parse(parts[0]);
                        dialog.Role = parts[1];
                        // 处理包含逗号的文本
                        dialog.Text = string.Join(",", parts, 2, parts.Length - 3);
                        dialog.Emotion = int.Parse(parts[parts.Length - 1]);
                        dialogs.Add(dialog);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("对话文件不存在: " + fullPath);
        }
    }

    // 显示指定索引的对话
    private void ShowDialog(int index)
    {
        if (index < 0 || index >= dialogs.Count)
            return;

        DialogData dialog = dialogs[index];

        // 设置对话框文本
        if (dialogText != null)
        {
            dialogText.text = dialog.Text;
        }

        // 设置角色脸和位置
        SetRoleFaceAndPosition(dialog.Role, dialog.Emotion);
    }

    // 设置角色脸和位置
    private void SetRoleFaceAndPosition(string role, int emotion)
    {
        if (roleFaceRenderer == null)
            return;

        // 根据角色设置脸和位置
        switch (role)
        {
            case "Marisa":
                // 设置魔理沙的脸
                if (emotion > 0 && emotion <= MarisaFace.Count)
                {
                    roleFaceRenderer.sprite = MarisaFace[emotion - 1];
                }
                // 魔理沙在左侧
                RoleFace.transform.localPosition = Face_left;
                // 不翻转
                roleFaceRenderer.flipX = false;
                // 对话框在右侧
                DialogBox.transform.localPosition = Dialog_right;
                break;

            case "Reimu":
                // 设置灵梦的脸
                if (emotion > 0 && emotion <= ReimuFace.Count)
                {
                    roleFaceRenderer.sprite = ReimuFace[emotion - 1];
                }
                // 灵梦在左侧
                RoleFace.transform.localPosition = Face_left;
                // 不翻转
                roleFaceRenderer.flipX = false;
                // 对话框在右侧
                DialogBox.transform.localPosition = Dialog_right;
                break;

            case "Chirno":
                // 设置琪露诺的脸
                if (emotion > 0 && emotion <= CirnoFace.Count)
                {
                    roleFaceRenderer.sprite = CirnoFace[emotion - 1];
                }
                // 琪露诺在右侧
                RoleFace.transform.localPosition = Face_right;
                // 沿X轴翻转180度
                roleFaceRenderer.flipX = true;
                // 对话框在左侧
                DialogBox.transform.localPosition = Dialog_left;
                break;
        }
    }

    // 切换到下一条对话
    private void NextDialog()
    {
        currentDialogIndex++;
        if (currentDialogIndex < dialogs.Count)
        {
            ShowDialog(currentDialogIndex);
        }
        else
        {
            // 对话结束，关闭对话框
            gameObject.SetActive(false);
        }
    }
}

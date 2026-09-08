using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ReplaySystem;

public class PauseEvent : MonoBehaviour
{
    [Header("暂停界面按钮")]
    public List<TextMeshProUGUI> pauseButtons = new();
    public GameObject Really; // Yes/No 父物体（两个状态共用同一个面板，靠位置区分：确认执行→按钮原位；保存回放→pos 下方）
    public TextMeshProUGUI Yes;
    public TextMeshProUGUI No;
    public TextMeshProUGUI DescriptionText;
    public PauseUI pauseUI;
    public AudioClip Choose;
    public AudioClip Click;
    public AudioClip Stop;

    public GameObject pausePanel1;
    public GameObject pausePanel2;
    public GameObject pausePanel3;

    [Header("说明书相关")]
    public List<TextMeshProUGUI> manualTexts = new();
    public List<TextMeshProUGUI> manualPanels = new();
    public GameObject manualPanel;
    public GameObject manual;
    public GameObject shadel;

    private readonly Color CancelAlphaColor = new (1,1,1,0.3f);
    private readonly Color FullAlphaColor = new (1,1,1,1f);

    private int  index = 0;        // 当前选中的 pauseButton 索引
    private bool isReally = false; // 是否处于"确认执行按钮操作"环节（第一层）
    private bool isRecording = false; // 是否处于"保存回放确认"环节（第二层）
    private bool YesOrNo = false;  // 默认选 No

    // 说明书相关变量
    private readonly Color darkColor = new(0.5f, 0.5f, 0.5f);
    private readonly Color lightColor = new(1f, 1f, 1f);
    private readonly Vector3 savePanelPos = new(0, -300, 0); // 保存回放确认面板位置
    private readonly float PanelAlpha = 0.7f;
    private int  manualIndex = 0;
    private int  lastManualIndex = 0;
    private bool isManualIndex = true;
    private bool isManualActive = false;

    // ---- 生命周期 ----

    void OnEnable()
    {
        index = 0;
        BeChoose(index, true);
        Really.SetActive(false);
        isReally = false;
        isRecording = false;

        // 初始化说明书状态
        manualIndex = 0;
        isManualIndex = true;
        isManualActive = false;
        if (manual != null) manual.SetActive(false);
        if (manualPanel != null) manualPanel.SetActive(false);
        if (shadel != null) shadel.SetActive(false);

        foreach (TextMeshProUGUI text in manualTexts) text.color = darkColor;
        if (manualTexts.Count > 0 && manualTexts[0] != null) manualTexts[0].color = lightColor;
        foreach (TextMeshProUGUI panel in manualPanels) panel.alpha = 0;
    }

    void OnDisable()
    {
        // 退出暂停菜单：清掉所有高亮
        foreach (var btn in pauseButtons) btn.color = CancelAlphaColor;
        pauseButtons[0].color = FullAlphaColor;
        Really.SetActive(false);
        isReally = false;
        isRecording = false;
    }

    // 暂停菜单导航在 Update：timeScale=0 后也要能继续
    void Update()
    {
        CheckChoose();
    }

    // ---- 主调度 ----

    private void CheckChoose()
    {
        // 说明书激活时优先处理（不经过 isReally/isRecording）
        if (isManualActive)
        {
            HandleManual();
            return;
        }

        if (isRecording)
        {
            HandleRecordingConfirm(); // 第二层：保存回放确认
        }
        else if (isReally)
        {
            HandleActionConfirm();    // 第一层：确认执行按钮操作
        }
        else
        {
            HandleMenuNavigation();   // 非确认：按钮导航
        }
    }

    // ---- 说明书（保持原样） ----

    private void HandleManual()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            lastManualIndex = manualIndex;
            manualIndex = (manualIndex - 1 + manualTexts.Count) % manualTexts.Count;
            UpdateManual();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            lastManualIndex = manualIndex;
            manualIndex = (manualIndex + 1) % manualTexts.Count;
            UpdateManual();
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Fire) && isManualIndex)
        {
            Global_AudioManager.Instance.PlaySFX(Click);
            isManualIndex = false;
            shadel.SetActive(true);
            if (manual != null) manual.SetActive(false);
            foreach (TextMeshProUGUI text in manualTexts) text.alpha = 0;
            manualPanels[manualIndex].alpha = PanelAlpha;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (!isManualIndex)
            {
                isManualIndex = true;
                shadel.SetActive(false);
                if (manual != null) manual.SetActive(true);
                foreach (TextMeshProUGUI text in manualTexts) text.alpha = 1;
                manualPanels[manualIndex].alpha = 0;
                foreach (TextMeshProUGUI text in manualTexts) text.color = darkColor;
                manualTexts[manualIndex].color = lightColor;
            }
            else
            {
                CloseManual();
            }
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Cancel))
        {
            CloseManual();
        }
    }

    private void CloseManual()
    {
        isManualActive = false;
        if (manual != null) manual.SetActive(false);
        if (manualPanel != null) manualPanel.SetActive(false);
        if (shadel != null) shadel.SetActive(false);
        pausePanel1.SetActive(true);
        pausePanel2.SetActive(true);
        pausePanel3.SetActive(true);
        BeChoose(index, true);
    }

    private void UpdateManual()
    {
        Global_AudioManager.Instance.PlaySFX(Choose);
        if (isManualIndex)
        {
            manualTexts[lastManualIndex].color = darkColor;
            manualTexts[manualIndex].color = lightColor;
        }
        else
        {
            manualPanels[lastManualIndex].alpha = 0;
            manualPanels[manualIndex].alpha = PanelAlpha;
        }
    }

    // ---- 第一层确认：确认执行按钮操作 ----

    private void HandleActionConfirm()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            YesOrNo = !YesOrNo;
            UpdateConfirmColor();
            Global_AudioManager.Instance.PlaySFX(Choose);
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Fire))
        {
            if (YesOrNo)
            {
                // 确认执行 —— 如果是需要离开游戏的操作（回菜单/重开），先问"保存回放？"
                if (NeedsSaveConfirm(index) && IsRecording())
                {
                    EnterRecordingConfirm();
                }
                else
                {
                    CommitAndExecute(index);
                }
            }
            else
            {
                BackToPause();
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || ReplayManager.Input.GetKeyDown(LogicalKey.Cancel))
        {
            BackToPause();
        }
    }

    /// <summary>是否是"需要离开游戏"、因而应询问回放去留的按钮</summary>
    private static bool NeedsSaveConfirm(int buttonIndex) =>
        buttonIndex == 1 || // 回菜单
        buttonIndex == 3;   // 重开

    private static bool IsRecording() =>
        ReplayManager.Instance != null &&
        ReplayManager.Instance.CurrentMode == ReplayManager.Mode.Record;

    /// <summary>
    /// 进入"是否保存回放"确认（第二层）。
    /// Really 面板从按钮位置移到 savePanelPos（0, -210, 0），
    /// DescriptionText 显式询问，按钮文本 Yes/No 对应保存/不保存。
    /// </summary>
    private void EnterRecordingConfirm()
    {
        isRecording = true;
        Really.transform.position = DescriptionText.transform.position + savePanelPos;
        Really.SetActive(true);
        YesOrNo = false; // 默认选 No
        if (DescriptionText != null) DescriptionText.text = "是否保存回放？";
        UpdateConfirmColor();
        Global_AudioManager.Instance.PlaySFX(Choose);
    }

    /// <summary>处理保存回放确认环节的按键</summary>
    private void HandleRecordingConfirm()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            YesOrNo = !YesOrNo;
            UpdateConfirmColor();
            Global_AudioManager.Instance.PlaySFX(Choose);
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Fire))
        {
            // 先处理回放文件，再真正执行按钮操作
            if (YesOrNo)
            {
                string path = ReplayManager.SaveRecording();
                Debug.Log($"[ReplayManager] PauseEvent 保存回放: {path}");
            }
            else
            {
                ReplayManager.DiscardRecording();
                Debug.Log("[ReplayManager] PauseEvent 丢弃回放");
            }

            // 回到 isReally 环节，执行真正的按钮操作
            isRecording = false;
            CommitAndExecute(index);
        }
        else if (Input.GetKeyDown(KeyCode.X) || ReplayManager.Input.GetKeyDown(LogicalKey.Cancel))
        {
            // 回退到上一层（确认执行环节）—— DescriptionText 恢复为按钮确认文本
            ExitRecordingConfirm();
        }
    }

    private void ExitRecordingConfirm()
    {
        isRecording = false;
        YesOrNo = false; // 回到上一层时，默认仍选 No（不执行）
        UpdateConfirmColor();
        Global_AudioManager.Instance.PlaySFX(Choose);
    }

    /// <summary>保存或丢弃回放后，执行 pauseButtons[index] 真正的操作</summary>
    private void CommitAndExecute(int btnIndex)
    {
        // 无论上一层是确认执行还是中途跳过保存确认面板，这里统一收尾
        isReally = false;
        isRecording = false;
        Really.SetActive(false);

        switch (btnIndex)
        {
            case 0:
                pauseUI.Resume();
                break;
            case 1:
                // 回菜单
                if (Global_GameManager.Instance != null)
                    Global_GameManager.Instance.RecycleAllEnemies();
                Global_SceneManager.Instance.IntoNextScene("GameStartMenu", false);
                break;
            case 2:
                OpenManual();
                break;
            case 3:
                // 重开
                Global_SceneManager.Instance.RestartGame();
                break;
        }
    }

    // ---- 非确认：暂停菜单按钮导航 ----

    private void HandleMenuNavigation()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            BeChooseCancel(index);
            if (index == 0) index = pauseButtons.Count - 1;
            else index--;
            BeChoose(index);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            BeChooseCancel(index);
            if (index == pauseButtons.Count - 1) index = 0;
            else index++;
            BeChoose(index);
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Fire))
        {
            BeClick(index);
        }
        else if (Input.GetKeyDown(KeyCode.X) || ReplayManager.Input.GetKeyDown(LogicalKey.Cancel))
        {
            pauseUI.Resume();
        }
    }

    // ---- UI 辅助 ----

    private void BeChoose(int i, bool isOnEnable = false)
    {
        if (isOnEnable)
        {
            Global_AudioManager.Instance.PlaySFX(Stop);
            return;
        }
        Global_AudioManager.Instance.PlaySFX(Choose);
        pauseButtons[i].color = FullAlphaColor;
    }

    private void BeChooseCancel(int i)
    {
        pauseButtons[i].color = CancelAlphaColor;
    }

    private void BeClick(int i)
    {
        Global_AudioManager.Instance.PlaySFX(Click);
        isReally = true;
        MakeReally(i);
    }

    private void MakeReally(int i)
    {
        // 第一层确认：把 Really 放到按钮原位（与该按钮重叠），Yes/No 左右并排
        pauseButtons[i].alpha = 0f;
        Really.transform.position = pauseButtons[i].transform.position;
        Really.SetActive(true);
        YesOrNo = false;
        UpdateConfirmColor();
    }

    private void BackToPause()
    {
        isReally = false;
        isRecording = false;
        BeChoose(index);
        Really.SetActive(false);
        if (DescriptionText != null) DescriptionText.text = "";
    }

    private void UpdateConfirmColor()
    {
        if (YesOrNo)
        {
            Yes.color = lightColor;
            No.color  = darkColor;
        }
        else
        {
            Yes.color = darkColor;
            No.color  = lightColor;
        }
    }

    private void OpenManual()
    {
        pauseButtons[2].color = FullAlphaColor;
        isManualActive = true;
        isManualIndex = true;
        manualIndex = 0;
        lastManualIndex = 0;

        pausePanel1.SetActive(false);
        pausePanel2.SetActive(false);
        pausePanel3.SetActive(false);

        if (manualPanel != null) manualPanel.SetActive(true);
        if (manual != null) manual.SetActive(true);
        if (shadel != null) shadel.SetActive(false);

        foreach (TextMeshProUGUI text in manualTexts)
        {
            text.color = darkColor;
            text.alpha = 1;
        }
        if (manualTexts.Count > 0) manualTexts[0].color = lightColor;

        foreach (TextMeshProUGUI panel in manualPanels) panel.alpha = 0;
    }
}

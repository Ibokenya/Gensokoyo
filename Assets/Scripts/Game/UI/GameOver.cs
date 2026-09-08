using System;
using System.Collections.Generic;
using TMPro;
using ReplaySystem;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI Continue;
    public TextMeshProUGUI ReStart;
    public TextMeshProUGUI Exit;
    public TextMeshProUGUI DescriptionText;
    public AudioClip SelectSound;
    public UIManager uiManager;

    [Header("保存回放确认")]
    public GameObject Really;           // Yes/No 父物体
    public TextMeshProUGUI Yes;         // "保存回放"
    public TextMeshProUGUI No;          // "不保存"
    public Vector3 confirmPanelOffset = new(-58f, 0f, 0f); // Really 相对于 Description 的偏移（GameOver 里是左移 58px）

    private bool IsReally = false;       // 是否处于"保存回放"确认环节
    private bool YesOrNo = false;        // 默认选 No

    private int CurrentIndex = 0;        // 0=Continue 1=ReStart 2=Exit 3=箭头
    private string currentBGMName = "";
    private float currentBgmPosition = 0f;

    private Color DefaultColor = new(0.5f, 0.5f, 0.5f, 0.5f);
    private Color SelectedColor = new(1f, 1f, 1f, 1f);
    private Color ConfirmYesColor = new(1f, 1f, 1f, 1f);
    private Color ConfirmNoColor  = new(0.5f, 0.5f, 0.5f, 1f);
    private readonly List<TextMeshProUGUI> Options = new();

    void OnEnable()
    {
        Time.timeScale = 0f;
        currentBGMName = Global_AudioManager.Instance.GetCurrentBGMName();
        currentBgmPosition = Global_AudioManager.Instance.GetCurrentBGMPosition();
        Global_AudioManager.Instance.StopBGM();

        CurrentIndex = 3;
        IsReally = false;
        YesOrNo = false;

        Global_GameManager.Instance.state = State.Over;

        Options.Clear();
        Options.Add(Continue);
        Options.Add(ReStart);
        Options.Add(Exit);

        // 确保确认面板默认隐藏
        if (Really != null) Really.SetActive(false);
    }

    // 菜单 UI 交互必须在 Update：timeScale=0 时 FixedUpdate 停止，菜单会冻死
    private void Update()
    {
        if (IsReally)
        {
            HandleConfirmStep();
        }
        else
        {
            HandleMenuStep();
        }
    }

    /// <summary>非确认环节：Continue/ReStart/Exit 三选一</summary>
    private void HandleMenuStep()
    {
        if (CurrentIndex == 3)
        {
            if (ReplayManager.Input.GetKeyDown(LogicalKey.Up) ||
                ReplayManager.Input.GetKeyDown(LogicalKey.Down))
            {
                CurrentIndex = 0;
                SelectOption(CurrentIndex);
            }
            return;
        }

        if (ReplayManager.Input.GetKeyDown(LogicalKey.Up))
        {
            RemoveOptions(CurrentIndex);
            CurrentIndex--;
            if (CurrentIndex < 0) CurrentIndex = Options.Count - 1;
            SelectOption(CurrentIndex);
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Down))
        {
            RemoveOptions(CurrentIndex);
            CurrentIndex++;
            if (CurrentIndex > Options.Count - 1) CurrentIndex = 0;
            SelectOption(CurrentIndex);
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Fire))
        {
            Global_AudioManager.Instance.PlaySFX(SelectSound);

            // 进入确认环节：先问"是否保存回放"，再执行对应的动作
            if (IsRecording())
            {
                EnterSaveConfirm();
            }
            else
            {
                // 没有正在录制的回放，直接执行动作（不浪费用户时间）
                ExecuteChosenAction();
            }
        }
    }

    /// <summary>确认环节：YesOrNo 切换保存/丢弃 → Z 确认 → 执行 → X/Esc 回退</summary>
    private void HandleConfirmStep()
    {
        if (ReplayManager.Input.GetKeyDown(LogicalKey.Left) ||
            ReplayManager.Input.GetKeyDown(LogicalKey.Right))
        {
            YesOrNo = !YesOrNo;
            UpdateConfirmColor();
            Global_AudioManager.Instance.PlaySFX(SelectSound);
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Fire))
        {
            CommitSaveOrDiscard();
            ExecuteChosenAction();
        }
        else if (ReplayManager.Input.GetKeyDown(LogicalKey.Cancel) ||
                 ReplayManager.Input.GetKeyDown(LogicalKey.Spell))
        {
            // 回退到上一步（保存确认 → 主菜单三选一）
            ExitSaveConfirm();
        }
    }

    /// <summary>是否存在正在录制、需要玩家决定去留的回放</summary>
    private static bool IsRecording()
    {
        return ReplayManager.Instance != null &&
               ReplayManager.Instance.CurrentMode == ReplayManager.Mode.Record;
    }

    /// <summary>进入"是否保存回放"确认</summary>
    private void EnterSaveConfirm()
    {
        // 隐藏当前选中的选项文本
        TextMeshProUGUI chosen = Options[CurrentIndex];
        chosen.alpha = 0f;

        // Really 面板（Yes/No）放到该选项的左边 —— 和现有 SaveRecording 方法保持一致
        Really.transform.position = chosen.transform.position + (Vector3)confirmPanelOffset;
        Really.SetActive(true);

        // 显示描述文本
        if (DescriptionText != null)
            DescriptionText.text = "是否保存回放？";

        YesOrNo = false; // 默认选 No
        UpdateConfirmColor();
        IsReally = true;
    }

    /// <summary>退出保存确认，恢复主菜单三选一显示</summary>
    private void ExitSaveConfirm()
    {
        if (Really != null) Really.SetActive(false);

        // 恢复被隐藏的选项
        TextMeshProUGUI chosen = Options[CurrentIndex];
        chosen.alpha = 1f;

        SetDescription(CurrentIndex);
        IsReally = false;
    }

    /// <summary>根据 YesOrNo 高亮 Yes/No 按钮</summary>
    private void UpdateConfirmColor()
    {
        if (YesOrNo)
        {
            Yes.color = ConfirmYesColor;
            No.color  = ConfirmNoColor;
        }
        else
        {
            Yes.color = ConfirmNoColor;
            No.color  = ConfirmYesColor;
        }
    }

    /// <summary>保存或丢弃回放（根据 YesOrNo），无论如何关闭录制模式</summary>
    private void CommitSaveOrDiscard()
    {
        if (!IsRecording()) return;

        if (YesOrNo)
        {
            string path = ReplayManager.SaveRecording();
            Debug.Log($"[ReplayManager] GameOver 保存回放: {path}");
        }
        else
        {
            ReplayManager.DiscardRecording();
            Debug.Log("[ReplayManager] GameOver 丢弃回放");
        }
    }

    /// <summary>根据 CurrentIndex 执行 Continue/ReStart/Exit —— 保存/丢弃回放后才走这里</summary>
    private void ExecuteChosenAction()
    {
        // 确认面板已经可以关掉了
        if (Really != null) Really.SetActive(false);

        switch (CurrentIndex)
        {
            case 0:
                ContinueGame();
                break;

            case 1:
                Global_SceneManager.Instance.RestartGame();
                Time.timeScale = 1f;
                Global_GameManager.Instance.state = State.Gaming;
                gameObject.SetActive(false);
                break;

            case 2:
                // 回收所有敌人
                if (Global_GameManager.Instance != null)
                    Global_GameManager.Instance.RecycleAllEnemies();

                Time.timeScale = 1f;
                Global_SceneManager.Instance.IntoNextScene("GameStartMenu", false);
                gameObject.SetActive(false);
                break;
        }
    }

    // ---- 辅助 ----

    private void SelectOption(int index)
    {
        Global_AudioManager.Instance.PlaySFX(SelectSound);
        RemoveOptions(index == 0 ? Options.Count - 1 : index - 1); // 只清掉旧的，防止索引回绕时残留
        Options[index].color = SelectedColor;
        SetDescription(index);
    }

    private void RemoveOptions(int index)
    {
        if (index < 0 || index >= Options.Count) return;
        Options[index].color = DefaultColor;
    }

    private void SetDescription(int index)
    {
        if (DescriptionText == null) return;
        string playername = Global_GameManager.Instance.character == Character.Reimu ? "灵梦" : "魔理沙";
        DescriptionText.text = index switch
        {
            0 => "借助不死秘药立刻重返战场，但这种作弊行为可是不会被计入结果的",
            1 => $"后来，休整完毕后的{playername}再度前来挑战",
            2 => $"{playername}太累了，就这样吧，先回去歇两天再说……至少先洗个澡换身衣服",
            _ => ""
        };
    }

    /// <summary>续关功能：恢复玩家状态并继续游戏</summary>
    private void ContinueGame()
    {
        Time.timeScale = 1f;
        Global_GameManager.Instance.state = State.Gaming;

        // 恢复玩家 HP 为 2,0
        Global_GameManager.Instance.Hp = 0;
        Global_GameManager.Instance.HpPiece = 0;
        Global_GameManager.Instance.AddLeftLife(2, 0);

        // 恢复玩家 Bomb 为 2,0
        Global_GameManager.Instance.SetBomb(2, 0);

        Global_GameManager.Instance.AddPower(100);
        Global_GameManager.Instance.AddPower(100);
        Global_GameManager.Instance.AddPower(100);
        Global_GameManager.Instance.AddPower(100);

        if (uiManager != null) uiManager.isContinueGame = true;

        Global_AudioManager.Instance.PlayBGM(currentBGMName);
        Global_AudioManager.Instance.SetBGMPosition(currentBgmPosition);
        Global_GameManager.Instance.ReBack();

        gameObject.SetActive(false);
    }
}

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

    // Canvas 缩放缓存（EnterSaveConfirm 中用偏移量移动 Really 面板时需要乘以 scaleFactor）
    private Canvas _parentCanvas;

    private float GetCanvasScaleFactor()
    {
        if (_parentCanvas == null)
        {
            if (Really != null) _parentCanvas = Really.GetComponentInParent<Canvas>();
            if (_parentCanvas == null)
            {
                Debug.LogError("[GameOver] GetCanvasScaleFactor: Really 未挂在 Canvas 下，UI 偏移将使用 1:1 缩放");
                return 1f;
            }
        }
        return _parentCanvas.scaleFactor;
    }

    void OnEnable()
    {
        //  注册硬暂停 —— GameOver 也要覆盖其他软缩放
        TimeScaleController.RegisterHardPause();
        //  关键：正常暂停/回放暂停都调了 AddSkipTickReason()，GameOver 漏了！
        //  SkipTick 让 ReplayManager.LateUpdate return early → SimClock 不推进
        //  否则 none2 等协程继续跑 yield return null + SimClock.SimTime → Boss 继续移动
        ReplayManager.AddSkipTickReason();
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

    private void HandleMenuStep()
    {
        //  meta 层 UI（菜单导航）直接读 Unity Input，不走 ReplayManager.Input
        // 因为 timeScale=0 时 FixedUpdate 不跑 → ConsumeEdges 不执行 → 边沿永远不被清 → 菜单疯狂滚动
        if (CurrentIndex == 3)
        {
            if (Input.GetKeyDown(PhysicalKeyMapping.Up) || Input.GetKeyDown(PhysicalKeyMapping.Down))
            {
                CurrentIndex = 0;
                SelectOption(CurrentIndex);
            }
            return;
        }

        if (Input.GetKeyDown(PhysicalKeyMapping.Up))
        {
            RemoveOptions(CurrentIndex);
            CurrentIndex--;
            if (CurrentIndex < 0) CurrentIndex = Options.Count - 1;
            SelectOption(CurrentIndex);
        }
        else if (Input.GetKeyDown(PhysicalKeyMapping.Down))
        {
            RemoveOptions(CurrentIndex);
            CurrentIndex++;
            if (CurrentIndex > Options.Count - 1) CurrentIndex = 0;
            SelectOption(CurrentIndex);
        }
        else if (Input.GetKeyDown(PhysicalKeyMapping.Z))
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

    private void HandleConfirmStep()
    {
        if (Input.GetKeyDown(PhysicalKeyMapping.Left) || Input.GetKeyDown(PhysicalKeyMapping.Right))
        {
            YesOrNo = !YesOrNo;
            UpdateConfirmColor();
            Global_AudioManager.Instance.PlaySFX(SelectSound);
        }
        else if (Input.GetKeyDown(PhysicalKeyMapping.Z))
        {
            CommitSaveOrDiscard();
            ExecuteChosenAction();
        }
        else if (Input.GetKeyDown(PhysicalKeyMapping.X) || Input.GetKeyDown(PhysicalKeyMapping.Escape))
        {
            // 回退到上一步（保存确认 → 主菜单三选一）
            ExitSaveConfirm();
        }
    }

    // 是否存在"可以保存"的正在录制的回放。
    // 如果玩家已经续过关（HasContinued=true），回放文件从重生节点开始，
    // 后半段是残缺的，回放时会当作从头放导致确定性错位 —— 直接丢弃不询问。
    private static bool IsRecording()
    {
        return ReplayManager.Instance != null &&
               ReplayManager.Instance.CurrentMode == ReplayManager.Mode.Record &&
               !ReplayManager.Instance.HasContinued;
    }

    private void EnterSaveConfirm()
    {
        // 隐藏当前选中的选项文本
        TextMeshProUGUI chosen = Options[CurrentIndex];
        chosen.alpha = 0f;

        // Really 面板（Yes/No）放到该选项的左边 —— 和现有 SaveRecording 方法保持一致
        // confirmPanelOffset 是参考坐标系偏移量，需要乘以 Canvas.scaleFactor 适配不同分辨率
        float sf = GetCanvasScaleFactor();
        Really.transform.position = chosen.transform.position + (Vector3)confirmPanelOffset * sf;
        Really.SetActive(true);

        // 显示描述文本
        if (DescriptionText != null)
            DescriptionText.text = "是否保存回放？";

        YesOrNo = false; // 默认选 No
        UpdateConfirmColor();
        IsReally = true;
    }

    private void ExitSaveConfirm()
    {
        if (Really != null) Really.SetActive(false);

        // 恢复被隐藏的选项
        TextMeshProUGUI chosen = Options[CurrentIndex];
        chosen.alpha = 1f;

        SetDescription(CurrentIndex);
        IsReally = false;
    }

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

    private void ExecuteChosenAction()
    {
        // 确认面板已经可以关掉了
        if (Really != null) Really.SetActive(false);

        //  GameOver OnEnable 加了 AddSkipTickReason，退出前必须 Remove
        //  否则 ReplayManager.LateUpdate 永远 return，下一局 SimClock 不动
        ReplayManager.RemoveSkipTickReason();

        switch (CurrentIndex)
        {
            case 0:
                ContinueGame();
                break;

            case 1:
                Global_SceneManager.Instance.RestartGame();
                //  重置所有暂停/缩放状态 —— 重开游戏从头开始
                TimeScaleController.ResetAll();
                Global_GameManager.Instance.state = State.Gaming;
                gameObject.SetActive(false);
                break;

            case 2:
                // 回收所有敌人
                if (Global_GameManager.Instance != null)
                    Global_GameManager.Instance.RecycleAllEnemies();

                //  重置所有暂停/缩放状态 —— 进入菜单场景
                TimeScaleController.ResetAll();
                Global_SceneManager.Instance.IntoNextScene("GameStartMenu", false);
                gameObject.SetActive(false);
                break;
        }
    }

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

    private void ContinueGame()
    {
        //  重置所有暂停/缩放状态 —— 续关等于重新开始游戏
        TimeScaleController.ResetAll();

        // ✅ CommitSaveOrDiscard 已经处理好了：
        //   - 选保存 → SaveRecording() 序列化死前录像 → recordBuffer 清 → Mode=Idle
        //   - 选丢弃 → DiscardRecording() 清缓存 → Mode=Idle
        // 现在把 Mode 切回 Record：让 LateUpdate 不再 return → SimClock 继续推进（敌人波次、对话框按时间节点走）
        // 绝对不能调 BeginRecord() —— 它会 SimClock.Reset() 把敌人波次时间清零！
        // 也绝对不能 SimClock.Reset() / SimTimer.CancelAll() —— 会清掉已注册的波次定时器
        if (ReplayManager.Instance != null)
        {
            ReplayManager.Instance.CurrentMode = ReplayManager.Mode.Record;
            ReplayManager.Instance.MarkContinued();
        }

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
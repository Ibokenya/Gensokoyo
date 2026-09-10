using TMPro;
using UnityEngine;
using ReplaySystem;

/// <summary>
/// 回放暂停 UI（Playback 模式 Esc 触发 / 回放自然结束也触发）。
///
/// 绑定在 Game1 场景的"回放暂停"UI 根物体上，该物体下应有：
///   - 两个按钮 TextMeshProUGUI（Button1=恢复/ReStart, Button2=返回菜单）
///   - 主界面/背景装饰等（任意）
///
/// 状态：
///   回放进行中 Esc → 打开，Button1 文本="Return To Game"
///   回放自然结束   → 打开，Button1 文本="ReStart"
///
/// 走 meta 层：读 Unity 原始 Input（不经过 ReplayManager.Input），
/// timeScale=0 时也能正常响应。
/// </summary>
public class ReplayPauseUI : MonoBehaviour
{
    [Header("按钮 UI")]
    public TextMeshProUGUI Button1;   // 恢复 / ReStart（根据状态改文本）
    public TextMeshProUGUI Button2;   // 返回菜单（恒定）
    
    [Header("音效")]
    public AudioClip pauseEnterSfx;  // 🔴 进入暂停时播放
    public AudioClip chooseSfx;
    public AudioClip confirmSfx;

    [Header("视觉")]
    public Color selectedColor = new(1f, 1f, 1f, 1f);
    public Color normalColor   = new(0.5f, 0.5f, 0.5f, 0.5f);

    private int selectedIndex = 0;
    private bool isAtEnd = false;  // 回放是否已自然结束

    // ---- 生命周期 ----

    // 🔴 Awake 不再管 SetActive —— UIManager 统一 Show/Hide
    // ReplayManager.Update 触发 ShowReplayPause() → UIManager.SetActive(true) → 本脚本 OnEnable 初始化

    void OnEnable()
    {
        // 判断回放是否已结束
        isAtEnd = ReplayManager.IsPlaybackFinished();

        selectedIndex = 0;
        
        // timeScale=0 让游戏停住（如果还没停）
        if (Time.timeScale > 0f) Time.timeScale = 0f;

        // 🔴 暂停 BGM（AudioSource 不受 timeScale 影响，需要手动 Pause）
        if (Global_AudioManager.Instance != null) Global_AudioManager.Instance.PauseBGM();
        
        // 🔴 暂停回放推进 —— AddSkipTickReason() 让 LateUpdate return early
        // 回放文件位置不前进、SimClock 不推进
        ReplayManager.AddSkipTickReason();
        
        // 🔴 播放进入暂停音效
        PlaySfx(pauseEnterSfx);
        
        // 回放结束时 Button1 显示 ReStart
        if (Button1 != null)
            Button1.text = isAtEnd ? "ReStart" : "Return To Game";

        RefreshSelection();
    }

    void OnDisable()
    {
        // 🔴 Resume / ReStart 后恢复回放推进
        ReplayManager.RemoveSkipTickReason();
    }

    void Update()
    {
        HandleInput();
    }

    // ---- 输入 ----

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            BeChooseCancel(selectedIndex);
            selectedIndex = 0;
            BeChoose(selectedIndex);
            PlaySfx(chooseSfx);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            BeChooseCancel(selectedIndex);
            selectedIndex = 1;
            BeChoose(selectedIndex);
            PlaySfx(chooseSfx);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            PlaySfx(confirmSfx);
            switch (selectedIndex)
            {
                case 0: // Button1：恢复 / ReStart
                    if (isAtEnd)
                    {
                        ReplayManager.ResetGameEndFlag();
                        ReplayManager.RestartPlayback();
                        ReplayManager.NotifyReplayResumed();
                        ReplayManager.UIManagerInstance?.HideReplayPause();
                    }
                    else Resume();
                    break;
                case 1: // Button2：返回菜单
                    ReturnToMenu();
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            // X 键：进行中恢复，结束时无效
            if (!isAtEnd) Resume();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Esc：进行中恢复；结束时返回菜单（不给 ReStart 也有出路）
            if (!isAtEnd) Resume();
            else ReturnToMenu();
        }
    }

    // ---- 动作 ----

    private void Resume()
    {
        Time.timeScale = 1f;
        // 🔴 UnPause BGM
        if (Global_AudioManager.Instance != null) Global_AudioManager.Instance.UnPauseBGM();
        ReplayManager.NotifyReplayResumed();
        ReplayManager.UIManagerInstance?.HideReplayPause();
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        ReplayManager.DiscardRecording();
        ReplayManager.NotifyReplayResumed();
        if (Global_SceneManager.Instance != null)
            Global_SceneManager.Instance.IntoNextScene("GameStartMenu", false);
        ReplayManager.UIManagerInstance?.HideReplayPause();
    }

    // ---- UI 辅助 ----

    private void BeChoose(int i)
    {
        if (i == 0 && Button1 != null) Button1.color = selectedColor;
        if (i == 1 && Button2 != null) Button2.color = selectedColor;
    }

    private void BeChooseCancel(int i)
    {
        if (i == 0 && Button1 != null) Button1.color = normalColor;
        if (i == 1 && Button2 != null) Button2.color = normalColor;
    }

    private void RefreshSelection()
    {
        BeChooseCancel(0); BeChooseCancel(1);
        BeChoose(selectedIndex);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && Global_AudioManager.Instance != null)
            Global_AudioManager.Instance.PlaySFX(clip);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using ReplaySystem;

/// <summary>
/// 历史战绩管理脚本。绑定在 GameStartMenu 场景的"历史战绩"界面物体上。
///
/// 管理最多 10 条回放记录：
///   - 进入界面时从 SavesDir 读文件 → 按保存时间倒序 → 填充 10 条 slots
///   - 不足 10 条的 slot 文本置空
///   - 默认选第 0 条（最新的）
///   - ↑↓ 切换（选中项加粗/高亮），Z 进入回放，Del 删除选中
///   - 删除后自动重排序、默认选第 0 条
///
/// ReplayMenu 是 UI 脚本：读文件、调 ReplayManager.BeginPlayback、调 SceneManager 进 Game1。
/// 回放的 tick 驱动、SimClock 管理、Input 提供都归 ReplayManager。
/// </summary>
public class ReplayMenu : MonoBehaviour
{
    [Header("UI 绑定（10 条，按顺序从第 0 到第 9）")]
    public List<TextMeshProUGUI> slotTexts = new();
    public GameObject replayRoot;
    public GameObject mainMenuRoot;

    [Header("视觉")]
    public FontStyles normalStyle = FontStyles.Normal;
    public FontStyles selectedStyle = FontStyles.Bold;

    [Header("音效")]
    public AudioClip chooseSfx;
    public AudioClip confirmSfx;
    public AudioClip deleteSfx;

    /// <summary>slot 0..9 对应的数据（可能 null 表示无存档）</summary>
    private readonly List<ReplayHeader> slots = new(10);
    private readonly List<string> slotPaths = new(10); // 对应每个 slot 的物理路径（可能 null）
    private int selectedIndex = 0;
    private int skipFrames = 2; // OnEnable 后前 N 帧屏蔽输入，避免菜单切换时的按键边沿被新面板误消费

    // ---- 生命周期 ----

    void OnEnable()
    {
        Reload();
        skipFrames = 2; // 重置冷却
    }

    void Update()
    {
        // 冷却帧期间只递减，不处理输入
        if (skipFrames > 0) { skipFrames--; return; }

        HandleInput();
    }

    // ---- 对外 ----

    /// <summary>重新从磁盘读所有 .rply 文件、排序、填充 UI</summary>
    public void Reload()
    {
        slots.Clear();
        slotPaths.Clear();
        selectedIndex = 0;

        var serializer = new BinaryReplaySerializer();
        var dir = ReplayManager.SavesDir;
        if (Directory.Exists(dir))
        {
            var records = new List<(ReplayHeader h, string path)>();
            foreach (var file in Directory.GetFiles(dir, "*.rply"))
            {
                try
                {
                    var h = serializer.Load(file).Header;
                    records.Add((h, file));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ReplayMenu] 读取回放 {Path.GetFileName(file)} 失败: {e.Message}");
                }
            }
            // 按 saveTimeMs 降序（最新在上）
            records.Sort((a, b) => b.h.saveTimeMs.CompareTo(a.h.saveTimeMs));

            foreach (var r in records.Take(ReplayHeader.MaxRecordSlots))
            {
                slots.Add(r.h);
                slotPaths.Add(r.path);
            }
        }

        // 填充 UI：不足 10 条的 slot 文本置空
        for (int i = 0; i < 10; i++)
        {
            if (i < slots.Count)
            {
                slotTexts[i].text = BuildSlotName(slots[i]);
            }
            else
            {
                slotTexts[i].text = "";
            }
        }

        RefreshSelection();
    }

    // ---- 输入 ----

    private void HandleInput()
    {
        // 🔴 meta 层 UI 全部读 Unity Input，不走 ReplayManager.Input
        // 避免 ReplayInputProvider 的回放边沿或 LiveInputProvider 未清的边沿干扰

        if (Input.GetKeyDown(PhysicalKeyMapping.Escape))
        {
            CloseMenu();
            return;
        }

        if (slots.Count == 0) return; // 空列表时除了退出什么都不做

        if (Input.GetKeyDown(PhysicalKeyMapping.Up))
        {
            if (selectedIndex > 0) selectedIndex--;
            else selectedIndex = slots.Count - 1;
            PlaySfx(chooseSfx);
            RefreshSelection();
        }
        else if (Input.GetKeyDown(PhysicalKeyMapping.Down))
        {
            if (selectedIndex < slots.Count - 1) selectedIndex++;
            else selectedIndex = 0;
            PlaySfx(chooseSfx);
            RefreshSelection();
        }
        else if (Input.GetKeyDown(PhysicalKeyMapping.Z))
        {
            EnterSelectedReplay();
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteSelected();
        }
    }

    // ---- 动作 ----

    private void EnterSelectedReplay()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;
        string path = slotPaths[selectedIndex];
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Reload();
            return;
        }

        PlaySfx(confirmSfx);
        Debug.Log($"[ReplayMenu] 进入回放: {Path.GetFileName(path)}");
        ReplayManager.BeginPlayback(path);

        // 回菜单场景，让 Game1.Awake 接管（BeginPlayback 已 Reset SimClock + Init RNG，Game1.Awake 末尾再 Reset 一次是安全的）
        // 如果是直接在 GameStartMenu 里触发，则进 Game1
        Global_SceneManager.Instance.IntoNextScene("Game1", false);
        gameObject.SetActive(false); // 关闭菜单 UI（或让 SceneManager 处理）
    }

    private void DeleteSelected()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count) return;
        string path = slotPaths[selectedIndex];
        if (string.IsNullOrEmpty(path)) return;

        if (!File.Exists(path))
        {
            Reload();
            return;
        }

        try
        {
            File.Delete(path);
            PlaySfx(deleteSfx);
            Debug.Log($"[ReplayMenu] 删除回放: {Path.GetFileName(path)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ReplayMenu] 删除失败: {e.Message}");
            return;
        }

        // 立即重排序，默认选第 0 条；如果删到 0 条，selectedIndex 保持 0 但 UI 全空
        Reload();
    }

    private void CloseMenu()
    {
        if (ReplayManager.Instance != null && ReplayManager.Instance.CurrentMode == ReplayManager.Mode.Playback)
        {
            ReplayManager.DiscardRecording();
        }
        // 恢复主菜单状态机 + 场景根物体显隐
        if (Global_GameManager.Instance != null)
            Global_GameManager.Instance.state = State.Menu;
        if (replayRoot != null) replayRoot.SetActive(false);
        if (mainMenuRoot != null) mainMenuRoot.SetActive(true);
    }

    // ---- UI ----

    private void RefreshSelection()
    {
        for (int i = 0; i < slotTexts.Count; i++)
        {
            if (slotTexts[i] == null) continue;
            if (i < slots.Count && i == selectedIndex)
            {
                slotTexts[i].fontStyle = selectedStyle;
            }
            else
            {
                slotTexts[i].fontStyle = normalStyle;
            }
        }
    }

    /// <summary>
    /// 拼接存档名：日期 -- 时间 -- 机体 -- 得分 -- 难度 -- Stage X
    /// 例："2026-9-8  --  17:18  --  Reimu  --  12892200  --  Easy  --  Stage 2"
    /// 旧格式文件（saveTimeMs==0）："（旧格式）Reimu 0 Easy Stage 1"
    /// </summary>
    public static string BuildSlotName(ReplayHeader h)
    {
        string charStr   = ((Character)h.character).ToString();
        string scoreStr  = h.score.ToString();                 // 0 就显示 "0"
        string modeStr   = ((GameMode)h.gameMode).ToString();
        string stageStr  = h.stage > 0 ? h.stage.ToString() : "?";

        if (h.saveTimeMs <= 0)
            return $"（旧格式）{charStr} {scoreStr} {modeStr} Stage {stageStr}";

        var dt = h.SaveDateTime;
        string dateStr   = $"{dt.Year}-{dt.Month}-{dt.Day}";   // 2026-9-8 （不补零）
        string timeStr   = $"{dt.Hour:D2}:{dt.Minute:D2}";     // 17:18

        return $"{dateStr}  --  {timeStr}  --  {charStr}  --  {scoreStr}  --  {modeStr}  --  Stage {stageStr}";
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && Global_AudioManager.Instance != null)
            Global_AudioManager.Instance.PlaySFX(clip);
    }
}

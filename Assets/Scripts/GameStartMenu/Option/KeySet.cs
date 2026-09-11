using System.Collections.Generic;
using ReplaySystem;
using TMPro;
using UnityEngine;

/// <summary>
/// 按键设置界面控制器。
///
/// 9 种逻辑键（PhysicalKeyMapping 字段）：
///   上下左右、Shift、Z、X、Ctrl、Escape
///
/// 状态机：
///   Navigation —— Sword 指向当前行
///     ↓ 按 Z → Listening
///     ↓ 按 X → 通知 OptionAnime 关闭面板
///   Listening  —— 等玩家按物理键
///     ↓ 成功绑定 → 保存 + 回到 Navigation（Sword 仍在当前行）
///     ↓ 冲突/非法 → ErrorSound + 保持 Listening
///     ↓ 按 X/Escape → 取消，回到 Navigation
///
/// 🔴 导航输入用 PhysicalKeyMapping 的当前映射（重绑后立即生效）。
///    Listening 态屏蔽导航，只监听"任意物理键"。
/// </summary>
public class KeySet : MonoBehaviour
{
    // ---- Row 定义（一行 = 一个可重绑的逻辑键）----

    [System.Serializable]
    public class KeySetRow
    {
        [Tooltip("AllKeys 里的逻辑键描述名（显示给玩家看的中文）")]
        public string DisplayName;

        [Tooltip("true = 对应 PhysicalKeyMapping.Escape 字段；false = 用下面的 LogicalKey")]
        public bool IsEscapeRow;

        [Tooltip("逻辑键枚举（当 IsEscapeRow=false 时生效）")]
        public LogicalKey LogicalKey;
    }

    // ========= 外部引用（Inspector 里拖）=========

    [Header("UI 引用")]
    public GameObject Sword;                     // 剑型箭头
    public TextMeshProUGUI MapperDesc;          // 状态提示（如"正在绑定[射击]..."）
    public List<GameObject> AllKeys;            // 每行逻辑键描述物体（供 Sword 定位）
    public List<TextMeshProUGUI> AllKeysMapper; // 每行当前绑定的物理键显示（用 KeyCodeDisplayTable 的分行文本）

    [Header("9 种逻辑键（必须与 AllKeys / AllKeysMapper 一一对应）")]
    public List<KeySetRow> Rows = new()
    {
        new KeySetRow { DisplayName = "移动-上",    IsEscapeRow = false, LogicalKey = LogicalKey.Up    },
        new KeySetRow { DisplayName = "移动-下",    IsEscapeRow = false, LogicalKey = LogicalKey.Down  },
        new KeySetRow { DisplayName = "移动-左",    IsEscapeRow = false, LogicalKey = LogicalKey.Left  },
        new KeySetRow { DisplayName = "移动-右",    IsEscapeRow = false, LogicalKey = LogicalKey.Right },
        new KeySetRow { DisplayName = "慢速",       IsEscapeRow = false, LogicalKey = LogicalKey.Shift },
        new KeySetRow { DisplayName = "射击",       IsEscapeRow = false, LogicalKey = LogicalKey.Z     },
        new KeySetRow { DisplayName = "符卡",       IsEscapeRow = false, LogicalKey = LogicalKey.X     },
        new KeySetRow { DisplayName = "对话快进",   IsEscapeRow = false, LogicalKey = LogicalKey.Ctrl  },
        new KeySetRow { DisplayName = "暂停(Esc)", IsEscapeRow = true,  LogicalKey = LogicalKey.Up    }, // 用 Up 占位，实际用 IsEscapeRow 分支
    };

    [Header("音效")]
    [SerializeField] private AudioClip moveSound;   // 上下切换音效
    [SerializeField] private AudioClip clickSound;  // Z 确认音效
    [SerializeField] private AudioClip errorSound;  // 冲突/非法音效
    [SerializeField] private AudioClip successSound;// 绑定成功音效

    // ========= 运行时 =========

    public enum UIState { Navigation, Listening }
    private UIState state = UIState.Navigation;
    private int selectedIndex = 0;
    private OptionAnime parentOption;

    private AudioSource sfxSource;

    void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void OnEnable()
    {
        PhysicalKeyMapping.Load();
        selectedIndex = 0;
        state = UIState.Navigation;
        RefreshMapperTexts();
        UpdateSwordPosition();
        if (MapperDesc != null) MapperDesc.text = "";

        parentOption = GetComponentInParent<OptionAnime>();
        if (parentOption == null) parentOption = FindObjectOfType<OptionAnime>();
    }

    void Update()
    {
        if (state == UIState.Listening) HandleListening();
        else HandleNavigation();
    }

    // ========== Navigation ==========

    private void HandleNavigation()
    {
        if (Input.GetKeyDown(PhysicalKeyMapping.Up))
        {
            PlaySfx(moveSound);
            selectedIndex = (selectedIndex - 1 + Rows.Count) % Rows.Count;
            UpdateSwordPosition();
        }
        if (Input.GetKeyDown(PhysicalKeyMapping.Down))
        {
            PlaySfx(moveSound);
            selectedIndex = (selectedIndex + 1) % Rows.Count;
            UpdateSwordPosition();
        }
        if (Input.GetKeyDown(PhysicalKeyMapping.Z))
        {
            PlaySfx(clickSound);
            state = UIState.Listening;
            if (MapperDesc != null) MapperDesc.text = $"正在绑定 [{Rows[selectedIndex].DisplayName}]";
        }
        // Navigation 态按 X：退出按键设置面板
        if (Input.GetKeyDown(PhysicalKeyMapping.X))
        {
            if (parentOption != null) parentOption.CloseKeySetPanel();
            else gameObject.SetActive(false);
        }
    }

    // ========== Listening ==========

    private void HandleListening()
    {
        // X 或 Escape → 取消本次绑定，回到 Navigation
        if (Input.GetKeyDown(PhysicalKeyMapping.X) || Input.GetKeyDown(PhysicalKeyMapping.Escape))
        {
            state = UIState.Navigation;
            if (MapperDesc != null) MapperDesc.text = "";
            return;
        }

        foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (code == KeyCode.None) continue;
            if (Input.GetKeyDown(code))
            {
                TryBindNewKey(code);
                break;
            }
        }
    }

    private void TryBindNewKey(KeyCode newCode)
    {
        KeySetRow row = Rows[selectedIndex];

        // 1) 禁止绑定的键（None / 鼠标物理键）
        if (!PhysicalKeyMapping.IsBindable(newCode))
        {
            PlaySfx(errorSound);
            Debug.LogWarning($"[KeySet] 按键 {newCode} 不允许绑定（保留给系统）");
            return; // 保持 Listening
        }

        // 2) 冲突检测：新键是否已被其他行占用（含 Escape 行和 LogicalKey 行）
        string conflictDesc = PhysicalKeyMapping.FindConflictDescription(newCode);
        if (conflictDesc != null)
        {
            // 被占用的就是当前行自己 → 没冲突
            bool isSelf = row.IsEscapeRow
                ? PhysicalKeyMapping.Escape == newCode
                : PhysicalKeyMapping.GetBinding(row.LogicalKey) == newCode;

            if (!isSelf)
            {
                PlaySfx(errorSound);
                Debug.LogWarning($"[KeySet] 按键 {newCode} 已绑定到 [{conflictDesc}]，冲突！");
                return;
            }
        }

        // 3) 执行绑定
        bool ok;
        if (row.IsEscapeRow)
        {
            KeyCode old = PhysicalKeyMapping.Escape;
            if (old == newCode) { state = UIState.Navigation; if (MapperDesc != null) MapperDesc.text = ""; return; }
            PhysicalKeyMapping.Escape = newCode;
            PhysicalKeyMapping.Save();
            ok = true;
        }
        else
        {
            ok = PhysicalKeyMapping.TrySetBinding(row.LogicalKey, newCode);
        }

        if (ok)
        {
            PlaySfx(successSound);
            Debug.Log($"[KeySet] [{row.DisplayName}] 绑定成功 → {newCode}");
            RefreshMapperTexts();
            if (MapperDesc != null) MapperDesc.text = "";
            state = UIState.Navigation;
            // 🔴 Sword 保持在当前行，玩家继续上下选其他行改
        }
        else
        {
            PlaySfx(errorSound);
            Debug.LogWarning($"[KeySet] 绑定 {row.DisplayName} → {newCode} 失败");
        }
    }

    // ========== UI 刷新 ==========

    private void UpdateSwordPosition()
    {
        if (Sword == null || AllKeys == null || selectedIndex >= AllKeys.Count) return;
        Transform target = AllKeys[selectedIndex].transform;
        Vector3 p = target.localPosition;
        Sword.transform.localPosition = new Vector3(p.x + 820f, p.y + 55f, Sword.transform.localPosition.z);
    }

    private void RefreshMapperTexts()
    {
        if (AllKeysMapper == null) return;
        for (int i = 0; i < Rows.Count && i < AllKeysMapper.Count; i++)
        {
            KeyCode code = Rows[i].IsEscapeRow
                ? PhysicalKeyMapping.Escape
                : PhysicalKeyMapping.GetBinding(Rows[i].LogicalKey);

            AllKeysMapper[i].text = KeyCodeDisplayTable.GetDisplayText(code);
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (Global_AudioManager.Instance != null)
            Global_AudioManager.Instance.PlaySFX(clip, false);
        else if (sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }
}

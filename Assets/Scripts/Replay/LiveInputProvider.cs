using System.Collections.Generic;
using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 物理键 → 逻辑键的映射表。
    ///
    /// 设计：
    ///   - 所有字段默认值保持原硬编码（保证老玩家不受影响）。
    ///   - Save() 把当前映射序列化到 PlayerPrefs（key 前缀 "KB_"）。
    ///   - Load() 从 PlayerPrefs 读回覆盖字段值；若无存过则保持默认。
    ///   - TrySetBinding() 提供统一重绑入口，含冲突检测 + 持久化。
    ///
    /// 为什么 LiveInputProvider 不需要改？
    ///   LiveInputProvider.SampleFromUnity() 每帧调 Input.GetKey(PhysicalKeyMapping.Up) 等，
    ///   所以 PhysicalKeyMapping 的静态字段一变，采样就立即反映新映射。
    ///   ButtonManager / ButtonEvent 等也直接用 Input.GetKey(KeyCode.Z)—但它们只在菜单场景，
    ///   菜单逻辑用固定 KeyCode（Z=确定、X=返回），不走重绑。
    /// </summary>
    public static class PhysicalKeyMapping
    {
        // ---- 默认值 ----
        public static KeyCode Up      = KeyCode.UpArrow;
        public static KeyCode Down    = KeyCode.DownArrow;
        public static KeyCode Left    = KeyCode.LeftArrow;
        public static KeyCode Right   = KeyCode.RightArrow;
        public static KeyCode Shift   = KeyCode.LeftShift;
        public static KeyCode Z       = KeyCode.Z;
        public static KeyCode X       = KeyCode.X;
        public static KeyCode Escape  = KeyCode.Escape;
        public static KeyCode Ctrl    = KeyCode.LeftControl;

        private const string Prefix = "KB_";

        // ---- 重绑入口 ----

        /// <summary>
        /// 尝试把逻辑键绑定到新的物理键。
        /// </summary>
        /// <param name="logical">要改的逻辑键（Up/Down/Left/Right/Shift/Z/X/Ctrl）</param>
        /// <param name="newCode">新的物理键码</param>
        /// <returns>true=成功；false=冲突或无效</returns>
        public static bool TrySetBinding(LogicalKey logical, KeyCode newCode)
        {
            // 1) 过滤不可绑定的键
            if (!IsBindable(newCode)) return false;

            // 2) 冲突检测：新键是否已被其他逻辑键占用
            LogicalKey? conflict = FindConflictingLogicalKey(newCode);
            if (conflict.HasValue && conflict.Value != logical) return false;

            // 3) 写入
            KeyCode oldCode = GetBinding(logical);
            if (oldCode == newCode) return true; // 没变化也算成功，不写 PlayerPrefs

            SetBindingField(logical, newCode);
            Save();
            return true;
        }

        /// <summary>
        /// 查询物理键是否已被某个逻辑键占用。返回 null 表示空闲。
        /// 🔴 Escape 单独字段存，不在 LogicalKey 枚举里，所以 FindConflictingLogicalKey 返回 null；
        ///    冲突检测要在外层由 KeySet 结合 Escape 字段判断。
        /// </summary>
        public static LogicalKey? FindConflictingLogicalKey(KeyCode code)
        {
            if (Up     == code) return LogicalKey.Up;
            if (Down   == code) return LogicalKey.Down;
            if (Left   == code) return LogicalKey.Left;
            if (Right  == code) return LogicalKey.Right;
            if (Shift  == code) return LogicalKey.Shift;
            if (Z      == code) return LogicalKey.Z;
            if (X      == code) return LogicalKey.X;
            if (Ctrl   == code) return LogicalKey.Ctrl;
            return null;
        }

        /// <summary>判断 KeyCode 是否已被 Escape 字段占用</summary>
        public static bool IsCodeUsedByEscape(KeyCode code) => Escape == code;

        /// <summary>判断 KeyCode 是否被任何逻辑键（含 Escape）占用。返回被占用的逻辑键描述</summary>
        public static string FindConflictDescription(KeyCode code)
        {
            if (Escape == code) return "Escape（暂停键）";
            var c = FindConflictingLogicalKey(code);
            if (c.HasValue) return LogicalKeyToDisplayName(c.Value);
            return null;
        }

        /// <summary>
        /// 禁止绑定的键：
        ///   - None
        ///   - 鼠标物理键（手柄键允许绑定）
        ///   - Delete（ReplayMenu 删除存档）
        ///   - R（Game2 返回菜单）
        /// Escape 是可绑定的逻辑键，不在这里过滤。
        /// </summary>
        public static bool IsBindable(KeyCode code)
        {
            if (code == KeyCode.None) return false;
            if (code == KeyCode.Delete) return false;
            if (code == KeyCode.R) return false;
            if (code == KeyCode.Mouse0 || code == KeyCode.Mouse1 || code == KeyCode.Mouse2) return false;
            if (code == KeyCode.Mouse3 || code == KeyCode.Mouse4 || code == KeyCode.Mouse5 || code == KeyCode.Mouse6) return false;
            return true;
        }

        public static KeyCode GetBinding(LogicalKey key)
        {
            return key switch
            {
                LogicalKey.Up    => Up,
                LogicalKey.Down  => Down,
                LogicalKey.Left  => Left,
                LogicalKey.Right => Right,
                LogicalKey.Shift => Shift,
                LogicalKey.Z     => Z,
                LogicalKey.X     => X,
                LogicalKey.Ctrl  => Ctrl,
                _ => KeyCode.None
            };
        }

        public static void ResetToDefaults()
        {
            Up    = KeyCode.UpArrow;
            Down  = KeyCode.DownArrow;
            Left  = KeyCode.LeftArrow;
            Right = KeyCode.RightArrow;
            Shift = KeyCode.LeftShift;
            Z     = KeyCode.Z;
            X     = KeyCode.X;
            Escape= KeyCode.Escape;
            Ctrl  = KeyCode.LeftControl;
            Save();
        }

        // ---- 持久化 ----

        public static void Save()
        {
            PlayerPrefs.SetInt(Prefix + "Up",     (int)Up);
            PlayerPrefs.SetInt(Prefix + "Down",   (int)Down);
            PlayerPrefs.SetInt(Prefix + "Left",   (int)Left);
            PlayerPrefs.SetInt(Prefix + "Right",  (int)Right);
            PlayerPrefs.SetInt(Prefix + "Shift",  (int)Shift);
            PlayerPrefs.SetInt(Prefix + "Z",      (int)Z);
            PlayerPrefs.SetInt(Prefix + "X",      (int)X);
            PlayerPrefs.SetInt(Prefix + "Escape", (int)Escape);
            PlayerPrefs.SetInt(Prefix + "Ctrl",   (int)Ctrl);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 启动时调用：从 PlayerPrefs 加载已保存的映射。
        /// 如果某键没有存过，保持默认值（由 static 字段初始化器给定）。
        /// </summary>
        public static void Load()
        {
            if (PlayerPrefs.HasKey(Prefix + "Up"))     Up     = (KeyCode)PlayerPrefs.GetInt(Prefix + "Up");
            if (PlayerPrefs.HasKey(Prefix + "Down"))   Down   = (KeyCode)PlayerPrefs.GetInt(Prefix + "Down");
            if (PlayerPrefs.HasKey(Prefix + "Left"))   Left   = (KeyCode)PlayerPrefs.GetInt(Prefix + "Left");
            if (PlayerPrefs.HasKey(Prefix + "Right"))  Right  = (KeyCode)PlayerPrefs.GetInt(Prefix + "Right");
            if (PlayerPrefs.HasKey(Prefix + "Shift"))  Shift  = (KeyCode)PlayerPrefs.GetInt(Prefix + "Shift");
            if (PlayerPrefs.HasKey(Prefix + "Z"))      Z      = (KeyCode)PlayerPrefs.GetInt(Prefix + "Z");
            if (PlayerPrefs.HasKey(Prefix + "X"))      X      = (KeyCode)PlayerPrefs.GetInt(Prefix + "X");
            if (PlayerPrefs.HasKey(Prefix + "Escape")) Escape = (KeyCode)PlayerPrefs.GetInt(Prefix + "Escape");
            if (PlayerPrefs.HasKey(Prefix + "Ctrl"))   Ctrl   = (KeyCode)PlayerPrefs.GetInt(Prefix + "Ctrl");
        }

        // ---- 辅助 ----

        private static void SetBindingField(LogicalKey key, KeyCode code)
        {
            switch (key)
            {
                case LogicalKey.Up:    Up    = code; break;
                case LogicalKey.Down:  Down  = code; break;
                case LogicalKey.Left:  Left  = code; break;
                case LogicalKey.Right: Right = code; break;
                case LogicalKey.Shift: Shift = code; break;
                case LogicalKey.Z:     Z     = code; break;
                case LogicalKey.X:     X     = code; break;
                case LogicalKey.Ctrl:  Ctrl  = code; break;
            }
        }

        /// <summary>逻辑键名显示（给 AllKeys 的描述用）。Escape 单独字段，不走此枚举。</summary>
        public static string LogicalKeyToDisplayName(LogicalKey key)
        {
            return key switch
            {
                LogicalKey.Up    => "移动-上",
                LogicalKey.Down  => "移动-下",
                LogicalKey.Left  => "移动-左",
                LogicalKey.Right => "移动-右",
                LogicalKey.Shift => "慢速",
                LogicalKey.Z     => "射击",
                LogicalKey.X     => "符卡",
                LogicalKey.Ctrl  => "对话快进",
                _ => key.ToString()
            };
        }
    }

    /// <summary>
    /// 从 Unity Input 采样 → 映射到逻辑键
    ///
    /// 🔴 覆盖式边沿模型（核心设计）：
    ///   SampleFromUnity() 每帧 Update 调用，用 **=`（覆盖）而非 |=`（累加）** 计算 edgesDown/Up。
    ///   边沿只在按键状态变化的那一帧存在，没变化时 edgesDown/Up 自动 =0。
    ///
    ///   这意味着：
    ///     - 🔴 **不需要 ConsumeEdges！** 覆盖式保证边沿不会跨帧泄漏
    ///     - 同一帧内：ReplayManager.Update（先 SampleFromUnity）→ 玩法脚本 Update（后读 GetKeyDown）→ 完美
    ///     - 跨帧（FixedUpdate 50Hz vs Update 60Hz）：edgesDown 从 SampleFromUnity 算出后，
    ///       存活到下一次 SampleFromUnity 覆盖 —— 足够让下一帧的 FixedUpdate 读到
    ///     - 决死期间（timeScale=0）：FixedUpdate 停但 Update 继续跑 → edgesDown 正常算 →
    ///       SpellCardEffect.Update 能正常读 GetKeyDown(X) ✓
    ///       （GunAnime.CheckUpdate 在 FixedUpdate，所以决死期间 Shift 慢速切换不触发，但这没问题）
    ///
    ///   录制时每 tick 只写 HeldMask 低 8 位（Ctrl 不存），回放方自行比较前后 tick 算边沿。
    /// </summary>
    public class LiveInputProvider : IInputProvider
    {
        private ushort currentHeld;
        private ushort previousHeld;
        private ushort edgesDown;
        private ushort edgesUp;

        public ushort HeldMask => currentHeld;

        /// <summary>🔴 录制时由 PanDing.OnTriggerEnter2D 调这个，把 bit7 设为 1。
        /// 这个标志跨 FixedUpdate → Update → LateUpdate 保留，直到 LateUpdate.recordBuffer.Add 后才清。</summary>
        public void MarkHitThisTick() => currentHeld |= LogicalKeyMask.HitFlag;

        /// <summary>🔴 LateUpdate 的 recordBuffer.Add 之后调，清掉本帧累积的 HitFlag</summary>
        public void ClearHitFlag() => currentHeld &= 0xFF7F; // ~bit7 等价：0b1111111011111111

        /// <summary>
        /// 🔴 覆盖式采样 —— 每帧 Update 调用。
        /// 用 = 覆盖 edgesDown/Up，不用 |= 累加。
        /// HitFlag 不在此处清零——它由 FixedUpdate 的 OnTriggerEnter2D 设置，跨 Update 保留到 LateUpdate 写文件后清。
        /// </summary>
        public void SampleFromUnity()
        {
            previousHeld = currentHeld;
            // 🔴 保留 HitFlag（由 PanDing 设，跨 FixedUpdate→Update→LateUpdate），只采样输入键位
            ushort hit = (ushort)(currentHeld & LogicalKeyMask.HitFlag);
            currentHeld = hit; // 先只留 HitFlag
            if (Input.GetKey(PhysicalKeyMapping.Up))     currentHeld |= LogicalKeyMask.Up;
            if (Input.GetKey(PhysicalKeyMapping.Down))   currentHeld |= LogicalKeyMask.Down;
            if (Input.GetKey(PhysicalKeyMapping.Left))   currentHeld |= LogicalKeyMask.Left;
            if (Input.GetKey(PhysicalKeyMapping.Right))  currentHeld |= LogicalKeyMask.Right;
            if (Input.GetKey(PhysicalKeyMapping.Shift))   currentHeld |= LogicalKeyMask.Shift;
            if (Input.GetKey(PhysicalKeyMapping.Z))   currentHeld |= LogicalKeyMask.Z;
            if (Input.GetKey(PhysicalKeyMapping.X))  currentHeld |= LogicalKeyMask.X;
            // 🔴 Esc 不再采样 —— UI 层直接读 UnityEngine.Input
            if (Input.GetKey(PhysicalKeyMapping.Ctrl))    currentHeld |= LogicalKeyMask.Ctrl;

            // 覆盖式边沿
            ushort xor = (ushort)(currentHeld ^ previousHeld);
            edgesDown = (ushort)(xor & currentHeld);
            edgesUp   = (ushort)(xor & ~currentHeld);
        }

        /// <summary>
        /// 🔴 空壳！完全不需要 ConsumeEdges。
        /// 覆盖式 SampleFromUnity 自动保证边沿不会跨帧泄漏。
        /// 保留这个方法只是为了 IInputProvider 接口兼容 + ReplayManager 初始化时清残留。
        /// </summary>
        public void ConsumeEdges()
        {
            edgesDown = 0;
            edgesUp   = 0;
        }

        public bool GetKey     (LogicalKey key) => LogicalKeyMask.Has(currentHeld, key);
        public bool GetKeyDown (LogicalKey key) => LogicalKeyMask.Has(edgesDown, key);
        public bool GetKeyUp   (LogicalKey key) => LogicalKeyMask.Has(edgesUp,   key);
    }

    /// <summary>
    /// 回放输入源：每 tick 从数组读取 held 掩码（低 8 位），边沿由相邻 tick 差分计算。
    /// Ctrl（第 8 位）回放期间不存文件，通过 ForceCtrlHeld 属性在 dialog 期间强制 held。
    /// </summary>
    public class ReplayInputProvider : IInputProvider
    {
        private ushort currentHeld;
        private ushort previousHeld;
        private int  tickIndex;
        private byte[] tickMasks;

        public ushort HeldMask => currentHeld;
        public bool IsFinished => tickIndex >= tickMasks.Length - 1;

        /// <summary>🔴 回放 dialog 期间设 true → GetKey(Ctrl) 永远返回 true，让 AboutDialog 自动快进</summary>
        public bool ForceCtrlHeld { get; set; }

        public ReplayInputProvider(byte[] masks)
        {
            tickMasks   = masks;
            tickIndex   = -1;  // 🔴 从 -1 开始——第一帧 Update AdvanceTick 会变成 0
            previousHeld = 0;
            currentHeld = 0;   // 🔴 初始 held = 0，等 AdvanceTick 到 masks[0]
        }

        /// <summary>每帧 Update 开头调用：前进到下一帧掩码（帧时序对齐 FixedUpdate 读到上一帧推进的 held）</summary>
        public void AdvanceTick()
        {
            previousHeld = currentHeld;
            if (tickIndex < tickMasks.Length - 1)
            {
                tickIndex++;
                currentHeld = tickMasks[tickIndex]; // 读的是低 8 位
            }
        }

        public bool GetKey(LogicalKey key)
        {
            // 🔴 Ctrl 特殊处理：回放期间强制 held（不管文件里有没有）
            if (key == LogicalKey.Ctrl && ForceCtrlHeld) return true;
            return LogicalKeyMask.Has(currentHeld, key);
        }

        public bool GetKeyDown(LogicalKey key)
        {
            // Ctrl 上升沿：上一 tick 未 held 本 tick ForceCtrl → 产生一次上升沿（用于触发对话快进）
            if (key == LogicalKey.Ctrl && ForceCtrlHeld)
            {
                bool prevWasHeld = LogicalKeyMask.Has(previousHeld, LogicalKey.Ctrl);
                return !prevWasHeld; // 上一 tick 没 ForceCtrl → 本 tick ForceCtrl → 产生一次 down
            }
            return LogicalKeyMask.Has((ushort)(currentHeld & ~previousHeld), key);
        }

        public bool GetKeyUp(LogicalKey key)
        {
            // Ctrl 下降沿：上一 tick ForceCtrl 但本 tick 不再 ForceCtrl
            if (key == LogicalKey.Ctrl)
            {
                bool prevWasForce = LogicalKeyMask.Has(previousHeld, LogicalKey.Ctrl);
                // 回放时 currentHeld 不会带 Ctrl 位，所以只要 ForceCtrl 关掉就算 up
                if (!ForceCtrlHeld && prevWasForce) return true;
            }
            return LogicalKeyMask.Has((ushort)(~currentHeld & previousHeld), key);
        }
    }
}

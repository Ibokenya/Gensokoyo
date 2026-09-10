using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 物理键→逻辑键的默认映射。硬编码在这里，后期可抽成 ScriptableObject 或 JSON 供玩家自定义。
    /// </summary>
    public static class PhysicalKeyMapping
    {
        public static KeyCode Up     = KeyCode.UpArrow;
        public static KeyCode Down   = KeyCode.DownArrow;
        public static KeyCode Left   = KeyCode.LeftArrow;
        public static KeyCode Right  = KeyCode.RightArrow;
        public static KeyCode Shift   = KeyCode.LeftShift;
        public static KeyCode Z   = KeyCode.Z;
        public static KeyCode X  = KeyCode.X;
        public static KeyCode Escape = KeyCode.Escape;
        public static KeyCode Ctrl = KeyCode.LeftControl; // 对话快进（回放不存，回放 dialog 期间强制 held）
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
        private ushort currentHeld;   // 当前帧 held（Update 末写入，ushort 容纳 Ctrl=第 8 位）
        private ushort previousHeld;  // 上一帧 held（Update 末保留，下一帧 Sample 时对比）
        private ushort edgesDown;     // GetKeyDown（覆盖式：只在按键按下那一帧非零）
        private ushort edgesUp;       // GetKeyUp（覆盖式：只在按键松开那一帧非零）

        public ushort HeldMask => currentHeld;

        /// <summary>
        /// 🔴 覆盖式采样 —— 每帧 Update 调用。
        /// 用 = 覆盖 edgesDown/Up，不用 |= 累加 —— 这样边沿只在按键变化的那一帧存在。
        /// </summary>
        public void SampleFromUnity()
        {
            previousHeld = currentHeld;
            currentHeld = 0;

            if (Input.GetKey(PhysicalKeyMapping.Up))     currentHeld |= LogicalKeyMask.Up;
            if (Input.GetKey(PhysicalKeyMapping.Down))   currentHeld |= LogicalKeyMask.Down;
            if (Input.GetKey(PhysicalKeyMapping.Left))   currentHeld |= LogicalKeyMask.Left;
            if (Input.GetKey(PhysicalKeyMapping.Right))  currentHeld |= LogicalKeyMask.Right;
            if (Input.GetKey(PhysicalKeyMapping.Shift))   currentHeld |= LogicalKeyMask.Shift;
            if (Input.GetKey(PhysicalKeyMapping.Z))   currentHeld |= LogicalKeyMask.Z;
            if (Input.GetKey(PhysicalKeyMapping.X))  currentHeld |= LogicalKeyMask.X;
            if (Input.GetKey(PhysicalKeyMapping.Escape)) currentHeld |= LogicalKeyMask.Escape;
            if (Input.GetKey(PhysicalKeyMapping.Ctrl))    currentHeld |= LogicalKeyMask.Ctrl;

            // 🔴 覆盖式边沿 —— = 不是 |=
            ushort xor = (ushort)(currentHeld ^ previousHeld);
            edgesDown = (ushort)(xor & currentHeld);    // 上升沿：本帧 held 上一帧没 held
            edgesUp   = (ushort)(xor & ~currentHeld);   // 下降沿：本帧没 held 上一帧 held
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

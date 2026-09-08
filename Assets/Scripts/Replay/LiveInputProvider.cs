using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 物理键→逻辑键的默认映射。硬编码在这里，后期可抽成 ScriptableObject 或 JSON 供玩家自定义。
    /// 注：映射表使用 partial + default 实现，后续可轻松替换成配置驱动。
    /// </summary>
    public static class PhysicalKeyMapping
    {
        public static KeyCode Up     = KeyCode.UpArrow;
        public static KeyCode Down   = KeyCode.DownArrow;
        public static KeyCode Left   = KeyCode.LeftArrow;
        public static KeyCode Right  = KeyCode.RightArrow;
        public static KeyCode Slow   = KeyCode.LeftShift;
        public static KeyCode Fire   = KeyCode.Z;
        public static KeyCode Spell  = KeyCode.X;
        public static KeyCode Cancel = KeyCode.Escape;
    }

    /// <summary>
    /// 从 Unity Input 采样 → 映射到逻辑键
    ///
    /// 采样/边沿模型：
    ///   1) Update 中调用 SampleFromUnity()，与上一帧 held 比较计算 down/up 边沿，累加到锁存器
    ///   2) FixedUpdate 开头玩法逻辑读取 GetKey/GetKeyDown/GetKeyUp（边沿在同一次 FixedUpdate 运行期间对所有组件可见）
    ///   3) FixedUpdate 末尾调用 ConsumeEdges() 清零，准备下一帧采样
    ///
    /// 这样极短的点按（<20ms，约一帧内按下再松开）在相邻两次 Update 之间仍能被完整捕获。
    /// 录制时每 tick 只写 HeldMask（8-bit held 状态），回放方自行比较前后 tick 算边沿——文件最小。
    /// </summary>
    public class LiveInputProvider : IInputProvider
    {
        private byte currentHeld;   // 当前帧 held（Update 末写入）
        private byte previousHeld;  // 上一帧 held（Update 末保留）
        private byte edgesDown;     // GetKeyDown 锁存（FixedUpdate 消费后清零）
        private byte edgesUp;       // GetKeyUp   锁存（FixedUpdate 消费后清零）

        public byte HeldMask => currentHeld;

        /// <summary>每帧 Update 末调用</summary>
        public void SampleFromUnity()
        {
            previousHeld = currentHeld;
            currentHeld = 0;

            if (Input.GetKey(PhysicalKeyMapping.Up))     currentHeld |= LogicalKeyMask.Up;
            if (Input.GetKey(PhysicalKeyMapping.Down))   currentHeld |= LogicalKeyMask.Down;
            if (Input.GetKey(PhysicalKeyMapping.Left))   currentHeld |= LogicalKeyMask.Left;
            if (Input.GetKey(PhysicalKeyMapping.Right))  currentHeld |= LogicalKeyMask.Right;
            if (Input.GetKey(PhysicalKeyMapping.Slow))   currentHeld |= LogicalKeyMask.Slow;
            if (Input.GetKey(PhysicalKeyMapping.Fire))   currentHeld |= LogicalKeyMask.Fire;
            if (Input.GetKey(PhysicalKeyMapping.Spell))  currentHeld |= LogicalKeyMask.Spell;
            if (Input.GetKey(PhysicalKeyMapping.Cancel)) currentHeld |= LogicalKeyMask.Cancel;

            // 边沿 = 当前异或上一帧，然后从异或里过滤出上升/下降
            byte xor = (byte)(currentHeld ^ previousHeld);
            edgesDown |= (byte)(xor & currentHeld);  // 上升沿
            edgesUp   |= (byte)(xor & ~currentHeld);  // 下降沿
        }

        /// <summary>每 tick FixedUpdate 运行完所有玩法逻辑后调用，消费锁存的边沿</summary>
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
    /// 回放输入源：每 tick 从数组读取 held 掩码
    /// 边沿由相邻 tick 的 held 差分计算，不需要额外锁存
    /// </summary>
    public class ReplayInputProvider : IInputProvider
    {
        private byte currentHeld;
        private byte previousHeld;
        private int  tickIndex;
        private byte[] tickMasks;

        public byte HeldMask => currentHeld;
        public bool IsFinished => tickIndex >= tickMasks.Length - 1;

        public ReplayInputProvider(byte[] masks)
        {
            tickMasks   = masks;
            tickIndex   = 0;
            previousHeld = 0;
            currentHeld = masks.Length > 0 ? masks[0] : (byte)0;
        }

        /// <summary>每 tick FixedUpdate 开头调用，前进到下一帧掩码</summary>
        public void AdvanceTick()
        {
            previousHeld = currentHeld;
            if (tickIndex < tickMasks.Length - 1)
            {
                tickIndex++;
                currentHeld = tickMasks[tickIndex];
            }
            else
            {
                // 文件读完：保持最后一帧 held 不变
            }
        }

        public bool GetKey     (LogicalKey key) => LogicalKeyMask.Has(currentHeld, key);
        public bool GetKeyDown (LogicalKey key) => LogicalKeyMask.Has((byte)(currentHeld & ~previousHeld), key);
        public bool GetKeyUp   (LogicalKey key) => LogicalKeyMask.Has((byte)(~currentHeld & previousHeld), key);
    }
}

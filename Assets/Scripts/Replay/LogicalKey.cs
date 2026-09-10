using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 逻辑键位枚举。固定 8 位掩码位序，与玩家物理按键配置解耦。
    /// LiveInputProvider 通过硬编码映射（或后期 JSON 配置）把 Unity KeyCode 映射到这些逻辑位。
    /// </summary>
    public enum LogicalKey : byte
    {
        Up    = 0, // 上
        Down  = 1, // 下
        Left  = 2, // 左
        Right = 3, // 右
        Shift  = 4, // 慢速（Shift）
        Z  = 5, // 射击（Z）
        X = 6, // 符卡（X）
        Escape= 7  // 取消/暂停（Esc）
    }

    /// <summary>掩码位辅助，按 LogicalKey 顺序编码的 8-bit 状态</summary>
    public static class LogicalKeyMask
    {
        public const byte Up     = 1 << 0;
        public const byte Down   = 1 << 1;
        public const byte Left   = 1 << 2;
        public const byte Right  = 1 << 3;
        public const byte Shift   = 1 << 4;
        public const byte Z   = 1 << 5;
        public const byte X  = 1 << 6;
        public const byte Escape = 1 << 7;

        public static bool Has(byte mask, LogicalKey key)
            => (mask & (byte)(1 << (int)key)) != 0;

        public static byte Set(byte mask, LogicalKey key)
            => (byte)(mask | (byte)(1 << (int)key));

        public static byte Clear(byte mask, LogicalKey key)
            => (byte)(mask & ~(byte)(1 << (int)key));
    }

    /// <summary>
    /// 模拟输入源接口。所有玩法脚本应改读此接口，不再直接调 Input.*
    /// 实现：LiveInputProvider（从 Unity Input 采样）/ ReplayInputProvider（从回放文件读）
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>当前 tick 持有的按键掩码（8-bit）</summary>
        byte HeldMask { get; }

        bool GetKey     (LogicalKey key);
        bool GetKeyDown (LogicalKey key);
        bool GetKeyUp   (LogicalKey key);
    }
}

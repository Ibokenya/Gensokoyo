using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 逻辑键位枚举。原 8 位（Up/Down/Left/Right/Shift/Z/X/Escape），新增 Ctrl=8 位。
    /// 回放文件格式仍保持每 tick 1 字节（低 8 位），Ctrl 不存文件（回放时 dialog 期间强制 held）。
    /// </summary>
    public enum LogicalKey : byte
    {
        Up     = 0, // 上
        Down   = 1, // 下
        Left   = 2, // 左
        Right  = 3, // 右
        Shift  = 4, // 慢速（Shift）
        Z      = 5, // 射击（Z）
        X      = 6, // 符卡（X）
        Escape = 7, // 取消/暂停（Esc）
        Ctrl   = 8  // 对话快进（左 Ctrl）—— 不存回放文件，回放 dialog 期间强制 held
    }

    /// <summary>掩码位辅助。ushort 足够存 Ctrl=第 9 位，回放文件只存低 8 位。</summary>
    public static class LogicalKeyMask
    {
        public const ushort Up     = 1 << 0;
        public const ushort Down   = 1 << 1;
        public const ushort Left   = 1 << 2;
        public const ushort Right  = 1 << 3;
        public const ushort Shift  = 1 << 4;
        public const ushort Z      = 1 << 5;
        public const ushort X      = 1 << 6;
        public const ushort Escape = 1 << 7;
        public const ushort Ctrl   = 1 << 8;

        /// <summary>只取低 8 位（回放文件存储用）</summary>
        public static byte LowByte(ushort mask) => (byte)(mask & 0xFF);

        public static bool Has(ushort mask, LogicalKey key)
            => (mask & (ushort)(1 << (int)key)) != 0;

        public static ushort Set(ushort mask, LogicalKey key)
            => (ushort)(mask | (ushort)(1 << (int)key));

        public static ushort Clear(ushort mask, LogicalKey key)
            => (ushort)(mask & ~(ushort)(1 << (int)key));
    }

    /// <summary>
    /// 模拟输入源接口。所有玩法脚本应改读此接口，不再直接调 Input.*
    /// 实现：LiveInputProvider（从 Unity Input 采样）/ ReplayInputProvider（从回放文件读）
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>当前 tick 持有的按键掩码（ushort，Ctrl=第 8 位）</summary>
        ushort HeldMask { get; }

        bool GetKey     (LogicalKey key);
        bool GetKeyDown (LogicalKey key);
        bool GetKeyUp   (LogicalKey key);
    }
}

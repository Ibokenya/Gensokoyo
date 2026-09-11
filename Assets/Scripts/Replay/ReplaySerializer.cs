using System.IO;
using UnityEngine;
using System;

namespace ReplaySystem
{
    /// <summary>
    /// 回放文件头（二进制紧凑）。
    ///
    /// 布局（112 字节，HeaderSize 固定）：
    ///   [0..3]   magic       : 0x594C5052 "RPLY"
    ///   [4..7]   version     : int32（2 = 当前版本）
    ///   [8..15]  seed        : uint64
    ///   [16..19] character   : Character enum
    ///   [20..23] gameMode    : GameMode enum
    ///   [24..27] tickRate    : 固定 50
    ///   [28..31] tickCount   : int32 tick 总数
    ///   [32]     metaReserved: 0
    ///   [33..36] score       : int32 最终得分（从预留区启用）
    ///   [37..40] stage       : int32 最终关卡（从预留区启用）
    ///   [41..48] saveTimeMs  : int64 Unix 毫秒时间戳（保存时刻，从预留区启用）
    ///   [49..111] reserved   : 63 字节继续预留
    ///   [112..]  tick body   : tickCount 字节 HeldMask
    ///
    /// Load 对旧版本文件（version=1，无 score/stage/saveTime）友好：
    /// score/stage/saveTime 默认 0，调用方用 score==0 判断是否有得分信息。
    /// </summary>
    public struct ReplayHeader
    {
        public const int  Magic           = 0x594C5052; // "RPLY"（小端）
        public const int  CurrentVersion  = 3; // v3 = 带校验帧位置表 + HitFlag 位（原 Esc 位换成中弹标志）
        public const int  HeaderSize      = 112;
        public const int  MaxRecordSlots = 10; // 历史战绩最多保留 10 条

        // 校验帧参数（v3 新增）：每 validateInterval tick 存一次玩家位置
        public const int ValidateInterval = 60;
        public const int ValidateEntrySize = 8; // 2 个 float (posX, posY)

        public int      magic;
        public int      version;
        public ulong    seed;
        public int      character;
        public int      gameMode;
        public int      tickRate;
        public int      tickCount;
        public byte     metaReserved;

        // 从旧预留区启用的元信息（version>=2 有效，version=1 默认为 0）
        public int      score;
        public int      stage;
        public long     saveTimeMs; // Unix epoch 毫秒（用于按时间排序）

        // v3 新增：校验帧数量 = ceil(tickCount / ValidateInterval)
        public int ValidateFrameCount => tickCount > 0 ? (tickCount + ValidateInterval - 1) / ValidateInterval : 0;

        /// <summary>saveTimeMs → DateTime（本地时区）。
        /// 用 .NET 内置 DateTimeOffset.FromUnixTimeMilliseconds，正确处理 epoch 偏移。
        /// 之前手动 ×10000 但漏加 epoch 偏移，导致年份变成 0001 后加 560 年 → 显示 0561。</summary>
        public System.DateTime SaveDateTime
        {
            get
            {
                if (saveTimeMs <= 0) return default;
                return DateTimeOffset.FromUnixTimeMilliseconds(saveTimeMs).LocalDateTime;
            }
        }
    }

    /// <summary>回放文件的内存表示：头 + tick 数组 + 校验帧位置表
    /// v3 新增：校验帧位置表（每 ValidateInterval tick 的玩家位置 + 游戏状态）
    /// v3 更改：回放文件每 tick 1 字节 bit7 从 Esc 换成 HitFlag
    /// v3.1：校验帧扩展为 [x, y, state] = 10 bytes（4+4+2）</summary>
    public struct ReplayFile
    {
        public ReplayHeader Header;
        public byte[] TickMasks;
        public float[] ValidatePositions;   // [x0,y0,x1,y1,...]
        public ushort[] ValidateStates;    // [state0, state1, ...] 和 ValidatePositions 并行
    }

    public interface IReplaySerializer
    {
        void Save(ReplayFile file, string path);
        ReplayFile Load(string path);
    }

    /// <summary>二进制紧凑序列化（正式）</summary>
    public class BinaryReplaySerializer : IReplaySerializer
    {
        public void Save(ReplayFile file, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var fs = File.Open(path, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            var h = file.Header;
            h.version = 3; // v3 = 校验帧位置表 + HitFlag 位（bit7 原 Esc 位）
            bw.Write(h.magic);
            bw.Write(h.version);
            bw.Write(h.seed);
            bw.Write(h.character);
            bw.Write(h.gameMode);
            bw.Write(h.tickRate);
            bw.Write(h.tickCount);
            bw.Write(h.metaReserved);
            bw.Write(h.score);
            bw.Write(h.stage);
            bw.Write(h.saveTimeMs);
            // 剩余 63 字节预留
            for (int i = 0; i < 63; i++) bw.Write((byte)0);

            // tick body
            for (int i = 0; i < h.tickCount; i++)
                bw.Write(file.TickMasks[i]);

            // v3 新增：校验帧位置表
            int vCount = file.ValidatePositions != null ? file.ValidatePositions.Length / 2 : 0;
            bw.Write(vCount);
            if (file.ValidatePositions != null)
            {
                for (int i = 0; i < file.ValidatePositions.Length; i++)
                    bw.Write(file.ValidatePositions[i]);
            }

            // v3.1 新增：校验帧游戏状态表（紧跟位置表之后，按帧顺序）
            int sCount = file.ValidateStates != null ? file.ValidateStates.Length : 0;
            bw.Write(sCount);
            if (file.ValidateStates != null)
            {
                for (int i = 0; i < file.ValidateStates.Length; i++)
                    bw.Write(file.ValidateStates[i]);
            }
        }

        public ReplayFile Load(string path)
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            var h = new ReplayHeader
            {
                magic        = br.ReadInt32(),
                version      = br.ReadInt32(),
                seed         = br.ReadUInt64(),
                character    = br.ReadInt32(),
                gameMode     = br.ReadInt32(),
                tickRate     = br.ReadInt32(),
                tickCount    = br.ReadInt32(),
                metaReserved = br.ReadByte()
            };

            if (h.magic != ReplayHeader.Magic)
                throw new IOException($"Invalid replay file magic: 0x{h.magic:X8}");
            if (h.version > ReplayHeader.CurrentVersion)
                throw new IOException($"Replay version {h.version} not supported (max {ReplayHeader.CurrentVersion})");

            if (h.version >= 2)
            {
                h.score      = br.ReadInt32();
                h.stage      = br.ReadInt32();
                h.saveTimeMs = br.ReadInt64();
                for (int i = 0; i < 63; i++) br.ReadByte();
            }
            else
            {
                // v1 旧格式：跳过旧预留区（79 字节），score/stage/saveTimeMs 保持 0
                for (int i = 0; i < 79; i++) br.ReadByte();
            }

            byte[] masks = new byte[h.tickCount];
            for (int i = 0; i < h.tickCount; i++)
                masks[i] = br.ReadByte();

            // v3 新增：读取校验帧位置表
            float[] validatePos = null;
            ushort[] validateStates = null;
            if (h.version >= 3)
            {
                int vCount = br.ReadInt32();
                validatePos = new float[vCount * 2];
                for (int i = 0; i < validatePos.Length; i++)
                    validatePos[i] = br.ReadSingle();

                // v3.1 新增：读取校验帧游戏状态表（紧跟位置表之后）
                if (h.version >= 3)
                {
                    int sCount = br.ReadInt32();
                    validateStates = new ushort[sCount];
                    for (int i = 0; i < sCount; i++)
                        validateStates[i] = br.ReadUInt16();
                }
            }

            return new ReplayFile { Header = h, TickMasks = masks, ValidatePositions = validatePos, ValidateStates = validateStates };
        }
    }

    /// <summary>JSON 调试序列化（可读性好，体积大）。只用于开发期排查失步。</summary>
    public class JsonReplaySerializer : IReplaySerializer
    {
        [System.Serializable]
        private class JsonReplayWrapper
        {
            public int magic;
            public int version;
            public ulong seed;
            public int character;
            public int gameMode;
            public int tickRate;
            public int tickCount;
            public int score;
            public int stage;
            public long saveTimeMs;
            public string ticks;
        }

        public void Save(ReplayFile file, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var w = new JsonReplayWrapper
            {
                magic      = file.Header.magic,
                version    = file.Header.version,
                seed       = file.Header.seed,
                character  = file.Header.character,
                gameMode   = file.Header.gameMode,
                tickRate   = file.Header.tickRate,
                tickCount  = file.Header.tickCount,
                score      = file.Header.score,
                stage      = file.Header.stage,
                saveTimeMs = file.Header.saveTimeMs,
                ticks      = BytesToHex(file.TickMasks)
            };
            File.WriteAllText(path, JsonUtility.ToJson(w, true));
        }

        public ReplayFile Load(string path)
        {
            var w = JsonUtility.FromJson<JsonReplayWrapper>(File.ReadAllText(path));
            if (w.magic != ReplayHeader.Magic)
                throw new IOException($"Invalid replay JSON magic: 0x{w.magic:X8}");
            var file = new ReplayFile
            {
                Header = new ReplayHeader
                {
                    magic      = w.magic,
                    version    = w.version,
                    seed       = w.seed,
                    character  = w.character,
                    gameMode   = w.gameMode,
                    tickRate   = w.tickRate,
                    tickCount  = w.tickCount,
                    score      = w.score,
                    stage      = w.stage,
                    saveTimeMs = w.saveTimeMs
                },
                TickMasks = HexToBytes(w.ticks)
            };
            return file;
        }

        private static string BytesToHex(byte[] buf)
        {
            var sb = new System.Text.StringBuilder(buf.Length * 2 + buf.Length);
            for (int i = 0; i < buf.Length; i++)
            {
                sb.Append(buf[i].ToString("X2"));
                if ((i + 1) % 20 == 0) sb.Append('\n');
                else sb.Append(' ');
            }
            return sb.ToString();
        }

        private static byte[] HexToBytes(string hex)
        {
            var cleaned = new System.Text.StringBuilder(hex.Length);
            foreach (char c in hex) if (c != ' ' && c != '\n' && c != '\r' && c != '\t') cleaned.Append(c);
            if (cleaned.Length % 2 != 0) throw new IOException("Hex string length not even");
            byte[] buf = new byte[cleaned.Length / 2];
            for (int i = 0; i < buf.Length; i++)
                buf[i] = System.Convert.ToByte(cleaned.ToString(i * 2, 2), 16);
            return buf;
        }
    }
}

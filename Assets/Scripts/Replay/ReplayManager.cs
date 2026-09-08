using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 回放子系统总控。
    ///
    /// 必须挂到一个 DDOL GameObject 上才能工作。在场景里放一个空 GameObject 叫 "ReplaySystem"
    /// 并挂载此脚本即可——Awake 里自动 DontDestroyOnLoad。
    ///
    /// 执行顺序：
    ///   Update（每帧，所有模式）：
    ///     a. ConsumeEdges()     —— 先清旧边沿（上一 tick 留下的）
    ///     b. SampleFromUnity()  —— 再采样物理按键，算新边沿并锁存
    ///        → 这样玩法 FixedUpdate 在同一帧读到的是本帧 Update 刚算的边沿
    ///
    ///   FixedUpdate（仅 Record/Playback 模式，Idle 时完全不推进）：
    ///     1. SimClock.Tick()        —— 推进模拟时间
    ///     2. AdvanceTick()          —— 回放模式前进到下一帧掩码
    ///     3. recordBuffer.Add()     —— 录制模式写入 held
    ///     4. Unity 随后 dispatch 所有其他 FixedUpdate —— 玩法逻辑在此运行
    ///
    /// timeScale=0 时 Unity 停止 FixedUpdate → SimClock 暂停，录制天然暂停；
    /// Update 不受影响，菜单/UI 交互正常工作。
    /// </summary>
    public class ReplayManager : MonoBehaviour
    {
        public enum Mode { Idle, Record, Playback }

        public static ReplayManager Instance { get; private set; }

        public Mode CurrentMode { get; private set; } = Mode.Idle;

        /// <summary>所有玩法脚本应读这里拿逻辑按键</summary>
        public static IInputProvider Input { get; private set; }

        // ---- 内部 ----
        private readonly LiveInputProvider  live = new();
        private ReplayInputProvider replay;

        // 录制缓冲（每 tick 1 字节，50Hz ≈ 360KB/小时，足够内存装下整局）
        private readonly List<byte> recordBuffer = new();
        private ulong recordSeed;

        // 回放文件元信息
        private ReplayHeader playbackHeader;

        // 回放目录（桌面 Saves 文件夹，用户要求显式路径）
        public static string SavesDir => @"C:\Users\34274\Desktop\Saves";

        // 外部钩子：Game1 初始化后调用 BeginRecord 或 BeginPlayback
        public static void BeginRecord()
        {
            EnsureInstance();
            Instance.recordSeed = (ulong)UnityEngine.Random.Range(0, int.MaxValue);
            GameRNG.Init(Instance.recordSeed);
            SimClock.Reset();
            Instance.recordBuffer.Clear();
            Instance.CurrentMode = Mode.Record;
            Instance.replay = null; // 清掉上一次回放残留
            Input = Instance.live;
            Debug.Log($"[ReplayManager] BeginRecord seed=0x{Instance.recordSeed:X16}");
        }

        public static void BeginPlayback(string path)
        {
            EnsureInstance();
            var serializer = new BinaryReplaySerializer();
            ReplayFile file = serializer.Load(path);
            GameRNG.Init(file.Header.seed);
            SimClock.Reset();
            Instance.replay = new ReplayInputProvider(file.TickMasks);
            Instance.recordBuffer.Clear();
            Instance.playbackHeader = file.Header;
            Instance.CurrentMode = Mode.Playback;
            Input = Instance.replay;
            Debug.Log($"[ReplayManager] BeginPlayback seed=0x{file.Header.seed:X16} ticks={file.Header.tickCount}");
        }

        /// <summary>
        /// 手动保存当前录制为下一个 Saves_x 文件；保存后自动回到 Idle 模式。
        /// 超过 10 条时自动淘汰：优先删得分最低的，同分时删保存时间最久的。
        /// </summary>
        public static string SaveRecording()
        {
            EnsureInstance();
            if (Instance.CurrentMode != Mode.Record) return null;
            string dir = SavesDir;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // 淘汰：检查现有文件，如果 >= MaxRecordSlots，删得分最低的
            PruneIfNeeded(dir);

            int next = 0;
            while (File.Exists(Path.Combine(dir, $"Saves_{next}.rply"))) next++;
            string path = Path.Combine(dir, $"Saves_{next}.rply");

            var gm = Global_GameManager.Instance;
            var serializer = new BinaryReplaySerializer();
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var file = new ReplayFile
            {
                Header = new ReplayHeader
                {
                    magic       = ReplayHeader.Magic,
                    version     = ReplayHeader.CurrentVersion,
                    seed        = Instance.recordSeed,
                    character   = gm != null ? (int)gm.character : 0,
                    gameMode    = gm != null ? (int)gm.gameMode  : 0,
                    tickRate    = (int)SimClock.FixedTickRate,
                    tickCount   = Instance.recordBuffer.Count,
                    score       = gm != null ? gm.Score : 0,
                    stage       = gm != null ? gm.SceneLevel : 1,
                    saveTimeMs  = nowMs
                },
                TickMasks = Instance.recordBuffer.ToArray()
            };
            serializer.Save(file, path);
            Instance.recordBuffer.Clear();
            Instance.CurrentMode = Mode.Idle;
            Input = Instance.live;
            Debug.Log($"[ReplayManager] SaveRecording → score={file.Header.score} stage={file.Header.stage} → 切回 Idle");
            return path;
        }

        /// <summary>当目录内文件 >= 10 时：读每个文件的头，淘汰得分最低（同分则保存时间最久）的</summary>
        private static void PruneIfNeeded(string dir)
        {
            var files = Directory.GetFiles(dir, "*.rply");
            if (files.Length < ReplayHeader.MaxRecordSlots) return;

            var serializer = new BinaryReplaySerializer();
            string victimPath = null;
            int victimScore = int.MaxValue;
            long victimMs = long.MaxValue;

            foreach (var f in files)
            {
                try
                {
                    var h = serializer.Load(f).Header;
                    if (h.score < victimScore || (h.score == victimScore && h.saveTimeMs < victimMs))
                    {
                        victimScore = h.score;
                        victimMs    = h.saveTimeMs;
                        victimPath  = f;
                    }
                }
                catch (Exception) { /* 损坏文件直接当受害者删 */ victimPath = f; victimScore = -1; }
            }

            if (victimPath != null)
            {
                File.Delete(victimPath);
                Debug.Log($"[ReplayManager] Prune 淘汰 {Path.GetFileName(victimPath)} (score={victimScore})");
            }
        }

        /// <summary>放弃当前录制（不保留），同时回到 Idle</summary>
        public static void DiscardRecording()
        {
            EnsureInstance();
            Instance.recordBuffer.Clear();
            Instance.replay = null;
            Instance.CurrentMode = Mode.Idle;
            Input = Instance.live;
            Debug.Log($"[ReplayManager] DiscardRecording → 切回 Idle");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 默认 Idle 下也用 LiveInputProvider，避免游戏还没开始录制时 Input 为 null
            Input = live;
            Debug.Log($"[ReplayManager] Awake Mode={CurrentMode}");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Update（每帧必跑，不受 timeScale 影响）：
        ///   a. 清上一帧锁存的边沿 —— 避免同一边沿被重复读到
        ///   b. 采样物理按键 → 算新边沿并锁存
        /// 这样 FixedUpdate 里的玩法逻辑读到的边沿是本帧刚算的、且每帧只读到一次。
        /// </summary>
        private void Update()
        {
            // 回放模式不采样物理输入（屏蔽玩家），但仍要清旧边沿
            live.ConsumeEdges();

            if (CurrentMode != Mode.Playback)
                live.SampleFromUnity();
        }

        /// <summary>
        /// FixedUpdate（仅 Record/Playback 模式才推进；Idle 时完全不动，菜单场景不消耗 SimClock）。
        /// timeScale=0（暂停/结算）时 Unity 自动停掉这里，录制天然暂停。
        /// </summary>
        private void FixedUpdate()
        {
            if (CurrentMode == Mode.Idle) return;

            // 1. 推进模拟时间（可能因 SimScale=0 而不推进）
            bool didTick = SimClock.Tick();
            if (!didTick) return; // 冻结时不推进输入/录制/定时器

            // 2. 驱动 tick 定时器（确定性，替代 Unity Invoke）
            SimTimer.Tick();

            // 3. 回放模式：前进到下一帧掩码
            if (replay != null)
                replay.AdvanceTick();

            // 4. 录制模式：写入当前 held
            if (CurrentMode == Mode.Record)
                recordBuffer.Add(live.HeldMask);
        }

        private static void EnsureInstance()
        {
            if (Instance == null)
            {
                var go = new GameObject("ReplaySystem");
                go.AddComponent<ReplayManager>();
            }
        }
    }
}

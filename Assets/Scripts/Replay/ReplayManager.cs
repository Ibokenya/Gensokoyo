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
    /// 🔴 关键架构（timeScale=0 也要跑的设计 + 覆盖式边沿模型）：
    ///
    ///   Update（每帧，不受 timeScale 影响）：
    ///     a. SampleFromUnity() —— 覆盖式采样物理按键，算 edgesDown/Up（live 模式）
    ///        🔴 覆盖式 = 不需要 ConsumeEdges！按键没变时 edgesDown/Up 自动归零。
    ///     b. 回放自然结束检测
    ///
    ///   LateUpdate（每帧，不受 timeScale 影响，在所有 Update 之后执行）：
    ///     用 Time.unscaledDeltaTime 累计，按 50Hz 节奏推进：
    ///     1. SimClock.Tick()       —— 推进模拟时间
    ///     2. SimTimer.Tick()       —— 驱动 tick 定时器
    ///     3. replay.AdvanceTick()   —— 回放文件位置前进
    ///     4. recordBuffer.Add()    —— 录制写入 held（低 8 位）
    ///
    ///   FixedUpdate（只有 timeScale>0 时跑）：所有玩法逻辑在此运行。
    ///   timeScale=0 时物理引擎停（子弹、敌人不动），但 replay 系统继续推进 → 决死时停能正常工作。
    ///
    /// Script Execution Order 不再重要（LateUpdate 天然在所有 Update 之后执行）。
    ///
    /// 🔴 帧顺序详解（60fps 下）：
    ///   FixedUpdate (~50Hz)：玩法脚本读 GetKeyDown/GetKeyUp —— edgesDown/Up 从上次 SampleFromUnity 存活到此刻 ✓
    ///   Update：ReplayManager.SampleFromUnity → 覆盖式重算 edgesDown/Up → 下一帧 FixedUpdate 读 ✓
    ///   LateUpdate：tick 推进（unscaledDeltaTime）
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
        public ReplayInputProvider replay;

        // 录制缓冲（每 tick 1 字节，50Hz ≈ 360KB/小时，足够内存装下整局）
        private readonly List<byte> recordBuffer = new();
        private ulong recordSeed;

        // 回放文件元信息
        private ReplayHeader playbackHeader;

        // 🔴 LateUpdate 累计器 —— 用 unscaledDeltaTime 累计，不受 timeScale 影响
        private float unscaledAccumulator;

        /// <summary>回放目录（桌面 Saves 文件夹，用户要求显式路径）</summary>
        public static string SavesDir => @"C:\Users\34274\Desktop\Saves";

        /// <summary>
        /// 🔴 引用计数的 SkipTick —— 任何来源（dialog / 暂停 UI / 回放暂停）
        /// 只要 count > 0 就跳过 tick 推进（不 tick、不 record、不 AdvanceTick）。
        /// AddSkipTick / RemoveSkipTick 必须成对调用（推荐在 OnEnable/OnDisable 里）。
        /// </summary>
        private static int skipTickRefCount;
        public static bool SkipTick => skipTickRefCount > 0;

        /// <summary>新增一个暂停 tick 的来源（dialog 打开、暂停菜单打开等）</summary>
        public static void AddSkipTickReason()
        {
            skipTickRefCount++;
            if (skipTickRefCount == 1)
                Debug.Log($"[ReplayManager] SkipTick ON (ref=1)");
        }

        /// <summary>移除一个暂停 tick 的来源 —— 最后一个移除后恢复 tick 推进</summary>
        public static void RemoveSkipTickReason()
        {
            skipTickRefCount--;
            if (skipTickRefCount <= 0)
            {
                skipTickRefCount = 0;
                Debug.Log($"[ReplayManager] SkipTick OFF (ref=0)");
            }
        }

        // 外部钩子：Game1 初始化后调用 BeginRecord 或 BeginPlayback
        public static void BeginRecord()
        {
            EnsureInstance();
            Instance.recordSeed = (ulong)UnityEngine.Random.Range(0, int.MaxValue);
            GameRNG.Init(Instance.recordSeed);
            SimClock.Reset();
            SimTimer.CancelAll();                // 🔴 清掉残留定时器（菜单/上一局注册的）
            Instance.recordBuffer.Clear();
            Instance.unscaledAccumulator = 0f;
            Instance.CurrentMode = Mode.Record;
            Instance.replay = null; // 清掉上一次回放残留
            Input = Instance.live;
            skipTickRefCount = 0;                // 🔴 重置 SkipTick 引用计数
            // 🔴 清掉菜单场景残留的物理键边沿（用户可能在菜单按过 X/Esc/Z 后还没进 Game1）
            Instance.live.ConsumeEdges();
            Debug.Log($"[ReplayManager] BeginRecord seed=0x{Instance.recordSeed:X16}");
        }

        /// <summary>UIManager 由 UIManager.OnEnable 时注册（它负责 Show/Hide 回放暂停 UI）</summary>
        public static UIManager UIManagerInstance;
        
        /// <summary>上一次 BeginPlayback 的路径（供 ReStart 用）</summary>
        public static string playbackPath;
        
        public static void BeginPlayback(string path)
        {
            EnsureInstance();
            playbackPath = path;
            var serializer = new BinaryReplaySerializer();
            ReplayFile file = serializer.Load(path);
            GameRNG.Init(file.Header.seed);
            SimClock.Reset();
            SimTimer.CancelAll();                // 🔴 清掉残留定时器（菜单/上一局注册的）
            Instance.replay = new ReplayInputProvider(file.TickMasks);
            Instance.recordBuffer.Clear();
            Instance.unscaledAccumulator = 0f;
            Instance.GameEndTriggered = false;   // 🔴 重置回放结束标志 —— 退回菜单再进回放不会卡住
            skipTickRefCount = 0;                // 🔴 重置 SkipTick 引用计数
            Instance.playbackHeader = file.Header;
            Instance.CurrentMode = Mode.Playback;
            Input = Instance.replay;

            // 🔴 关键：把回放文件里的角色/难度元信息写回 Global_GameManager，
            // 这样 GunAnime/PlayerAnime/SpellCardEffect 的 character 判断能正确走 Marisa 分支
            if (Global_GameManager.Instance != null)
            {
                Global_GameManager.Instance.character = (Character)file.Header.character;
                Global_GameManager.Instance.gameMode  = (GameMode)file.Header.gameMode;
            }
            // 🔴 清掉菜单场景残留的物理键边沿
            Instance.live.ConsumeEdges();
            // 🔴 种子 + tick 数 debug 打印
            Debug.Log($"[ReplayManager] BeginPlayback seed=0x{file.Header.seed:X16} ticks={file.Header.tickCount} character={(Character)file.Header.character} mode={(GameMode)file.Header.gameMode}");
        }

        /// <summary>回放自然结束 / 用户 ReStart 时调用：完整清理 + 重新加载 Game1 场景从头播放</summary>
        public static void RestartPlayback()
        {
            if (Instance == null || string.IsNullOrEmpty(playbackPath)) return;

            // 先切回 Idle，确保 RestartGame 的清理不被 Playback 模式干扰
            EnsureInstance();
            Instance.replay = null;
            Instance.recordBuffer.Clear();
            Instance.unscaledAccumulator = 0f;
            Instance.CurrentMode = Mode.Idle;
            Input = Instance.live;
            Time.timeScale = 1f;

            // 🔴 用 Global_SceneManager.RestartGame 做完整清理：
            // RecycleAllEnemies → 回收道具 → ClearAllPools → ResetGameDate → ResetSceneFromJson → IntoNextScene("Game1") 或 LoadScene
            // 这保证了无论回放结束在 Game1/Game2/Boss 哪个场景，都能干净地回到 Game1 初始状态
            if (Global_SceneManager.Instance != null)
            {
                Global_SceneManager.Instance.RestartGame();
            }

            // 清理完后重新 Initialize 回放状态
            // （必须在 RestartGame 之后，否则 RestartGame 里的 IntoNextScene 会覆盖我们的 Playback 设置）
            BeginPlayback(playbackPath);
            Debug.Log($"[ReplayManager] RestartPlayback → clean restart from Game1, seed=0x{Instance.recordSeed:X16}");
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
            Debug.Log($"[ReplayManager] SaveRecording → seed=0x{file.Header.seed:X16} ticks={file.Header.tickCount} score={file.Header.score} stage={file.Header.stage} → {path}");
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

        // --------------------------- 回放暂停 API ---------------------------

        public static bool IsReplayPaused { get; private set; }
        
        public static void NotifyReplayPaused()
        {
            IsReplayPaused = true;
        }
        
        public static void NotifyReplayResumed()
        {
            IsReplayPaused = false;
        }

        private bool GameEndTriggered;

        public static void ResetGameEndFlag()
        {
            EnsureInstance();
            Instance.GameEndTriggered = false;
        }

        public static bool IsPlaybackFinished()
        {
            var inst = Instance;
            return inst != null && inst.CurrentMode == Mode.Playback &&
                   inst.replay != null && inst.replay.IsFinished;
        }

        // --------------------------- 帧驱动 ---------------------------

        /// <summary>
        /// 🔴 Update（每帧，不受 timeScale 影响）：
        ///   1. live 模式：SampleFromUnity 采样物理键 → 覆盖式算 edgesDown/Up（不需要 ConsumeEdges！）
        ///   2. 回放自然结束检测
        ///
        /// 帧顺序详解（60fps 下）：
        ///   1. FixedUpdate（~50Hz）：所有玩法脚本在此读 GetKeyDown/GetKeyUp
        ///      - 覆盖式 edgesDown/Up 从上一次 SampleFromUnity 算出后一直存活到这次 FixedUpdate 消费 ✓
        ///   2. Update：ReplayManager.SampleFromUnity → 覆盖式重算 edgesDown/Up → 下一帧 FixedUpdate 读 ✓
        ///   3. LateUpdate：tick 推进（unscaledDeltaTime，不受 timeScale 影响）
        ///
        /// 决死期间（timeScale=0）：
        ///   ✅ Update 继续跑 → SampleFromUnity 正常算 edges → SpellCardEffect.Update 能读决死按键 ✓
        ///   ✅ LateUpdate 继续跑 → SimClock 继续 tick → WaitForSecondsSim 正常等 ✓
        ///   ✅ 回放文件继续推 → 回放能读到决死按键 ✓
        ///   ✅ 录制继续写 → 录制包含决死按键 ✓
        ///   ❌ FixedUpdate 停 → 物理停（时停视觉 ✓）
        /// </summary>
        private void Update()
        {
            // 🔴 回放自然结束检测
            if (CurrentMode == Mode.Playback && replay != null && replay.IsFinished && !GameEndTriggered)
            {
                GameEndTriggered = true;
                Debug.Log($"[ReplayManager] 回放结束 → 触发 ReplayPauseUI");
                NotifyReplayPaused();
                UIManagerInstance?.ShowReplayPause();
            }

            // 🔴 live 模式：覆盖式采样 —— 用 = 覆盖 edgesDown/Up，**不需要 ConsumeEdges**
            if (CurrentMode == Mode.Record)
                live.SampleFromUnity();

            // 🔴 回放模式：不采样也不 AdvanceTick
            // AdvanceTick 必须和 SimClock.Tick 同频（50Hz），放在 LateUpdate 的 tick 循环里
        }

        /// <summary>
        /// 🔴 LateUpdate（每帧，不受 timeScale 影响，在所有 Update 之后执行）：
        ///   用 Time.unscaledDeltaTime 累计，按 50Hz 节奏推进：
        ///     SimClock.Tick → SimTimer.Tick → 回放文件推进 → 录制写文件
        ///
        ///   🔴 注意：ConsumeEdges 已完全移除！LiveInputProvider 用覆盖式 SampleFromUnity，
        ///   edgesDown/Up 只在按键变化那一帧非零，不需要手动清。
        ///
        /// 决死期间 timeScale=0 时：
        ///   ✅ LateUpdate 继续跑（不受 timeScale 影响）→ SimClock 继续 tick
        ///   ✅ WaitForSecondsSim 能正常等待 → 决死协程能推进
        ///   ✅ 回放文件位置继续推进 → SpellCardEffect.Update 能读到正确边沿
        ///   ✅ recordBuffer 继续写 → 录制包含决死期间的按键
        ///   ❌ FixedUpdate 停 → 物理引擎停 → 子弹敌人不动（时停视觉效果保留 ✓）
        /// </summary>
        private void LateUpdate()
        {
            if (CurrentMode == Mode.Idle) return;

            // 🔴 dialog 期间跳过所有 tick（录制/回放/dialog 同步跳过）
            if (SkipTick) return;

            // 🔴 用 unscaledDeltaTime 累计 —— timeScale=0 也继续累
            // 🔴 clamp 到 0.1s（5 帧最大）防止 Editor Pause / 长时间卡帧后一次性大跳
            //    不 clamp 的话：Pause 20s → unscaledDeltaTime=20s → 瞬间推进 1000 ticks → 回放跳到末尾
            unscaledAccumulator += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            while (unscaledAccumulator >= 0.02f) // 50Hz = 0.02s/tick
            {
                unscaledAccumulator -= 0.02f;

                // 🔴 帧时序对齐核心：先推进回放 held，再推进 SimClock
                // 这样帧 N LateUpdate AdvanceTick → masks[N]，帧 N+1 FixedUpdate 读到 masks[N]
                // 录制帧 N+1 FixedUpdate 也读到 masks[N]（因为录制帧 N LateUpdate 写 masks[N]）
                if (CurrentMode == Mode.Playback && replay != null)
                    replay.AdvanceTick();

                // 1. 推进模拟时间
                bool didTick = SimClock.Tick();
                if (didTick)
                {
                    // 2. 驱动 tick 定时器
                    SimTimer.Tick();

                    // 3. 录制模式：写 recordBuffer（和 SimClock.Tick 50Hz 严格对齐）
                    if (CurrentMode == Mode.Record)
                        recordBuffer.Add(LogicalKeyMask.LowByte(live.HeldMask));

                    // 4. 🔴 State Hash（每 10 tick 一次）
                    if (didTick && SimClock.SimTick % 10 == 0)
                        LogStateHash();
                }
            }
            // 🔴 无 ConsumeEdges —— LiveInputProvider 覆盖式自动清
        }

        private static void EnsureInstance()
        {
            if (Instance == null)
            {
                var go = new GameObject("ReplaySystem");
                go.AddComponent<ReplayManager>();
            }
        }

        /// <summary>
        /// 🔴 每 50 tick 对游戏状态做指纹，用于定位录 vs 回放的第一个分叉点。
        /// 指纹内容：SimTick + GameRNG 状态 + 玩家位置 + 玩家移动方向。
        /// 录制时 Console 打 [HASH-REC]，回放时打 [HASH-PLAY]，搜 hash 字符串就能对齐比对。
        ///
        /// 如果录和回放的 hash 在 tick 100 不同，说明 tick 51-100 之间发生了第一次分叉。
        /// </summary>
        private static void LogStateHash()
        {
            var gm = Global_GameManager.Instance;
            if (gm == null) return;

            // 1. SimTick（全局时钟）
            ulong tick = SimClock.SimTick;

            // 2. GameRNG 状态（最敏感的分叉探针 —— 只要录 vs 回放有任何 RNG 消费差就会不同）
            var rngState = GameRNG.State;

            // 3. 玩家位置（如果能找到）
            Transform playerTf = null;
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTf = playerObj.transform;

            // 组合 hash：简单 djb2 风格，足够定位差异
            ulong h = 5381;
            h = (h * 33) ^ tick;
            h = (h * 33) ^ rngState.s0;
            h = (h * 33) ^ rngState.s1;
            if (playerTf != null)
            {
                // 用 1 位精度的位置避免浮点噪声
                int px = Mathf.RoundToInt(playerTf.position.x * 10);
                int py = Mathf.RoundToInt(playerTf.position.y * 10);
                h = (h * 33) ^ (uint)px;
                h = (h * 33) ^ (uint)py;
            }

            string tag = Instance != null && Instance.CurrentMode == Mode.Record ? "HASH-REC" : "HASH-PLAY";
            string pos = playerTf != null ? $"pos=({playerTf.position.x:F2},{playerTf.position.y:F2})" : "pos=N/A";
            Debug.Log($"[{tag}] tick={tick} hash=0x{h:X16} rng=({rngState.s0:X8},{rngState.s1:X8}) {pos}");
        }
    }
}

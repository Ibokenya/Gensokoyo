using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 确定性模拟时钟，固定 50Hz tick
    ///
    /// 设计：SimClock 是纯静态类，**不自己注册 FixedUpdate**。
    /// tick 由 ReplayManager 在 FixedUpdate 开头调用 SimClock.Tick() 驱动，
    /// 这样 tick 推进、输入消费、回放前进的执行顺序完全确定。
    ///
    /// timeScale=0 时 Unity 停止 FixedUpdate → SimClock 自动暂停，不需要额外开关。
    ///
    /// SimScale 慢放模型：FixedUpdate 照常 50Hz 跑，tickAccumulator 按 SimScale 累加。
    /// SimScale=1.0 → 每 FixedUpdate +1 → tick++ 一次；
    /// SimScale=0.3 → 每 FixedUpdate +0.3 → 约每 4 tick 累加够 1 → tick++ 一次。
    /// 这样 tick 计数、RNG 调用节奏、所有玩法逻辑同步变慢。
    /// SimScale=0 → 完全冻结（FixedUpdate 仍跑，但 SimClock.Tick() 直接 return 不推进）。
    /// </summary>
    public static class SimClock
    {
        public const float FixedTickDt    = 0.02f; // 50Hz
        public const float FixedTickRate  = 50f;
        public const int   FixedTickMs    = 20;

        /// <summary>累计 tick 数（整数，从 0 开始）</summary>
        public static ulong SimTick { get; private set; }

        /// <summary>累计模拟时间（秒），= SimTick * FixedTickDt</summary>
        public static float SimTime => SimTick * FixedTickDt;

        /// <summary>慢放比例（>0 推进，0 冻结）</summary>
        public static float SimScale { get; private set; } = 1f;

        /// <summary>浮点累加器，用于 SimScale 慢速下的 fractional tick</summary>
        private static float tickAccumulator;

        /// <summary>让外部判断：这次 FixedUpdate 是否真的推进了 SimTick</summary>
        public static ulong LastTickThisFrame { get; private set; }
        public static bool  DidTickThisFrame  { get; private set; }

        /// <summary>由 ReplayManager.FixedUpdate 开头调用。返回本帧是否推进了至少一次 tick</summary>
        public static bool Tick()
        {
            LastTickThisFrame = SimTick;
            if (SimScale <= 0f)
            {
                tickAccumulator = 0f;
                DidTickThisFrame = false;
                return false;
            }

            tickAccumulator += SimScale;
            DidTickThisFrame = false;
            // 整数 tick 才计次，保证 tick 计数离散、RNG 调用节奏一致
            while (tickAccumulator >= 1f)
            {
                SimTick++;
                tickAccumulator -= 1f;
                DidTickThisFrame = true;
            }
            return DidTickThisFrame;
        }

        /// <summary>新一局（Game1 初始化时）重置</summary>
        public static void Reset()
        {
            SimTick          = 0;
            SimScale         = 1f;
            tickAccumulator  = 0f;
            LastTickThisFrame= 0;
            DidTickThisFrame = false;
        }

        /// <summary>设置慢放/加速比例</summary>
        public static void SetScale(float scale)
        {
            SimScale = Mathf.Max(0f, scale);
        }
    }
}

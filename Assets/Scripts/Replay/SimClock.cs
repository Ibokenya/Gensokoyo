using UnityEngine;

namespace ReplaySystem
{
    // 确定性模拟时钟，固定 50Hz tick
    //
    // 设计：SimClock 是纯静态类，**不自己注册 FixedUpdate**。
    // tick 由 ReplayManager 在 FixedUpdate 开头调用 SimClock.Tick() 驱动，
    // 这样 tick 推进、输入消费、回放前进的执行顺序完全确定。
    //
    // timeScale=0 时 Unity 停止 FixedUpdate → SimClock 自动暂停，不需要额外开关。
    //
    // SimScale 慢放模型：FixedUpdate 照常 50Hz 跑，tickAccumulator 按 SimScale 累加。
    // SimScale=1.0 → 每 FixedUpdate +1 → tick++ 一次；
    // SimScale=0.3 → 每 FixedUpdate +0.3 → 约每 4 tick 累加够 1 → tick++ 一次。
    // 这样 tick 计数、RNG 调用节奏、所有玩法逻辑同步变慢。
    // SimScale=0 → 完全冻结（FixedUpdate 仍跑，但 SimClock.Tick() 直接 return 不推进）。
    public static class SimClock
    {
        public const float FixedTickDt    = 0.02f; // 50Hz
        public const float FixedTickRate  = 50f;
        public const int   FixedTickMs    = 20;// 50Hz tick 每帧 20ms

        public static ulong SimTick { get; private set; }

        public static float SimTime => SimTick * FixedTickDt;

        public static float SimScale { get; private set; } = 1f;

        private static float tickAccumulator;

        public static ulong LastTickThisFrame { get; private set; }
        public static bool  DidTickThisFrame  { get; private set; }

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

        public static void Reset()
        {
            SimTick          = 0;
            SimScale         = 1f;
            tickAccumulator  = 0f;
            LastTickThisFrame= 0;
            DidTickThisFrame = false;
        }

        public static void SetScale(float scale)
        {
            SimScale = Mathf.Max(0f, scale);
        }
    }
}
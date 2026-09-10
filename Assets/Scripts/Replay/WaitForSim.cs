using UnityEngine;

namespace ReplaySystem
{
    /// <summary>
    /// 协程 yield 指令：等指定的模拟秒数（SimClock.SimTime）。
    ///
    /// 替代玩法协程里的 WaitForSeconds / WaitForSecondsRealtime：
    /// - WaitForSeconds   受 timeScale 影响，但我们用 SimScale 做慢放，timeScale 只当 0/1
    /// - WaitForSecondsRealtime 完全不受任何时间控制——回放/SimScale 下就是个 bug
    ///
    /// WaitForSecondsSim 看的是 SimClock.SimTime，所以：
    /// - 回放时用回放文件里的固定 tick 序列，时间确定性 ✅
    /// - SimScale=0.3 时，wait 2秒 实际等约 6.67 秒（SimClock 按比例慢）✅
    /// - timeScale=0 时 FixedUpdate 停，SimClock 也停——协程不会前进 ✅
    ///
    /// 使用：yield return new WaitForSecondsSim(2.0f);
    ///       yield return new WaitForTicksSim(100);  // 或直接按 tick 数等
    /// </summary>
    public class WaitForSecondsSim : CustomYieldInstruction
    {
        private readonly float targetTime;  // 等待期间 SimClock 达到的目标时间

        public WaitForSecondsSim(float seconds)
        {
            targetTime = SimClock.SimTime + seconds;
        }

        public override bool keepWaiting => SimClock.SimTime < targetTime;
    }

    /// <summary>
    /// 协程 yield 指令：等指定的 tick 数（精确，用于小间隔）。
    /// 用法：yield return new WaitForTicksSim(10);  // 等 10 个 tick = 0.2 秒
    /// </summary>
    public class WaitForTicksSim : CustomYieldInstruction
    {
        private readonly ulong targetTick;

        public WaitForTicksSim(int ticks)
        {
            if (ticks < 0) ticks = 0;
            targetTick = SimClock.SimTick + (ulong)ticks;
        }

        public override bool keepWaiting => SimClock.SimTick < targetTick;
    }
}

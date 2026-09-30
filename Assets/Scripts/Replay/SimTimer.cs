using System;
using System.Collections.Generic;

namespace ReplaySystem
{
    // 确定性 tick 定时器。替代 Unity 的 Invoke / CancelInvoke。
    //
    // 生命周期：ReplayManager.FixedUpdate 每帧 SimClock.Tick() 之后、其他 FixedUpdate 之前调用 SimTimer.Tick()。
    // 这样 tick 定时器的触发时机和所有玩法逻辑的 SimClock 推进是同步的。
    //
    // 使用：
    //   SimTimer.Once(callback, 100);        // 100 tick (2 秒) 后触发
    //   SimTimer.Once(callback, 0);          // 下一 tick 触发
    //   SimTimer.Repeat(callback, 25);       // 每 25 tick 触发一次
    //   SimTimer.CancelAll();                // 全清
    //   SimTimer.Cancel(handle);             // 清单个
    public static class SimTimer
    {
        private static readonly List<TimerEntry> pending = new();// 待执行定时器列表
        private static readonly List<TimerEntry> ready = new(); // 当 tick 到达时缓存，防止回调里修改集合
        private static long nextHandle = 1;

        public class TimerEntry
        {
            public long Handle;         // 句柄
            public ulong FireTick;      // 第一次触发的 SimTick
            public Action Callback;     // 回调
            public int PeriodTicks;     // Repeat 时 > 0；Once 时 = 0
        }

        public static long Once(Action callback, int delayTicks)
        {
            if (delayTicks < 0) delayTicks = 0;
            var entry = new TimerEntry
            {
                Handle     = nextHandle++,
                FireTick   = SimClock.SimTick + (ulong)delayTicks,
                Callback   = callback,
                PeriodTicks = 0
            };
            pending.Add(entry);
            return entry.Handle;
        }

        public static long Repeat(Action callback, int periodTicks)
        {
            if (periodTicks <= 0) periodTicks = 1;
            var entry = new TimerEntry
            {
                Handle      = nextHandle++,
                FireTick    = SimClock.SimTick + (ulong)periodTicks,
                Callback    = callback,
                PeriodTicks = periodTicks
            };
            pending.Add(entry);
            return entry.Handle;
        }

        public static void Cancel(long handle)
        {
            for (int i = 0; i < pending.Count; i++)
                if (pending[i].Handle == handle) { pending.RemoveAt(i); return; }
        }

        public static void CancelAll()
        {
            pending.Clear();
        }

        public static void Tick()
        {
            ulong now = SimClock.SimTick;
            ready.Clear();
            for (int i = 0; i < pending.Count; i++)
            {
                var e = pending[i];
                if (e.FireTick <= now)
                {
                    ready.Add(e);
                    if (e.PeriodTicks > 0)
                        e.FireTick = now + (ulong)e.PeriodTicks;
                    else
                        pending.RemoveAt(i--);
                }
            }
            // 分开执行防止回调里 CancelAll 影响遍历
            for (int i = 0; i < ready.Count; i++)
            {
                try { ready[i].Callback?.Invoke(); }
                catch (Exception ex) { UnityEngine.Debug.LogError($"[SimTimer] 回调异常: {ex.Message}\n{ex.StackTrace}"); }
            }
        }
    }
}
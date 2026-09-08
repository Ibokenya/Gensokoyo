using System;

namespace ReplaySystem
{
    /// <summary>
    /// 确定性伪随机数生成器（xorshift128+）
    ///
    /// 独立于 UnityEngine.Random，状态可完整序列化（用于回放/快照/校验）。
    /// 所有玩法脚本应调用 GameRNG；仅视觉表现（精灵变体、粒子种子、装饰抖动）允许继续用 UnityEngine.Random。
    ///
    /// 注意：EnemyShoot.cs 中原代码 Random.InitState(enemyIndex * 37) 是定时炸弹——
    /// 它中途重播种全局 RNG，会污染其他系统的随机数流。后续迁移时必须整段删除。
    /// 每个敌人的随机偏移改为在生成时从 GameRNG 一次性抽取，不再重播种。
    /// </summary>
    public static class GameRNG
    {
        private static ulong s0, s1;
        private static bool initialized = false;

        public struct RNGState
        {
            public ulong s0;
            public ulong s1;
        }

        public static bool IsInitialized => initialized;

        public static void Init(ulong seed)
        {
            s0 = seed;
            s1 = SplitMix64(seed);
            if (s0 == 0 && s1 == 0) { s0 = 1; s1 = 2; } // 确保非零状态
            initialized = true;
        }

        public static void Init(RNGState state)
        {
            s0 = state.s0; s1 = state.s1;
            initialized = true;
        }

        public static RNGState State
        {
            get => new RNGState { s0 = s0, s1 = s1 };
            set => Init(value);
        }

        /// <summary>核心：下一个 64-bit 伪随机整数</summary>
        public static ulong NextUInt64()
        {
            if (!initialized) { Init((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); }

            ulong x = s0, y = s1;
            s0 = y;
            x ^= x << 23; x ^= x >> 17; x ^= y ^ (y >> 26);
            s1 = x;
            return x + y;
        }

        /// <summary>[0, int.MaxValue] 非负整数</summary>
        public static int Next() => (int)(NextUInt64() & 0x7FFFFFFF);

        /// <summary>[0, max) 非负整数</summary>
        public static int Range(int max)
        {
            if (max <= 0) return 0;
            ulong limit = 0xFFFFFFFFFFFFFFFF - (0xFFFFFFFFFFFFFFFF % (ulong)max);
            ulong r;
            do { r = NextUInt64(); } while (r >= limit);
            return (int)(r % (ulong)max);
        }

        /// <summary>[minInclusive, maxExclusive) 整数</summary>
        public static int Range(int min, int max)
        {
            if (max <= min) return min;
            return min + Range(max - min);
        }

        /// <summary>[0f, 1f) 浮点（精度 2^-53）</summary>
        public static float value => (NextUInt64() >> 11) * (1f / 9007199254740992f);

        /// <summary>[0f, max) 浮点</summary>
        public static float Range(float max)
        {
            if (max <= 0f) return 0f;
            return value * max;
        }

        /// <summary>[min, max) 浮点</summary>
        public static float Range(float min, float max)
        {
            if (max <= min) return min;
            return min + value * (max - min);
        }

        private static ulong SplitMix64(ulong z)
        {
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }
}

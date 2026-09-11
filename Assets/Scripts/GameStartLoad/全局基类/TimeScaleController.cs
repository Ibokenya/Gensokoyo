using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 🔴 统一管理 Time.timeScale 的集中控制器
/// 解决"多个暂停源被一个恢复"的问题。
///
/// 设计思路：
/// - 硬暂停（timeScale=0）：引用计数。任何一个暂停源未恢复就保持 0。
/// - 软缩放（timeScale∈(0,1]）：栈。Boss 慢放等非 0 缩放效果。
/// - 最终 timeScale = 硬暂停计数 > 0 ? 0 : (栈顶值 ?? 1f)
///
/// 用法：
///   // Esc 暂停
///   TimeScaleController.RegisterHardPause();
///   TimeScaleController.UnregisterHardPause();
///
///   // Boss 慢放
///   TimeScaleController.RegisterSoftScale(0.3f);
///   TimeScaleController.UnregisterSoftScale();
///
///   // 场景切换重置
///   TimeScaleController.ResetAll();
/// </summary>
public static class TimeScaleController
{
    private static int hardPauseRefCount = 0;
    private static readonly Stack<float> softScaleStack = new Stack<float>();
    private const float DefaultScale = 1f;

    /// <summary>最终生效的 Time.timeScale 值（只读）</summary>
    public static float CurrentEffectiveScale
    {
        get
        {
            if (hardPauseRefCount > 0) return 0f;
            if (softScaleStack.Count > 0) return softScaleStack.Peek();
            return DefaultScale;
        }
    }

    /// <summary>是否有任何暂停/慢放源处于活跃状态</summary>
    public static bool HasAnyPause => CurrentEffectiveScale < DefaultScale;

    /// <summary>
    /// 注册硬暂停请求（timeScale=0）。引用计数 +1。
    /// 任何来源（Esc 暂停、决死、时停、GameOver、FinalUI 等）都调这个。
    /// </summary>
    public static void RegisterHardPause()
    {
        hardPauseRefCount++;
        Apply();
    }

    /// <summary>
    /// 释放硬暂停请求。引用计数 -1。计数到 0 才真正恢复。
    /// </summary>
    public static void UnregisterHardPause()
    {
        if (hardPauseRefCount > 0)
        {
            hardPauseRefCount--;
        }
        // 防止多调（成对调用保护）
        if (hardPauseRefCount < 0) hardPauseRefCount = 0;
        Apply();
    }

    /// <summary>
    /// 注册软缩放入栈（如 Boss 慢放 0.3f）。
    /// 如果此时有硬暂停，软缩放会被覆盖为 0；硬暂停解除后自动回到这个值。
    /// </summary>
    public static void RegisterSoftScale(float scale)
    {
        softScaleStack.Push(scale);
        Apply();
    }

    /// <summary>
    /// 释放最近一次软缩放。出栈后自动恢复到上一个缩放值或 1f。
    /// </summary>
    public static void UnregisterSoftScale()
    {
        if (softScaleStack.Count > 0)
        {
            softScaleStack.Pop();
        }
        Apply();
    }

    /// <summary>
    /// 清空所有暂停/缩放请求，强制恢复 timeScale=1。
    /// 场景切换、游戏重置时调用。
    /// </summary>
    public static void ResetAll()
    {
        hardPauseRefCount = 0;
        softScaleStack.Clear();
        Time.timeScale = DefaultScale;
    }

    /// <summary>重新计算并写入 Time.timeScale</summary>
    private static void Apply()
    {
        float target = CurrentEffectiveScale;
        if (!Mathf.Approximately(Time.timeScale, target))
        {
            Time.timeScale = target;
        }
    }
}

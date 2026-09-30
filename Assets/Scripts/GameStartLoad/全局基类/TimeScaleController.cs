using System.Collections.Generic;
using UnityEngine;

//  统一管理 Time.timeScale 的集中控制器
// 解决"多个暂停源被一个恢复"的问题。
//
// 设计思路：
// - 硬暂停（timeScale=0）：引用计数。任何一个暂停源未恢复就保持 0。
// - 软缩放（timeScale∈(0,1]）：栈。Boss 慢放等非 0 缩放效果。
// - 最终 timeScale = 硬暂停计数 > 0 ? 0 : (栈顶值 ?? 1f)
//
// 用法：
//   // Esc 暂停
//   TimeScaleController.RegisterHardPause();
//   TimeScaleController.UnregisterHardPause();
//
//   // Boss 慢放
//   TimeScaleController.RegisterSoftScale(0.3f);
//   TimeScaleController.UnregisterSoftScale();
//
//   // 场景切换重置
//   TimeScaleController.ResetAll();
public static class TimeScaleController
{
    private static int hardPauseRefCount = 0;// 硬暂停引用计数
    private static readonly Stack<float> softScaleStack = new Stack<float>();// 软缩放栈
    private const float DefaultScale = 1f;// 默认缩放值

    public static float CurrentEffectiveScale
    {
        get
        {
            if (hardPauseRefCount > 0) return 0f;
            if (softScaleStack.Count > 0) return softScaleStack.Peek();
            return DefaultScale;
        }
    }

    public static bool HasAnyPause => CurrentEffectiveScale < DefaultScale;

    // 注册硬暂停请求（timeScale=0）。引用计数 +1。
    // 任何来源（Esc 暂停、决死、时停、GameOver、FinalUI 等）都调这个。
    public static void RegisterHardPause()
    {
        hardPauseRefCount++;
        // 异常检测：通常同一来源只会 +1，超过阈值说明某处有重复调用
        if (hardPauseRefCount > 10)
        {
            Debug.LogWarning($"[TimeScaleController] hardPauseRefCount={hardPauseRefCount} 异常偏高！" +
                             $"可能存在未成对的 Register/Unregister 调用。");
        }
        Apply();
    }

    // 释放硬暂停请求。引用计数 -1。计数到 0 才真正恢复。
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

    // 注册软缩放入栈（如 Boss 慢放 0.3f）。
    // 如果此时有硬暂停，软缩放会被覆盖为 0；硬暂停解除后自动回到这个值。
    public static void RegisterSoftScale(float scale)
    {
        softScaleStack.Push(scale);
        Apply();
    }

    // 释放最近一次软缩放。出栈后自动恢复到上一个缩放值或 1f。
    public static void UnregisterSoftScale()
    {
        if (softScaleStack.Count > 0)
        {
            softScaleStack.Pop();
        }
        Apply();
    }

    // 清空所有暂停/缩放请求，强制恢复 timeScale=1。
    // 场景切换、游戏重置时调用。
    public static void ResetAll()
    {
        hardPauseRefCount = 0;
        softScaleStack.Clear();
        Time.timeScale = DefaultScale;
    }

    private static void Apply()
    {
        float target = CurrentEffectiveScale;
        if (!Mathf.Approximately(Time.timeScale, target))
        {
            Time.timeScale = target;
        }
    }
}
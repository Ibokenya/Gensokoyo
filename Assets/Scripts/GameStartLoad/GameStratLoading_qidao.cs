using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStratLoading_qidao : MonoBehaviour
{
    public TextMeshProUGUI T;

    // 各通道初始方向配置（0=减，1=增）
    private int _rFlag = 0;
    private int _gFlag = 0;
    private int _bFlag = 0;
    private int _aFlag = 0;

    // 步长配置（可独立调整每个通道的速度）
    private readonly float _rStep = 1f / 255f;
    private readonly float _gStep = 1f / 255f;
    private readonly float _bStep = 1f / 255f;
    private readonly float _aStep = 0.8f / 255f;

    void Update()
    {
        UpdateTextColor();
    }

    // 核心方法：统一处理颜色循环
    void UpdateTextColor()
    {
        Color color = T.color;

        // 调用通用方法处理每个通道，自动更新值和方向flag
        color.r = CycleColorChannel(color.r, ref _rFlag, _rStep);
        color.g = CycleColorChannel(color.g, ref _gFlag, _gStep);
        color.b = CycleColorChannel(color.b, ref _bFlag, _bStep);
        color.a = CycleColorChannel(color.a, ref _aFlag, _aStep);

        T.color = color;
    }

    // 参数：currentValue=当前值，flag=方向标记（引用传递），step=变化步长
    private float CycleColorChannel(float currentValue, ref int flag, float step)
    {
        if (flag == 0) // 减少
        {
            currentValue -= step;
            if (currentValue <= 0)
            {
                currentValue = 0f;
                flag = 1; // 切换为增加
            }
        }
        else // 增加
        {
            currentValue += step;
            if (currentValue >= 1)
            {
                currentValue = 1f;
                flag = 0; // 切换为减少
            }
        }
        return Mathf.Clamp01(currentValue); // 最终边界保护
    }
}

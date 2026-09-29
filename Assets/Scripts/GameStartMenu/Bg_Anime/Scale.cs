using UnityEngine;

/// <summary>
/// 单向脉冲式缩放 —— 从初始大小扩散到 maxScale 后瞬间重置重来（模拟月亮光晕一圈圈扩散）。
/// 保留原始设计意图 + 加速度逐渐减速，修复：
///   1. 加 Time.deltaTime 保证帧率无关（编辑器/打包速度一致）
///   2. 钳制 TrueSpeed 防负值（防止"越缩越快永不重置"的 bug）
///   3. 修复 .transform.transform 笔误
///   4. localScale 保持 z=1（防 2D 精灵消失）
/// </summary>
public class Scale : MonoBehaviour
{
    [Header("引用")]
    public GameObject myself; // 要缩放的物体

    [Header("缩放设置（每秒单位）")]
    public float ScaleSpeed = 1.2f;        // 初始缩放增长速度（每秒增加多少 scale）
    public float Scale_a = -0.003f;        // 加速度（负值=逐渐减速，让外圈扩散越来越慢）
    public float resetThreshold = 4f;      // 超过这个 scale 就归零重来
    public float initialScale = 0f;        // 重置时的初始 scale（0=从点开始扩散）

    private float TrueSpeed;

    void Reset()
    {
        if (myself == null) myself = this.gameObject;
    }

    void Start()
    {
        if (myself == null) myself = this.gameObject;
        TrueSpeed = ScaleSpeed;
    }

    void Update()
    {
        if (myself == null) myself = this.gameObject;

        // ✅ 关键修复：两处都乘 Time.deltaTime，帧率无关
        myself.transform.localScale += new Vector3(TrueSpeed, TrueSpeed, 0f) * Time.deltaTime;

        // ✅ 加速度也乘 deltaTime
        TrueSpeed += Scale_a * Time.deltaTime;

        // ✅ 钳制 TrueSpeed 到 0 以上，防止变成负后越缩越快永不重置
        if (TrueSpeed < 0f) TrueSpeed = 0f;

        // ✅ 扩散到阈值后重置（修复原代码 .transform.transform 笔误）
        if (myself.transform.localScale.x >= resetThreshold)
        {
            myself.transform.localScale = new Vector3(initialScale, initialScale, 1f);
            TrueSpeed = ScaleSpeed;
        }
    }
}

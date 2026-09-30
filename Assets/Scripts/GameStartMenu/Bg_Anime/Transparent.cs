using UnityEngine;
using UnityEngine.UI;

// 透明度呼吸动画 —— 菜单场景背景装饰用
// alpha 在初始值和 0 之间平滑往返。
// 加 Time.deltaTime 保证帧率无关。
public class Transparent : MonoBehaviour
{
    [Header("引用")]
    public Image targetImage; // 要变透明的 Image（若为空则从自身取）

    [Header("透明度设置")]
    public float minAlpha = 0f;      // 最小透明度（0=完全透明）
    public float maxAlpha = 1f;      // 最大透明度
    public float cycleSeconds = 3f;  // 一次完整呼吸的秒数

    private float _time;
    private Color _baseColor; // 颜色的 RGB 部分缓存，只改 alpha

    void Reset()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    void Start()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (targetImage != null) _baseColor = targetImage.color;
    }

    void Update()
    {
        if (targetImage == null) return;

        _time += Time.deltaTime;

        // 正弦波做平滑往返
        float t = (Mathf.Sin(_time / cycleSeconds * Mathf.PI * 2f) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = _baseColor;
        c.a = alpha;
        targetImage.color = c;
    }
}
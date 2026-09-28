using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas 16:9 适配脚本 —— 让 UI 始终在 1920×1080 参考区域内，屏幕多余区域用黑条 Image 盖住。
///
/// 核心原理：
///   ScaleWithScreenSize + matchWidthOrHeight 动态选值 → Canvas 物理填满屏幕但逻辑坐标系变化。
///   宽屏时 Canvas 逻辑变宽（比如 1920×720 屏幕下 Canvas=2880×1080 逻辑像素），
///   16:9 参考区居中，左右多出来的逻辑空间 = 黑条区域。
///   黑条 Image 用 STRETCH ANCHOR 直接盖住黑条区域（比单点 anchor+sizeDelta+pivot 更简单、零偏移）。
///
/// 关于 Canvas 逻辑尺寸变大：
///   比如 1920×720 宽屏 → scaleFactor=0.667 → Canvas 逻辑 = 2880×1080。
///   这是 ScaleWithScreenSize 的正常行为，不是 bug。UI 元素设计在 0~1920 逻辑宽范围内
///   （参考区域），实际渲染时 CanvasScaler 自动 scale，不会错位。你不需要改任何现有 UI 元素的尺寸。
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasLetterbox : MonoBehaviour
{
    [Header("参考分辨率")]
    public const int RefW = 1920;
    public const int RefH = 1080;

    [Header("黑条颜色")]
    public Color barColor = Color.black;

    private CanvasScaler scaler;
    private Image barLeft, barRight, barTop, barBottom;

    private void Awake()
    {
        EnsureScaler();
        EnsureBars();
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    private void EnsureScaler()
    {
        scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(RefW, RefH);
    }

    private void EnsureBars()
    {
        if (barLeft   == null) barLeft   = CreateBar("__BlackBar_Left");
        if (barRight  == null) barRight  = CreateBar("__BlackBar_Right");
        if (barTop    == null) barTop    = CreateBar("__BlackBar_Top");
        if (barBottom == null) barBottom = CreateBar("__BlackBar_Bottom");
    }

    private Image CreateBar(string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling(); // 最底层，确保在所有 UI 下面
        var img = go.GetComponent<Image>();
        img.color = barColor;
        img.raycastTarget = false;
        return img;
    }

    private void Apply()
    {
        if (scaler == null) EnsureScaler();
        if (barLeft == null) EnsureBars();

        float refAspect = (float)RefW / RefH;
        float scrAspect = (float)Screen.width / Screen.height;
        bool wider = scrAspect > refAspect;

        // 动态 match：宽屏→match=1（按高度等比，留左右黑）；窄屏→match=0（按宽度等比，留上下黑）
        scaler.matchWidthOrHeight = wider ? 1f : 0f;

        // scaleFactor = Min(screenW/refW, screenH/refH)
        float sf = Mathf.Lerp(
            (float)Screen.width  / RefW,
            (float)Screen.height / RefH,
            scaler.matchWidthOrHeight
        );
        if (sf <= 0f) sf = 1f;

        // Canvas 当前逻辑尺寸（由 ScaleWithScreenSize + match 决定）
        float canvasLogicalW = Screen.width  / sf;
        float canvasLogicalH = Screen.height / sf;

        // 先全隐藏
        barLeft.gameObject.SetActive(false);
        barRight.gameObject.SetActive(false);
        barTop.gameObject.SetActive(false);
        barBottom.gameObject.SetActive(false);

        if (wider)
        {
            // 宽屏 → 左右黑条
            // Canvas 逻辑宽 > RefW，多出的空间在 RefW 两侧（居中）
            // 黑条覆盖 Canvas 逻辑 x ∈ [0, leftEdge) 和 (rightEdge, canvasLogicalW]
            float leftBarWidthLogical  = (canvasLogicalW - RefW) / 2f;
            float rightBarWidthLogical = (canvasLogicalW - RefW) / 2f;
            // 归一化锚点（anchorMin/Max 是 Canvas RectTransform 的相对坐标 0~1）
            float leftBarNorm  = leftBarWidthLogical  / canvasLogicalW;
            float rightBarNorm = rightBarWidthLogical / canvasLogicalW;

            // 左黑条：stretch Canvas 左边缘 → 16:9 左界
            ConfigureBar(barLeft,   anchorMinX: 0f,               anchorMaxX: leftBarNorm);
            // 右黑条：stretch 16:9 右界 → Canvas 右边缘
            ConfigureBar(barRight,  anchorMinX: 1f - rightBarNorm, anchorMaxX: 1f);
        }
        else
        {
            // 窄屏 → 上下黑条
            float bottomBarHeightLogical = (canvasLogicalH - RefH) / 2f;
            float topBarHeightLogical    = (canvasLogicalH - RefH) / 2f;
            float bottomBarNorm = bottomBarHeightLogical / canvasLogicalH;
            float topBarNorm    = topBarHeightLogical    / canvasLogicalH;

            // 下黑条：stretch Canvas 下边缘 → 16:9 下界
            ConfigureBar(barBottom, anchorMinY: 0f,              anchorMaxY: bottomBarNorm);
            // 上黑条：stretch 16:9 上界 → Canvas 上边缘
            ConfigureBar(barTop,    anchorMinY: 1f - topBarNorm, anchorMaxY: 1f);
        }
    }

    /// <summary>
    /// 配置一条黑条 —— 用 STRETCH ANCHOR 直接定义覆盖区域。
    /// 黑条永远纵向（anchorMin/Max Y）和横向（anchorMin/Max X）都 stretch 满对应方向。
    /// 这种方式零偏移，不需要管 pivot（anchor 直接定义矩形四个角）。
    /// </summary>
    private void ConfigureBar(Image bar,
                              float anchorMinX = 0f, float anchorMaxX = 1f,
                              float anchorMinY = 0f, float anchorMaxY = 1f)
    {
        var rt = bar.rectTransform;
        bar.gameObject.SetActive(true);
        rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
        rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        // sizeDelta 和 offset 都保持 0 —— stretch anchor 下它们没用
        rt.sizeDelta = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}

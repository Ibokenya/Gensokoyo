using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas 16:9 适配脚本 —— ScaleWithScreenSize + matchWidthOrHeight 动态选值
///
/// 核心原理：
///   宽屏时 Canvas 逻辑变宽，窄屏时 Canvas 逻辑变高。
///   所有 UI 元素设计在 0~RefW / 0~RefH 参考区域内，
///   CanvasScaler 自动 scale 到真实屏幕像素。
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasLetterbox : MonoBehaviour
{
    [Header("参考分辨率")]
    public const int RefW = 1920;
    public const int RefH = 1080;

    private CanvasScaler scaler;

    private void Awake()
    {
        EnsureScaler();
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

    private void Apply()
    {
        if (scaler == null) EnsureScaler();

        float refAspect = (float)RefW / RefH;
        float scrAspect = (float)Screen.width / Screen.height;
        bool wider = scrAspect > refAspect;

        // 动态 match：宽屏→match=1（按高度等比）；窄屏→match=0（按宽度等比）
        scaler.matchWidthOrHeight = wider ? 1f : 0f;

        // scaleFactor = Min(screenW/refW, screenH/refH)
        float sf = Mathf.Lerp(
            (float)Screen.width  / RefW,
            (float)Screen.height / RefH,
            scaler.matchWidthOrHeight
        );
        if (sf <= 0f) sf = 1f;
    }
}

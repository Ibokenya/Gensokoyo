using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏摄像机 RT 适配脚本 —— 把 2D 世界渲染到 RenderTexture，再显示到 Canvas 的 RawImage。
///
/// CanvasLetterbox 配合：
///   CanvasLetterbox 用 ScaleWithScreenSize 让 Canvas 填满屏幕，
///   创建 4 条黑条 Image 盖住 Canvas 上 16:9 参考区域之外的部分。
///   游戏画面的 RawImage 铺满整个 Canvas，AspectRatioFitter 让 RT 本身保持 16:9，
///   黑条 Image 盖住 RT 超出 Canvas 有效区的部分。
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraRTAdapter : MonoBehaviour
{
    [Header("手动赋值（Inspector 里拖）")]

    /// <summary>RenderTexture，你在 Project 里手动创建的 1920×1080 RT</summary>
    public RenderTexture targetRT;

    /// <summary>Canvas 里的 RawImage，显示 RT 用的那张</summary>
    public RawImage displayImage;

    [Header("可选参数")]
    [Tooltip("强制 Camera aspect = 16:9。关闭则保留你原来的 aspect 设置。")]
    public bool forceAspect16_9 = true;
    [Tooltip("Camera 清除背景色")]
    public Color clearColor = Color.black;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void Apply()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        // 渲染到 RT（不再到主屏幕）
        cam.targetTexture = targetRT;

        if (forceAspect16_9)
        {
            cam.aspect = 16f / 9f;
            cam.rect = new Rect(0, 0, 1, 1);
        }

        // 黑底清屏（RT 边缘是黑色）
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = clearColor;

        // 把 RT 赋给 RawImage
        if (displayImage != null && targetRT != null)
        {
            displayImage.texture = targetRT;
        }
    }

    private void OnDestroy()
    {
        if (cam != null && cam.targetTexture == targetRT)
        {
            cam.targetTexture = null;
        }
    }
}

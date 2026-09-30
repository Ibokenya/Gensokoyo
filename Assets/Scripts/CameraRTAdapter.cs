using UnityEngine;
using UnityEngine.UI;

// 游戏摄像机 RT 适配脚本 —— 把 2D 世界渲染到 RenderTexture，再显示到 Canvas 的 RawImage。
// CanvasLetterbox 配合 ScaleWithScreenSize 让 Canvas 填满屏幕，屏幕清屏色作为黑条遮罩。
// LateUpdate 兜底防止 targetTexture 被意外清除导致画面泄漏到屏幕。
[RequireComponent(typeof(Camera))]
public class CameraRTAdapter : MonoBehaviour
{
    [Header("必需引用")]
    [Tooltip("摄像机输出的 RenderTexture")]
    public RenderTexture targetRT;

    [Tooltip("Canvas 里显示 RT 的 RawImage")]
    public RawImage displayImage;

    [Header("可选参数")]
    [Tooltip("强制 Camera aspect = 16:9。")]
    public bool forceAspect16_9 = true;
    [Tooltip("Camera 清除背景色（RT 边缘颜色）")]
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

    private void LateUpdate()
    {
        if (targetRT == null) return;

        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        //确保自己挂的 Camera 没丢 targetTexture
        if (cam.targetTexture != targetRT)
        {
            cam.targetTexture = targetRT;
            if (displayImage != null && displayImage.texture != targetRT)
                displayImage.texture = targetRT;
        }

        //全局兜底：Camera.allCameras 是 Unity 内部维护的静态列表，零分配
        foreach (var c in Camera.allCameras)
        {
            if (c == cam) continue;
            if (c.targetTexture != null) continue;
            if (c.enabled) c.enabled = false;
        }
    }

    private void Apply()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        // targetRT 为 null 时不要把 targetTexture 设为 null —— 会让 Camera 直接渲染屏幕
        if (targetRT != null)
            cam.targetTexture = targetRT;

        if (forceAspect16_9)
        {
            cam.aspect = 16f / 9f;
            cam.rect = new Rect(0, 0, 1, 1);
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = clearColor;

        if (displayImage != null && targetRT != null)
            displayImage.texture = targetRT;
    }

    private void OnDestroy()
    {
        if (cam != null && cam.targetTexture == targetRT)
            cam.targetTexture = null;
    }
}
using UnityEngine;
using UnityEngine.UI;

// Loading 场景 RT 渲染配置
public class LoadingCameraRTAdapter : MonoBehaviour
{
    [Header("引用")]
    public Camera targetCamera;

    [Tooltip("RT 输出目标")]
    public RenderTexture targetRT;

    [Tooltip("Canvas 显示 RT 的 RawImage")]
    public RawImage displayImage;

    [Header("可选参数")]
    public Color clearColor = Color.black;

    private void Awake()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void Apply()
    {
        if (targetCamera == null || targetRT == null) return;

        targetCamera.enabled = true;
        targetCamera.targetTexture = targetRT;
        targetCamera.cullingMask = int.MaxValue;
        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = clearColor;

        if (displayImage != null)
            displayImage.texture = targetRT;
    }

    private void LateUpdate()
    {
        if (targetCamera == null || targetRT == null) return;
        
        if (targetCamera.targetTexture != targetRT)
            targetCamera.targetTexture = targetRT;

        if (displayImage != null && displayImage.texture != targetRT)
            displayImage.texture = targetRT;
    }
}
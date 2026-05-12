using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game2 : MonoBehaviour
{
    public GameObject Msg1;
    public GameObject Msg2;

    public Animator FinalAnime;

    public BGImageScroll starBgScroll;
    public BGImageScroll BlueStarBgScroll;

    public List<GameObject> UIobs;
    public GameObject UIImage;
    public GameObject RightUI;

    [Header("走廊物体")]
    public GameObject corridorLeft;
    public GameObject corridorRight;
    public GameObject corridorUp;
    public GameObject corridorDown;

    [Header("摄像机")]
    public Camera mainCamera;

    // 存储所有SpriteRenderer组件（父物体+子物体）
    public List<SpriteRenderer> allSpriteRenderers = new List<SpriteRenderer>();
    // 存储所有BGImageScroll组件
    public List<BGImageScroll> allBgScrolls = new List<BGImageScroll>();

    // 初始透明度
    private const float initialAlpha = 0f;
    // 目标透明度
    private const float targetAlpha = 1f;
    // 淡入时间（秒）
    private const float fadeInDuration = 3f;
    // 绘制时间（秒）
    private const float drawDuration = 5f;
    // 目标高度（左右）
    private const float targetHeight = 14f;
    // 目标高度（上下，2倍速度）
    private const float targetHeightDouble = 28f;

    void OnEnable()
    {
        // 启动10秒延迟协程
        StartCoroutine(DelayAfterActivate());
    }

    /// <summary>
    /// 启动走廊绘制和淡入效果
    /// </summary>
    public void StartCorridorEffect()
    {
        StartCoroutine(CorridorDrawAndFade());
    }

    /// <summary>
    /// 走廊绘制和淡入协程
    /// </summary>
    private IEnumerator CorridorDrawAndFade()
    {
        RotateCameraZ(30f);
        EnableBGScrolls();
        // 同时启动绘制和淡入协程
        yield return StartCoroutine(DrawCorridors());
        yield return StartCoroutine(FadeInCorridors());
        // 绘制和淡入完成后，启用所有BGImageScroll脚本
    }

    /// <summary>
    /// 绘制走廊协程（高度从0变为目标值）
    /// </summary>
    private IEnumerator DrawCorridors()
    {
        float elapsedTime = 0f;

        while (elapsedTime < drawDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / drawDuration);

            // 更新左右物体的高度（目标13）
            UpdateSpriteHeight(corridorLeft, t, targetHeight);
            UpdateSpriteHeight(corridorRight, t, targetHeight);

            // 更新上下物体的高度（目标26，2倍速度）
            UpdateSpriteHeight(corridorUp, t, targetHeightDouble);
            UpdateSpriteHeight(corridorDown, t, targetHeightDouble);

            yield return null;
        }

        // 确保最终高度正确
        UpdateSpriteHeight(corridorLeft, 1f, targetHeight);
        UpdateSpriteHeight(corridorRight, 1f, targetHeight);
        UpdateSpriteHeight(corridorUp, 1f, targetHeightDouble);
        UpdateSpriteHeight(corridorDown, 1f, targetHeightDouble);
    }

    /// <summary>
    /// 更新物体及其子物体的SpriteRenderer高度
    /// </summary>
    private void UpdateSpriteHeight(GameObject parent, float t, float target)
    {
        if (parent == null) return;

        // 更新父物体的SpriteRenderer
        SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Vector2 size = sr.size;
            size.y = Mathf.Lerp(0f, target, t);
            sr.size = size;
        }

        // 更新所有子物体的SpriteRenderer
        SpriteRenderer[] childSRs = parent.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer childSR in childSRs)
        {
            Vector2 size = childSR.size;
            size.y = Mathf.Lerp(0f, target, t);
            childSR.size = size;
        }
    }

    /// <summary>
    /// 淡入走廊协程（透明度从0变为1）
    /// </summary>
    private IEnumerator FadeInCorridors()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeInDuration);
            float currentAlpha = Mathf.Lerp(initialAlpha, targetAlpha, t);

            // 更新所有SpriteRenderer的透明度
            foreach (SpriteRenderer sr in allSpriteRenderers)
            {
                if (sr != null)
                {
                    Color color = sr.color;
                    color.a = currentAlpha;
                    sr.color = color;
                }
            }

            yield return null;
        }

        // 确保最终透明度为1
        foreach (SpriteRenderer sr in allSpriteRenderers)
        {
            if (sr != null)
            {
                Color color = sr.color;
                color.a = targetAlpha;
                sr.color = color;
            }
        }
    }

    /// <summary>
    /// 启用所有BGImageScroll脚本
    /// </summary>
    private void EnableBGScrolls()
    {
        foreach (BGImageScroll scroll in allBgScrolls)
        {
            if (scroll != null)
            {
                scroll.enabled = true;
            }
        }
    }

    /// <summary>
    /// 让摄像机沿Z轴旋转的方法
    /// </summary>
    /// <param name="Speed">旋转速度</param>
    public void RotateCameraZ(float Speed)
    {
        StartCoroutine(RotateCameraZCoroutine(Speed));
    }

    /// <summary>
    /// 摄像机Z轴旋转协程
    /// </summary>
    private IEnumerator RotateCameraZCoroutine(float Speed)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not assigned!");
            yield break;
        }
        while (true)
        {
            mainCamera.transform.Rotate(Vector3.forward, Speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator DelayAfterActivate()
    {
        yield return new WaitForSeconds(10f);

        // 禁用Msg1和Msg2
        if (Msg1 != null)
        {
            Msg1.SetActive(false);
        }
        if (Msg2 != null)
        {
            Msg2.SetActive(false);
        }

        // 设置Animator的IsAnime为true
        if (FinalAnime != null)
        {
            FinalAnime.SetBool("IsAnime", true);
            Invoke("FadeOut", 40f);
        }
    }

    public void StartBgMove()
    {
        // 开启Star的纹理偏移
        if (starBgScroll != null)
        {
            starBgScroll.enabled = true;
        }
        if (BlueStarBgScroll != null)
        {
            BlueStarBgScroll.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 检测R键或ESC键按下，返回菜单界面
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReturnToMenu();
        }
    }

    /// <summary>
    /// 返回菜单界面
    /// </summary>
    private void ReturnToMenu()
    {
        // 回收所有敌人
        if (Global_GameManager.Instance != null)
        {
            Global_GameManager.Instance.RecycleAllEnemies();
        }
        
        // 切换到菜单场景
        Global_SceneManager.Instance.IntoNextScene("GameStartMenu", false);
    }

    private void FadeOut()
    {
        Global_AudioManager.Instance.FadeOutMusic(10f);
        StartCoroutine(FadeOutBg());
    }

    private IEnumerator FadeOutBg()
    {
        float duration = 12f;
        float elapsedTime = 0f;
        foreach (var item in UIobs)
        {
            item.SetActive(false);
        }
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            Color color = UIImage.GetComponent<Image>().color;
            color.a = 1 - elapsedTime / duration;
            UIImage.GetComponent<Image>().color = color;

            color = RightUI.GetComponent<Image>().color;
            color.a = 1 - elapsedTime / duration;
            RightUI.GetComponent<Image>().color = color;

            yield return null;
        }
        UIImage.SetActive(false);
        RightUI.SetActive(false);
        Global_AudioManager.Instance.PlayBGM("Ending");
        StartBgMove();
        Invoke("StartCorridorEffect",4f);
    }
}

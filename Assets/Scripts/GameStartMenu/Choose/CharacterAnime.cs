using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReplaySystem;

public class CharacterAnime : MonoBehaviour
{
    public GameObject Reimu;
    public GameObject Marisa;

    public Animator reimuAnimator;
    public Animator marisaAnimator;

    private bool ismirror = false;
    private bool isfirst = true;

    public GameObject Logo;
    private RectTransform logoRectTransform;

    [Header("音效设置")]
    [SerializeField] private AudioClip moveoffSound;   // 取消选中音效

    // Logo 在 CanvasScaler 参考坐标系 (1920x1080) 下的 anchoredPosition
    // 锚点 stretch(0,0)-(1,1)，锚框中心 (960, 540)
    // 右上角 worldPos(1720, 980) = 锚框中心 + anchoredPos(760, 440)
    // 左上角 worldPos(200, 980)  = 锚框中心 + anchoredPos(-760, 440)
    private static readonly Vector2 LogoPosRight = new Vector2(760, 440);
    private static readonly Vector2 LogoPosLeft  = new Vector2(-760, 440);

    void Awake()
    {
        if (Logo != null)
            logoRectTransform = Logo.GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        ResetToDefault();
    }

    void ResetToDefault()
    {
        ismirror = false;
        isfirst = true;
        Global_GameManager.Instance.character = Character.Reimu;

        // 重置动画状态
        if (reimuAnimator != null)
        {
            reimuAnimator.SetBool("IsFirst", true);
            reimuAnimator.SetBool("IsMirror", false);
        }
        if (marisaAnimator != null)
        {
            marisaAnimator.SetBool("IsFirst", true);
            marisaAnimator.SetBool("IsMirror", false);
        }

        // 重置组件
        if (Reimu != null)
        {
            Reimu.transform.Find("模糊").gameObject.SetActive(false);
            Reimu.transform.Find("灵梦简介").gameObject.SetActive(true);
        }
        if (Marisa != null)
        {
            Marisa.transform.Find("模糊").gameObject.SetActive(true);
            Marisa.transform.Find("魔理沙简介").gameObject.SetActive(false);
        }
        SetLogoPositionRight();
    }

    void Update()
    {
        // 右键：切镜像态
        if (Input.GetKeyDown(PhysicalKeyMapping.Right)&&!ismirror)
        {
            if (isfirst) 
            { 
                isfirst = false;
                reimuAnimator.SetBool("IsFirst", false);
                marisaAnimator.SetBool("IsFirst", false);
            }
            SetMirrorState(true);
            ismirror = true;
            Reimu.transform.Find("模糊").gameObject.SetActive(true);
            Reimu.transform.Find("灵梦简介").gameObject.SetActive(false);
            Marisa.transform.Find("模糊").gameObject.SetActive(false);
            Marisa.transform.Find("魔理沙简介").gameObject.SetActive(true);
            SetLogoPositionLeft();
            Global_GameManager.Instance.character = Character.Marisa;

            // 播放取消选中音效
            if (moveoffSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(moveoffSound, false);
            }
        }
        // 左键：切初始态
        else if (Input.GetKeyDown(PhysicalKeyMapping.Left)&&ismirror)
        {
            SetMirrorState(false);
            ismirror = false;
            Reimu.transform.Find("模糊").gameObject.SetActive(false);
            Reimu.transform.Find("灵梦简介").gameObject.SetActive(true);
            Marisa.transform.Find("模糊").gameObject.SetActive(true);
            Marisa.transform.Find("魔理沙简介").gameObject.SetActive(false);
            SetLogoPositionRight();
            Global_GameManager.Instance.character = Character.Reimu;

            // 播放取消选中音效
            if (moveoffSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(moveoffSound, false);
            }
        }
    }

    // 核心：设置镜像状态 + 防重复触发
    void SetMirrorState(bool isMirror)
    {
        // 触发动画
        reimuAnimator.SetBool("IsMirror", isMirror);
        marisaAnimator.SetBool("IsMirror", isMirror);
    }

    void SetLogoPositionRight()
    {
        if (logoRectTransform != null)
            logoRectTransform.anchoredPosition = LogoPosRight;
    }

    void SetLogoPositionLeft()
    {
        if (logoRectTransform != null)
            logoRectTransform.anchoredPosition = LogoPosLeft;
    }
}
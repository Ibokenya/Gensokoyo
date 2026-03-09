using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterAnime : MonoBehaviour
{
    public GameObject Reimu;
    public GameObject Marisa;

    public Animator reimuAnimator;
    public Animator marisaAnimator;

    private bool ismirror = false;
    private bool isfirst = true;

    [Header("动画播放间隔")]
    public float animDuration = 0.5f;

    private bool canClick = true;

    public GameObject Logo;

    [Header("音效设置")]
    [SerializeField] private AudioClip moveoffSound;   // 取消选中音效


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
            Reimu.transform.Find("模糊")?.gameObject.SetActive(false);
            Reimu.transform.Find("灵梦简介")?.gameObject.SetActive(true);
        }
        if (Marisa != null)
        {
            Marisa.transform.Find("模糊")?.gameObject.SetActive(true);
            Marisa.transform.Find("灵梦简介")?.gameObject.SetActive(false);
        }
        Logo.transform.position = new Vector3(1720, 980, 0);
        canClick = true;
    }

    void Update()
    {
        // 动画播放中不响应按键
        if (!canClick) return;

        // 右键：切镜像态
        if (Input.GetKeyDown(KeyCode.RightArrow)&&!ismirror)
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
            Logo.transform.position = new Vector3(200, 980, 0);
            Global_GameManager.Instance.character = Character.Marisa;

            // 播放取消选中音效
            if (moveoffSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(moveoffSound, false);
            }
        }
        // 左键：切初始态
        else if (Input.GetKeyDown(KeyCode.LeftArrow)&&ismirror)
        {
            SetMirrorState(false);
            ismirror = false;
            Reimu.transform.Find("模糊").gameObject.SetActive(false);
            Reimu.transform.Find("灵梦简介").gameObject.SetActive(true);
            Marisa.transform.Find("模糊").gameObject.SetActive(true);
            Marisa.transform.Find("魔理沙简介").gameObject.SetActive(false);
            Logo.transform.position = new Vector3(1720, 980, 0);
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
        // 锁定按键
        canClick = false;

        // 触发动画
        reimuAnimator.SetBool("IsMirror", isMirror);
        marisaAnimator.SetBool("IsMirror", isMirror);

        // 动画结束后解锁按键（时长必须匹配你的动画）
        Invoke("UnlockClick", animDuration);
    }

    // 解锁按键
    void UnlockClick()
    {
        canClick = true;
    }
}

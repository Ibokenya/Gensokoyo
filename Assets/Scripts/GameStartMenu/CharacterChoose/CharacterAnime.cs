using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterAnime : MonoBehaviour
{
    public Animator reimuAnimator;
    public Animator marisaAnimator;

    [Header("动画时长（秒）")]
    public float animDuration = 0.5f;

    // 仅用于防重复触发
    private bool canClick = true;

    void Update()
    {
        // 动画播放中不响应按键
        if (!canClick) return;

        // 右键：切镜像态
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SetMirrorState(true);
        }
        // 左键：切初始态
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SetMirrorState(false);
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

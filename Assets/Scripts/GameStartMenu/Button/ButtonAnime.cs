using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnime : MonoBehaviour
{
    private TMP_Text text;
    [Header("颜色差（选中时增加颜色差数值）")]
    [SerializeField]
    private Color color = new Color(0.2f, 0f, 0.4f,0f);
    [Header("字体差（选中时增加字体差数值）")]
    [SerializeField]
    private int font=20;
    
    [Header("音效设置")]
    [SerializeField] private AudioClip moveoffSound;   // 取消选中音效
    [SerializeField] private AudioClip clickSound;      // 点击音效

    // 当按钮被选中时
    public void ButtonBeChoose(Button button)
    {
        text=button.GetComponentInChildren<TMP_Text>();
        text.fontSize += font;
        text.color += color;
    }

    // 当按钮被取消选中时，必须恢复原状
    public void ButtonBeMoveoff(Button button)
    {
        text = button.GetComponentInChildren<TMP_Text>();
        text.fontSize -= font;
        text.color -= color;

        // 播放取消选中音效
        if (moveoffSound != null)
        {
            Global_AudioManager.Instance.PlaySFX(moveoffSound, false);
        }
    }

    // 按钮被选择时（按下了Z键），不过这时候也没什么动画可做的，但，还有音效不是吗？
    public void ButtonBeClick(Button button)
    {
        // 播放点击音效
        if (clickSound != null)
        {
            Global_AudioManager.Instance.PlaySFX(clickSound, false);
        }
    }
}
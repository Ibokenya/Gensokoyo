using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnime : MonoBehaviour
{
    private TMP_Text text;
    private Color NewColor = new Color(1f, 0.2f, 0.9f);
    private Color OldColor = new Color(0.8f, 0.2f, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 当按钮被选中时
    /// </summary>
    /// <param name="button">将被选中的按钮传递进来</param>
    public void ButtonBeChoose(Button button)
    {
        Debug.Log($"按钮{button.name}被选中");
        text=button.GetComponentInChildren<TMP_Text>();
        text.fontSize += 20;
        text.color = NewColor;
    }

    /// <summary>
    /// 当按钮被取消选中时，必须恢复原状
    /// </summary>
    /// <param name="button">被移除选中状态的按钮</param>
    public void ButtonBeMoveoff(Button button)
    {
        Debug.Log($"按钮{button.name}被抛弃了");
        text = button.GetComponentInChildren<TMP_Text>();
        text.fontSize -= 20;
        text.color = OldColor;
    }

    /// <summary>
    /// 按钮被选择时（按下了Z键），不过这时候也没什么动画可做的，但，还有音效不是吗？
    /// </summary>
    /// <param name="button">被按下的按钮</param>
    public void ButtonBeClick(Button button)
    {
        Debug.Log($"按钮{button.name}被按下，biu~");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [Header("按钮列表（自上到下）")]
    public List<Button> buttons = new List<Button>();
    [Header("默认选中的按钮索引（默认第一个）")]
    public int defaultButtonIndex = 0;
    [Header("按钮动画脚本引用")]
    public ButtonAnime ButtonAnime;
    [Header("按钮事件脚本引用（Button&Scene物体上）")]
    public ButtonEvent buttonEvent;

    private int currentButtonIndex;

    // Start is called before the first frame update
    void Start()
    {
        if(buttons.Count == 0)
        {
            Debug.Log("按钮列表为空！");
        }
        if(ButtonAnime == null)
        {
            Debug.Log("没有引用按钮动画组件（脚本）");
        }
        if(buttonEvent == null)
        {
            Debug.Log("没有引用按钮事件组件（脚本），尝试自动查找...");
            // 尝试在父物体或同层级查找ButtonEvent
            buttonEvent = GetComponentInParent<ButtonEvent>();
            if(buttonEvent == null)
            {
                buttonEvent = FindObjectOfType<ButtonEvent>();
            }
        }
        currentButtonIndex = defaultButtonIndex;
        // 令默认按钮被选中
        ButtonAnime.ButtonBeChoose(buttons[currentButtonIndex]);
    }

    // Update is called once per frame
    void Update()
    {
        // 检测上下键切换按钮（GetKeyDown确保只触发一次）
        CheckButtonSwitch();
        // 检测Z键模拟点击选中的按钮
        CheckButtonClick();
    }

    /// <summary>
    /// 检测按钮选择的方法，每一帧都监听是否按下上下键切换选中按钮，允许按住一直切换
    /// </summary>
    private void CheckButtonSwitch()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))// 按下↑键
        {
            ButtonAnime.ButtonBeMoveoff(buttons[currentButtonIndex]);
            currentButtonIndex--;
            if(currentButtonIndex<0)// 最上方循环到底层
            {
                currentButtonIndex = buttons.Count - 1;
            }
            ButtonAnime.ButtonBeChoose(buttons[currentButtonIndex]);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))// 按下↓键
        {
            ButtonAnime.ButtonBeMoveoff(buttons[currentButtonIndex]);
            currentButtonIndex++;
            if (currentButtonIndex > buttons.Count - 1)// 最下方循环到顶层
            {
                currentButtonIndex = 0;
            }
            ButtonAnime.ButtonBeChoose(buttons[currentButtonIndex]);
        }
    }

    /// <summary>
    /// 主要是检测Z键，按下的时候根据当前选中的按钮执行对应的按下逻辑
    /// 还有X键，X键默认选中退出按钮
    /// </summary>
    private void CheckButtonClick()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            ButtonAnime.ButtonBeClick(buttons[currentButtonIndex]);
            
            // 检查buttonEvent是否为空
            if(buttonEvent == null)
            {
                Debug.LogError("ButtonEvent未引用，无法执行按钮事件！");
                return;
            }
            
            switch(currentButtonIndex)
            {
                case 0: // Start按钮
                    buttonEvent.Strat_Event();
                    break;
                case 1: // ExStart按钮
                    buttonEvent.ExStart_Event();
                    break;
                case 2: // Result按钮
                    buttonEvent.Result_Event();
                    break;
                case 3: // Manual按钮
                    buttonEvent.Manual_Event();
                    break;
                case 4: // MusicRoom按钮
                    buttonEvent.MusicRoom_Event();
                    break;
                case 5: // Option按钮
                    buttonEvent.Option_Event();
                    break;
                case 6: // Quit按钮
                    buttonEvent.Quit_Event();
                    break;
                default:
                    Debug.LogWarning($"未处理的按钮索引：{currentButtonIndex}");
                    break;
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ButtonAnime.ButtonBeMoveoff(buttons[currentButtonIndex]);
            currentButtonIndex = buttons.Count - 1;
            ButtonAnime.ButtonBeChoose(buttons[currentButtonIndex]);
        }
    }
}

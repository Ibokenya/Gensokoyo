using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 此状态应该在ModeChoose状态下
/// </summary>
public class ModeChooseButton : MonoBehaviour
{
    [Header("管理四个难度按钮")]
    public List<Button> buttons;

    public int defaultButtonIndex;
    private int currentButtonIndex;

    public ButtonAnime ButtonAnime;
    public ButtonEvent buttonEvent;

    public TextMeshProUGUI text;

    private void OnEnable()
    {
        currentButtonIndex = defaultButtonIndex;
        ButtonAnime.ButtonBeChoose(buttons[currentButtonIndex]);
        text.color = buttons[currentButtonIndex].GetComponentInChildren<TMP_Text>().color;
        text.text =
                    "Easy 难度\r\n也就是最简单，轻松无比的那种\r\n一般来讲适合想要完整通关的家伙们\r\n" +
                    "然而也有过不去的\r\n那样可真没什么好说的了\r\n\r\n" +
                    "怎么办呢？\r\n那就只能多加练习好好努力了\r\n祝武运昌隆";
    }
    private void OnDisable()
    {
        ButtonAnime.ButtonBeMoveoff(buttons[currentButtonIndex]);
        currentButtonIndex = defaultButtonIndex;
    }
    // Update is called once per frame
    void Update()
    {
        CheckButtonSwitch();
        CheckButtonClick();
    }

    private void CheckButtonSwitch()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))// 按下↑键
        {
            ButtonAnime.ButtonBeMoveoff(buttons[currentButtonIndex]);
            currentButtonIndex--;
            if (currentButtonIndex < 0)// 最上方循环到底层
            {
                currentButtonIndex = buttons.Count - 1;
            }
            ButtonAnime.ButtonBeChoose(buttons[currentButtonIndex]);
            ChangeText(currentButtonIndex);
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
            ChangeText(currentButtonIndex);
        }
    }

    private void CheckButtonClick()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(currentButtonIndex>=0&&currentButtonIndex<=3)
            {
                Global_GameManager.Instance.gameMode = (GameMode)currentButtonIndex;
                buttonEvent.StartPharse2();
                ButtonAnime.ButtonBeClick(buttons[currentButtonIndex]);
            }
            else
            {
                Debug.LogWarning("为什么会在难度选择界面之后还是Ex难度啊？");
            }
        }
    }

    /// <summary>
    /// 按照当前选中按钮的索引改变描述文本的内容
    /// </summary>
    /// <param name="index"></param>
    private void ChangeText(int index)
    {
        text.color = buttons[currentButtonIndex].GetComponentInChildren<TMP_Text>().color;// 先变色
        switch (index)
        {
            case 0:// Easy
                text.text =
                    "Easy 难度\r\n也就是最简单，轻松无比的那种\r\n一般来讲适合想要完整通关的家伙们\r\n" +
                    "然而也有过不去的\r\n这种可真没什么好说的了\r\n\r\n" +
                    "怎么办呢？\r\n那就只能多加练习好好努力了\r\n祝武运昌隆";
                break;
            case 1:// Normal
                text.text = "Normal 难度\r\n很标准的这种游戏的难度\r\n所谓“中庸之道”\r\n" +
                    "就是在最难和最简单之间折中\r\n像汉堡或者三明治一样\r\n" +
                    "中间的肉饼才最美味对吧\r\n\r\n所以可以尽情享受了\r\n需要帮助的话\r\n似乎也没什么办法？";
                break;
            case 2:// Hard
                text.text = "Hard 难度\r\n糟糕，有点吃力了\r\n如果不集中精神的话。。。\r\n" +
                    "深邃的宇宙里无疑是有不可名状存在的\r\n你现在要对上廷达罗斯猎犬了\r\n" +
                    "好在还不是旧日支配者的程度\r\n\r\n为勇气献上赞歌！\r\nViva Laghent'an!";
                break;
            case 3:// Lunatic
                text.text = "Lunatic 难度\r\n咚。咚。 。咚 。 。\r\n你看到一轮圆月升起\r\n" +
                    "这宇宙中有多少星体\r\n多少生命\r\n你不清楚\r\n但这轮月亮显然与你以往所见不同\r\n" +
                    "这巨大的变化\r\n足以彻底地击溃了你\r\n你死在了虚无与错乱中\r\n这就是，“月狂”。";
                break;
            default:// 按理不该有这选项的
                Debug.LogWarning("在选难度界面出现了意料之外的选项索引");
                break;
        }
    }
}

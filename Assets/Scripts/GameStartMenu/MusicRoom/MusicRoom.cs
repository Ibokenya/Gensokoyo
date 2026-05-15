using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MusicRoom : MonoBehaviour
{
    List<string> MusicDescriptions;
    List<Color> MusicColors;
    public List<GameObject> MusicButtons;
    public GameObject MusicDescription;
    private int currentindex;

    private void Awake()
    {
        MusicDescriptions = new List<string>()
        {
            "就如曲子名那样，欢迎来到月面\n"+
            "但是，在无垠的宇宙中真的还能看到月亮所在吗？\n"+
            "月亮说到底也只是地球的卫星罢了",
            "魔法呢，就是Magic，即魔术“\n"+
            "在科学飞速发展之前，天文学总是和占星术之类的联系在一起\n"+
            "广阔天空中，隐藏着多少奥秘……",
            "好了，现在该对\"宴会\"做出解释了\n"+
            "一切的幕后黑手也该出现了吧\n"+
            "是……意料之外的家伙"+
            "但无论怎样都不奇怪啦",
            "咖啡哪怕只是瞬间暴露于真空中\n"+
            "也会因为过低的气压而瞬间沸腾\n"+
            "宇宙是完美的真空……还有低温\n"+
            "到底要咖啡怎么办呢？",
            "古有镜花水月之说\n"+
            "当月亮倒映在水面上时\n"+
            "或许可以看到月上的仙人、白兔\n"+
            "把月亮拖到市场上去能卖多少钱啊",
        };  
        MusicColors = new List<Color>()
        {
            new(1,1,0,0.6f),
            new(1,0,1,0.6f),
            new(0,0.5f,0.75f,0.6f),
            new(0.75f,0.5f,0.25f,0.6f),
            new(0.5f,0.5f,0.25f,0.6f)
        };
    }

    void OnEnable()
    {
        currentindex = 0;
        SelectCurrentButton(0);
    }

    void OnDisable()
    {
        RemoveCurrentButton(currentindex);
    }

    void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RemoveCurrentButton(currentindex);
            currentindex--;
            if (currentindex < 0)
            {
                currentindex = MusicDescriptions.Count - 1;
            }
            SelectCurrentButton(currentindex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            RemoveCurrentButton(currentindex);
            currentindex++;
            if (currentindex >= MusicDescriptions.Count)
            {
                currentindex = 0;
            }
            SelectCurrentButton(currentindex);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            if(Global_AudioManager.Instance != null)
            {
                Global_AudioManager.Instance.StopBGM();
                switch(currentindex)
                {
                    case 0:
                        Global_AudioManager.Instance.PlayBGM("Menu");
                        break;
                    case 1:
                        Global_AudioManager.Instance.PlayBGM("Game1");
                        break;
                    case 2:
                        Global_AudioManager.Instance.PlayBGM("Boss");
                        break;
                    case 3:
                        Global_AudioManager.Instance.PlayBGM("Over");
                        break;
                    case 4:
                        Global_AudioManager.Instance.PlayBGM("Ending");
                        break;
                }
            }
        }
       }

    private void RemoveCurrentButton(int index)
    {
        Color color = MusicColors[index];
        color.a = 0.6f;
        MusicButtons[index].GetComponent<TextMeshProUGUI>().color = color;
        MusicButtons[index].GetComponent<TextMeshProUGUI>().fontSize = 72;
    }
    private void SelectCurrentButton(int index)
    {
        Color color = MusicColors[index];
        color.a = 1;
        MusicButtons[index].GetComponent<TextMeshProUGUI>().color = color;
        MusicDescription.GetComponent<TextMeshProUGUI>().color = color;
        MusicButtons[index].GetComponent<TextMeshProUGUI>().fontSize = 84;
        Vector3 position = MusicButtons[index].transform.position;
        position.x = MusicDescription.transform.position.x;
        MusicDescription.transform.position = position;
        MusicDescription.GetComponent<TextMeshProUGUI>().text = MusicDescriptions[index];
    }
}

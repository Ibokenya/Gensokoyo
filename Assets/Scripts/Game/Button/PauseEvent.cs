using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PauseEvent : MonoBehaviour
{
    [Header("暂停界面按钮")]
    public List<TextMeshProUGUI> pauseButtons = new();
    public GameObject Really;// 存储确认键的空物体
    public TextMeshProUGUI Yes;// 确认键的文本组件
    public TextMeshProUGUI No;// 取消键的文本组件
    public PauseUI pauseUI;// 暂停界面的脚本组件引用
    public AudioClip Choose;// 选择音效
    public AudioClip Click;// 点击音效

    private float NoneAlpha = 0.3f;
    private float FullAlpha = 1f;
    private int index = 0;// 当前选中的按钮索引
    private bool isReally = false;// 是否是“确认操作”环节
    private bool YesOrNo = false;// 是或否？

    void OnEnable()
    {
        index = 0;
        BeChoose(index);
        Really.SetActive(false);
        isReally = false;
    }

    void OnDisable()
    {
        BeChooseCancel(index);
    }

    void Update()
    {
        CheckChoose();
    }

    private void CheckChoose()
    {
        if (isReally)// 是确认环节，检测左右，Z，X，ESC键
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.RightArrow))
            {
                YesOrNo = !YesOrNo;
                if(YesOrNo)// YES
                {
                    Yes.alpha = FullAlpha;
                    No.alpha = NoneAlpha;
                }
                else// NO
                {
                    Yes.alpha = NoneAlpha;
                    No.alpha = FullAlpha;
                }
            }
            else if(Input.GetKeyDown(KeyCode.Z))
            {
                if(YesOrNo)// YES
                {
                    switch(index)
                    {
                        case 0:
                            pauseUI.Resume();
                            break;
                        case 1:
                            Global_SceneManager.Instance.IntoNextScene("GameStartMenu",false);
                            // 返回游戏菜单
                            break;
                        case 2:
                            Debug.Log("说明书，暂未实现");
                            // 说明书   
                            break;
                        case 3:
                            Debug.Log("重开游戏，暂未实现");
                            // 重开游戏
                            break;
                        default:
                            break;
                    }
                }
                else// NO
                {
                    BackToPause();
                }
            }
            else if (Input.GetKeyDown(KeyCode.X)||Input.GetKeyDown(KeyCode.Escape))
            {
                BackToPause();
            }
        }
        else// 不是确认环节，检测上下，Z，X，ESC键
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                BeChooseCancel(index);
                if(index==0) index = pauseButtons.Count-1;
                else index--;
                BeChoose(index);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                BeChooseCancel(index);
                if(index==pauseButtons.Count-1) index = 0;
                else index++;
                BeChoose(index);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                BeClick(index);
            }
            else if (Input.GetKeyDown(KeyCode.X)||Input.GetKeyDown(KeyCode.Escape))
            {
                pauseUI.Resume();
            }
        }
    }

    private void BeChoose(int index)
    {
        Global_AudioManager.Instance.PlaySFX(Choose);
        pauseButtons[index].alpha = FullAlpha;
    }
    private void BeChooseCancel(int index)
    {
        pauseButtons[index].alpha = NoneAlpha;
    }
    private void BeClick(int index)// 默认不是确认环节
    {
        Global_AudioManager.Instance.PlaySFX(Click);
        isReally = true;
        Really.SetActive(true);
        MakeReally(index);
    }

    private void MakeReally(int index)
    {
        pauseButtons[index].alpha = 0f;
        Really.transform.position = pauseButtons[index].transform.position;
        YesOrNo = false;
        Yes.alpha = NoneAlpha;
        No.alpha = FullAlpha;
    }

    private void BackToPause()
    {
        BeChoose(index);
        Really.SetActive(false);
        isReally = false;
    }

}

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
    public AudioClip Stop;// 停止音效

    public GameObject pausePanel1;// 暂停界面的根物体
    public GameObject pausePanel2;// 暂停界面的根物体
    public GameObject pausePanel3;// 暂停界面的根物体

    [Header("说明书相关")]
    public List<TextMeshProUGUI> manualTexts = new();// 说明书目录
    public List<TextMeshProUGUI> manualPanels = new();// 说明书页面
    public GameObject manualPanel;// 说明书本身
    public GameObject manual;// 说明书logo
    public GameObject shadel;// 遮罩物体

    private readonly float NoneAlpha = 0.3f;
    private readonly float FullAlpha = 1f;
    private int index = 0;// 当前选中的按钮索引
    private bool isReally = false;// 是否是“确认操作”环节
    private bool YesOrNo = false;// 是或否？

    // 说明书相关变量
    private Color darkColor = new (0.5f, 0.5f, 0.5f);
    private Color lightColor = new (1f, 1f, 1f);
    private readonly float PanelAlpha = 0.7f;
    private int manualIndex = 0;
    private int lastManualIndex = 0;
    private bool isManualIndex = true;// 是否是索引页
    private bool isManualActive = false;// 说明书是否激活

    void OnEnable()
    {
        index = 0;
        BeChoose(index,true);
        Really.SetActive(false);
        isReally = false;
        
        // 初始化说明书状态
        manualIndex = 0;
        isManualIndex = true;
        isManualActive = false;
        if (manual != null)
        {
            manual.SetActive(false);
        }
        if (manualPanel != null)
        {
            manualPanel.SetActive(false);
        }
        if (shadel != null)
        {
            shadel.SetActive(false);
        }
        // 初始化说明书目录颜色
        foreach (TextMeshProUGUI text in manualTexts)
        {
            text.color = darkColor;
        }
        if (manualTexts.Count > 0)
        {
            manualTexts[0].color = lightColor;
        }
        // 初始化说明书页面透明度
        foreach (TextMeshProUGUI panel in manualPanels)
        {
            panel.alpha = 0;
        }
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
        // 说明书激活时的处理
        if (isManualActive)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                lastManualIndex = manualIndex;
                manualIndex = (manualIndex - 1 + manualTexts.Count) % manualTexts.Count;
                UpdateManual();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                lastManualIndex = manualIndex;
                manualIndex = (manualIndex + 1) % manualTexts.Count;
                UpdateManual();
            }
            else if (Input.GetKeyDown(KeyCode.Z) && isManualIndex)
            {
                // 进入页态
                Global_AudioManager.Instance.PlaySFX(Click);
                isManualIndex = false;
                shadel.SetActive(true);
                if (manual != null)
                {
                    manual.SetActive(false);
                }
                foreach (TextMeshProUGUI text in manualTexts)
                {
                    text.alpha = 0;
                }
                manualPanels[manualIndex].alpha = PanelAlpha;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                if (!isManualIndex)
                {
                    // 回退到索引态
                    isManualIndex = true;
                    shadel.SetActive(false);
                    if (manual != null)
                    {
                        manual.SetActive(true);
                    }
                    foreach (TextMeshProUGUI text in manualTexts)
                    {
                        text.alpha = 1;
                    }
                    manualPanels[manualIndex].alpha = 0;
                    // 重置目录颜色
                    foreach (TextMeshProUGUI text in manualTexts)
                    {
                        text.color = darkColor;
                    }
                    manualTexts[manualIndex].color = lightColor;
                }
                else
                {
                    // 关闭说明书，返回暂停菜单
                    isManualActive = false;
                    if (manual != null)
                    {
                        manual.SetActive(false);
                    }
                    if (manualPanel != null)
                    {
                        manualPanel.SetActive(false);
                    }
                    if (shadel != null)
                    {
                        shadel.SetActive(false);
                    }
                    pausePanel1.SetActive(true);
                    pausePanel2.SetActive(true);
                    pausePanel3.SetActive(true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 关闭说明书，返回暂停菜单
                isManualActive = false;
                if (manual != null)
                {
                    manual.SetActive(false);
                }
                if (manualPanel != null)
                {
                    manualPanel.SetActive(false);
                }
                if (shadel != null)
                {
                    shadel.SetActive(false);
                }
                pausePanel1.SetActive(true);
                pausePanel2.SetActive(true);
                pausePanel3.SetActive(true);
            }
            return;
        }

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
                            // 回收所有敌人
                            if (Global_GameManager.Instance != null)
                            {
                                Global_GameManager.Instance.RecycleAllEnemies();
                            }
                            Global_SceneManager.Instance.IntoNextScene("GameStartMenu",false);
                            // 返回游戏菜单
                            break;
                        case 2:
                            // 打开说明书
                            OpenManual();
                            break;
                        case 3:
                            // 重开游戏
                            Global_SceneManager.Instance.RestartGame();
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

    private void BeChoose(int index,bool isOnEnable = false)
    {
        if(isOnEnable)
        {
            Global_AudioManager.Instance.PlaySFX(Stop);
            return;
        }
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

    /// <summary>
    /// 打开说明书
    /// </summary>
    private void OpenManual()
    {
        isManualActive = true;
        isManualIndex = true;
        manualIndex = 0;
        lastManualIndex = 0;

        pausePanel1.SetActive(false);
        pausePanel2.SetActive(false);
        pausePanel3.SetActive(false);
        
        // 显示说明书
        if (manualPanel != null)
        {
            manualPanel.SetActive(true);
        }
        if (manual != null)
        {
            manual.SetActive(true);
        }
        if (shadel != null)
        {
            shadel.SetActive(false);
        }
        
        // 重置目录颜色
        foreach (TextMeshProUGUI text in manualTexts)
        {
            text.color = darkColor;
            text.alpha = 1;
        }
        if (manualTexts.Count > 0)
        {
            manualTexts[0].color = lightColor;
        }
        
        // 重置页面透明度
        foreach (TextMeshProUGUI panel in manualPanels)
        {
            panel.alpha = 0;
        }
    }

    /// <summary>
    /// 更新说明书状态
    /// </summary>
    private void UpdateManual()
    {
        Global_AudioManager.Instance.PlaySFX(Choose);
        if (isManualIndex)
        {
            // 索引态，更新目录颜色
            manualTexts[lastManualIndex].color = darkColor;
            manualTexts[manualIndex].color = lightColor;
        }
        else
        {
            // 页态，更新页面显示
            manualPanels[lastManualIndex].alpha = 0;
            manualPanels[manualIndex].alpha = PanelAlpha;
        }
    }

}

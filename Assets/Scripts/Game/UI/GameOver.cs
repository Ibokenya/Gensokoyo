using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI Continue;
    public TextMeshProUGUI ReStart;
    public TextMeshProUGUI Exit;
    public TextMeshProUGUI DescriptionText;
    public AudioClip SelectSound;
    public UIManager uiManager;

    private int CurrentIndex = 0;
    private string currentBGMName = "";
    private float currentBgmPosition = 0f;

    private Color DefaultColor = new Color(0.5f,0.5f,0.5f,0.5f);
    private Color SelectedColor = new Color(1,1,1,1);
    private List<TextMeshProUGUI> Options = new List<TextMeshProUGUI>();

    void OnEnable()
    {
        Time.timeScale = 0f;
        currentBGMName = Global_AudioManager.Instance.GetCurrentBGMName();
        currentBgmPosition = Global_AudioManager.Instance.GetCurrentBGMPosition();
        Global_AudioManager.Instance.StopBGM();
        CurrentIndex = 3;
        Global_GameManager.Instance.state = State.Over;
        Options.Add(Continue);
        Options.Add(ReStart);
        Options.Add(Exit);
    }

    private void Update()
    {
        if(CurrentIndex == 3)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                CurrentIndex = 0;
                SelectOption(CurrentIndex);
            }
            return;
        }
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            RemoveOptions(CurrentIndex);
            CurrentIndex--;
            if(CurrentIndex < 0)
            {
                CurrentIndex = Options.Count - 1;
            }
            SelectOption(CurrentIndex);
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            RemoveOptions(CurrentIndex);
            CurrentIndex++;
            if(CurrentIndex > Options.Count - 1)
            {
                CurrentIndex = 0;
            }
            SelectOption(CurrentIndex);
        }
        else if(Input.GetKeyDown(KeyCode.Z))
        {
            if(CurrentIndex == 0)
            {
                // 续关：恢复玩家状态并继续游戏
                ContinueGame();
            }
            else if(CurrentIndex == 1)
            {
                Global_SceneManager.Instance.RestartGame();
                Time.timeScale = 1f;
                Global_GameManager.Instance.state = State.Gaming;
                gameObject.SetActive(false);
            }
            else if(CurrentIndex == 2)
            {
                // 回收所有敌人
                if (Global_GameManager.Instance != null)
                {
                    Global_GameManager.Instance.RecycleAllEnemies();
                }
                Time.timeScale = 1f;
                Global_SceneManager.Instance.IntoNextScene("GameStartMenu",false);
                gameObject.SetActive(false);
            }
        }
    }

    private void SelectOption(int index)
    {
        Global_AudioManager.Instance.PlaySFX(SelectSound);
        Options[index].color = SelectedColor;
        SetDescription(index);
    }

    private void RemoveOptions(int index)
    {
        Options[index].color = DefaultColor;
    }

    private void SetDescription(int index)
    {
        string playername = Global_GameManager.Instance.character == Character.Reimu ? "灵梦" : "魔理沙";
        switch(index)
        {
            case 0:
                DescriptionText.text = "借助不死秘药立刻重返战场，但这种作弊行为可是不会被计入结果的";
                break;
            case 1:
                DescriptionText.text = "后来，休整完毕后的" + playername + "再度前来挑战";
                break;
            case 2:
                DescriptionText.text = playername + "太累了，就这样吧，先回去歇两天再说……至少先洗个澡换身衣服";
                break;
        }
    }

    /// <summary>
    /// 续关功能：恢复玩家状态并继续游戏
    /// </summary>
    private void ContinueGame()
    {
        Time.timeScale = 1f;
        // 设置游戏状态为Gaming
        Global_GameManager.Instance.state = State.Gaming;
        
        // 恢复玩家HP为2,0（2个完整，0个残片）
        Global_GameManager.Instance.Hp = 0;
        Global_GameManager.Instance.HpPiece = 0;
        Global_GameManager.Instance.AddLeftLife(2,0);
        
        // 恢复玩家Bomb为2,0（2个完整，0个残片）
        Global_GameManager.Instance.SetBomb(2,0);

        Global_GameManager.Instance.AddPower(100);
        Global_GameManager.Instance.AddPower(100);
        Global_GameManager.Instance.AddPower(100);
        Global_GameManager.Instance.AddPower(100);
        
        // 设置续关标记为true
        if (uiManager != null)
        {
            uiManager.isContinueGame = true;
        }
        Global_AudioManager.Instance.PlayBGM(currentBGMName);
        Global_AudioManager.Instance.SetBGMPosition(currentBgmPosition);
        Global_GameManager.Instance.ReBack();
        // 禁用GameOver UI
        gameObject.SetActive(false);
    }
}

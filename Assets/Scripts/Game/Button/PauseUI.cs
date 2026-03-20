using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    private bool isPaused = false;
    private string currentBGMName = "";
    private float currentBGMPosition = 0f;
    public GameObject PausePanel;
   void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!isPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Global_GameManager.Instance.state = State.Stop;
        
        // 记录当前BGM状态
        if(Global_AudioManager.Instance != null)
        {
            currentBGMName = Global_AudioManager.Instance.GetCurrentBGMName();
            currentBGMPosition = Global_AudioManager.Instance.GetCurrentBGMPosition();
            
            Global_AudioManager.Instance.StopBGM();
            Global_AudioManager.Instance.StopAllSFX();
        }
        
        PausePanel.SetActive(true);
        
    }

    public void Resume()
    {
        Global_GameManager.Instance.state = State.Gaming;

        isPaused = false;
        Time.timeScale = 1;
        
        // 恢复播放之前的BGM
        if(Global_AudioManager.Instance != null && !string.IsNullOrEmpty(currentBGMName))
        {
            Global_AudioManager.Instance.PlayBGM(currentBGMName);
            Global_AudioManager.Instance.SetBGMPosition(currentBGMPosition);
        }
        
        PausePanel.SetActive(false);  
    }
}

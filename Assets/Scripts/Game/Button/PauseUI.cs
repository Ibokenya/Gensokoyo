using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySystem;

public class PauseUI : MonoBehaviour
{
    private bool isPaused = false;
    private string currentBGMName = "";
    private float currentBGMPosition = 0f;
    public GameObject PausePanel;
    private State pastState;
    // 暂停切换在 Update：timeScale=0 后也要能响应继续
    void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        //  Esc 路由中心：整个游戏只有这里读 Esc
        // 加 IsReplayPaused 让已暂停时 PauseUI 什么都不做，把 Esc 留给 ReplayPauseUI 消费
        //  读 PhysicalKeyMapping.Escape 而非 KeyCode.Escape —— 玩家可重绑暂停键
        if (!Input.GetKeyDown(PhysicalKeyMapping.Escape)) return;
        if (Global_GameManager.Instance.state == State.Over) return;
        // 决死时停期间，禁止 Esc 暂停（普通 Pause 和回放 ReplayPause 均屏蔽）
        if (Global_GameManager.Instance.state == State.TimeStop) return;

        // 回放模式：路由到回放暂停 UI
        var rm = ReplayManager.Instance;
        if (rm != null && rm.CurrentMode == ReplayManager.Mode.Playback)
        {
            if (!ReplayManager.IsReplayPaused)
            {
                // 未暂停 → 打开回放暂停面板
                ReplayManager.UIManagerInstance?.ShowReplayPause();
                ReplayManager.NotifyReplayPaused();
            }
            // 已暂停时 PauseUI 不响应 Esc，让 ReplayPauseUI 在自己的 Update 里消费 Esc 做 Resume
            return;
        }

        // 普通游戏：路由到自己的 Pause/Resume
        if (!isPaused) Pause();
        else
        {
            //  manual 状态下 Esc 只让 PauseEvent 关 manual，不直接 Resume
            // 否则 manual 关了同时暂停也关了，后续再按 Esc 打开会乱
            var pauseEvent = PausePanel != null ? PausePanel.GetComponentInChildren<PauseEvent>() : null;
            if (pauseEvent != null && pauseEvent.IsManualActive) return;

            Resume();
        }
    }

    private void Pause()
    {
        // 检查是否处于时停状态，时停期间不允许暂停
        if (Global_GameManager.Instance.state == State.TimeStop)
        {
            return;
        }
        
        isPaused = true;
        TimeScaleController.RegisterHardPause();
        pastState = Global_GameManager.Instance.state;
        Global_GameManager.Instance.state = State.Pause;
        
        //  暂停期间停止录制 —— AddSkipTickReason() 让 LateUpdate return early
        // 不推进 tick、不写入 recordBuffer、不推进回放文件位置
        ReplayManager.AddSkipTickReason();
        
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
        Global_GameManager.Instance.state = pastState;

        isPaused = false;
        TimeScaleController.UnregisterHardPause();
        
        //  恢复录制 —— RemoveSkipTickReason()
        ReplayManager.RemoveSkipTickReason();

        //  清掉 LiveInputProvider 的 edges + Z/X held —— 防止"用 Z/X 关闭暂停"的按键泄漏给射击/符卡脚本
        ReplayManager.ClearInputAfterPause();
        
        // 恢复播放之前的BGM
        if(Global_AudioManager.Instance != null && !string.IsNullOrEmpty(currentBGMName))
        {
            Global_AudioManager.Instance.PlayBGM(currentBGMName);
            Global_AudioManager.Instance.SetBGMPosition(currentBGMPosition);
        }
        
        PausePanel.SetActive(false);  
    }
}

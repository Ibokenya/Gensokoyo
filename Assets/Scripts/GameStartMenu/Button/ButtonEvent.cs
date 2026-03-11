using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonEvent : MonoBehaviour
{
    /// <summary>
    /// 序列化该场景内不同按钮对应的分支的大物体
    /// 0，开始菜单（默认）
    /// 1，选择难度（Start）
    /// 2，选择人物 (Start)
    /// 3，历史战绩（Result）
    /// 4，手册（Manual）
    /// 5，音乐室（MusicRoom）
    /// 6，设置（Option）
    /// <相关状态机>
    /// 本场景中存在————Menu,CharacterChoose，ModeChoose，Replay，Option，MusicRoom，Manual这些状态
    /// </相关状态机>
    /// </summary>
    [Header("0-菜单,1-难度选择,2-人物选择,3-历史战绩,4-手册,5-音乐室,6-设置")]
    public List<GameObject> SceneObjects;

    private bool IsStart = false;// 标志位，记录当前的选择角色界面是由Start引起的，还是ExStart
    
    private float inputCooldown = 0f; // 输入冷却时间，防止同一帧内重复处理输入

    [Header("音效设置")]
    [SerializeField] private AudioClip ZSound;   // Z音效
    [SerializeField] private AudioClip XSound;   // X音效
    [SerializeField] private AudioClip ErrorSound;// 不可选音效

    void Awake()
    {
        Global_GameManager.Instance.state = State.Menu;
    }

    // Update is called once per frame
    void Update()
    {
        // 更新输入冷却时间
        if (inputCooldown > 0f)
        {
            inputCooldown -= Time.deltaTime;
        }
        
        if(Global_GameManager.Instance.state!=State.Menu || Global_GameManager.Instance.state != State.Manual
        || Global_GameManager.Instance.state != State.Option)
            // 如果是菜单态则不监听输入,不是菜单态才监听输入(Manual态，Option态有内部退出情况)
            //为什么呢，因为只有菜单态时点击X键不是切换小场景而是选中该小场景下的最后一个按钮。其他情况下按X都是退出小场景
        {
            CheckUpDate();
        }
    }

    public void CheckUpDate()
    {
        switch(Global_GameManager.Instance.state)
        {
            case State.CharacterChoose:// 选角色界面
                CharacterChoose();
                break;
            case State.ModeChoose:// 选难度界面
                ModeChoose();
                break;
            case State.Replay:// 回放界面
                Replay();
                break;
            case State.MusicRoom:// 音乐室界面
                MusicRoom();
                break;
        }
    }

    public void Strat_Event()// 进入选择难度界面
    {
        IsStart = true;
        SceneObjects[0].SetActive(false);
        SceneObjects[1].SetActive(true);
        Global_GameManager.Instance.state = State.ModeChoose;
    }

    public void ExStart_Event()// 进入选择人物界面
    {
        IsStart = false;
        SceneObjects[0].SetActive(false);
        SceneObjects[2].SetActive(true);
        Global_GameManager.Instance.state = State.CharacterChoose;
        Global_GameManager.Instance.gameMode=GameMode.Extra;
        
        // 设置输入冷却时间，防止同一帧内重复处理 Z 键
        inputCooldown = 0.2f; // 0.2秒冷却时间
    }

    public void Result_Event()
    {
        SceneObjects[0].SetActive(false);
        SceneObjects[3].SetActive(true);
        Global_GameManager.Instance.state = State.Replay;
    }

    public void Manual_Event()
    {
        SceneObjects[0].SetActive(false);
        SceneObjects[4].SetActive(true);
        Global_GameManager.Instance.state = State.Manual;
    }

    public void MusicRoom_Event()
    {
        SceneObjects[0].SetActive(false);
        SceneObjects[5].SetActive(true);
        Global_GameManager.Instance.state = State.MusicRoom;
    }

    public void Option_Event()
    {
        SceneObjects[0].SetActive(false);
        SceneObjects[6].SetActive(true);
        Global_GameManager.Instance.state = State.Option;
    }

    public void Quit_Event()
    {
        // 播放X音效
        if (XSound != null)
        {
            Global_AudioManager.Instance.PlaySFX(XSound, false);
        }
        Application.Quit();
    }

    public void StartPharse2()// 开始按钮的第二阶段————选择完难度后该选择人物了
    {
        SceneObjects[1].SetActive(false);
        SceneObjects[2].SetActive(true);
        Global_GameManager.Instance.state = State.CharacterChoose;
    }

    private void CharacterChoose()
    {
        // 检查输入冷却，防止同一帧内重复处理
        if (inputCooldown > 0f)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 播放X音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(XSound, false);
            }
            if (IsStart)// 是Start的二阶段
            {
                SceneObjects[2].SetActive(false);
                SceneObjects[1].SetActive(true);
                Global_GameManager.Instance.state = State.ModeChoose;
            }
            else
            {
                SceneObjects[2].SetActive(false);
                SceneObjects[0].SetActive(true);
                Global_GameManager.Instance.state = State.Menu;
            }         
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(Global_GameManager.Instance.gameMode==GameMode.Extra)
            {
                // 播放Z音效
                if (ErrorSound != null)
                {
                    Global_AudioManager.Instance.PlaySFX(ErrorSound, false);
                }
                Debug.Log("暂未实装！");
            }
            else
            {
                // 播放Z音效
                if (XSound != null)
                {
                    Global_AudioManager.Instance.PlaySFX(ZSound, false);
                }
                Global_GameManager.Instance.state = State.Gaming;
                Global_SceneManager.Instance.IntoNextScene("Game1", true, 0.2f);
            }               
        }
    }

    private void ModeChoose()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 播放X音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(XSound, false);
            }
            SceneObjects[1].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.gameMode = GameMode.Easy;
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    private void Replay()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 播放X音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(XSound, false);
            }
            SceneObjects[3].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    public void Option()
    {
            SceneObjects[6].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
    }

    private void MusicRoom()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 播放X音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(XSound, false);
            }
            SceneObjects[5].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    public void Manual()
    {
            SceneObjects[4].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
    }
}

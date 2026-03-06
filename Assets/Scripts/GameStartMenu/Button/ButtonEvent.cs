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
    [SerializeField]
    public List<GameObject> SceneObjects;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Global_GameManager.Instance.state!=State.Menu)// 如果是菜单态则不监听输入,不是菜单态才监听输入
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
            case State.Option:// 设置界面
                Option();
                break;
            case State.MusicRoom:// 音乐室界面
                MusicRoom();
                break;
            case State.Manual:// 手册界面
                Manual();
                break;
        }
    }

    public void Strat_Event()
    {
        SceneObjects[0].SetActive(false);
        SceneObjects[1].SetActive(true);
        Global_GameManager.Instance.state = State.ModeChoose;
    }

    public void ExStart_Event()
    {
        SceneObjects[0].SetActive(false);
        SceneObjects[2].SetActive(true);
        Global_GameManager.Instance.state = State.CharacterChoose;
        Global_GameManager.Instance.gameMode=GameMode.Extra;
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

    }

    private void CharacterChoose()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneObjects[2].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    private void ModeChoose()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneObjects[1].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    private void Replay()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneObjects[3].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    private void Option()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneObjects[6].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    private void MusicRoom()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneObjects[5].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }

    private void Manual()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneObjects[4].SetActive(false);
            SceneObjects[0].SetActive(true);
            Global_GameManager.Instance.state = State.Menu;
        }
    }
}

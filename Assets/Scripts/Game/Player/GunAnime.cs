using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySystem;

public class GunAnime : MonoBehaviour
{
    public List<GameObject> NormalGuns = new();// 普通常规机体
    public GameObject ReimuGun;// 灵梦子机
    public List<GameObject> ReimuGuns;// 灵梦子机们
    public GameObject MarisaGun;// 魔理沙子机
    public List<GameObject> MarisaGuns;// 魔理沙子机们
    public GameObject MagicGun;// 七曜魔法子机
    public ShootNormal ShootNormal;// 普通常规机体引用

    private readonly List<Vector2> GunPos = new()
    {
        // 火力为1时的位置
        new Vector2(0f, 0.4f),// 0
        // 火力为2时的位置
        new Vector2(-0.18f, 0.22f),
        new Vector2(0.18f, 0.22f),
        new Vector2(-0.1f, 0.25f),// 3按下shift后的位置
        new Vector2(0.1f, 0.25f),
        // 火力为3时的位置
        new Vector2(0f, 0.32f),// 5
        new Vector2(-0.2f, 0.15f),
        new Vector2(0.2f, 0.15f),
        new Vector2(-0.17f, 0.22f),// 8按下shift后的位置
        new Vector2(0.17f, 0.22f),
        // 火力为4时的位置
        new Vector2(-0.19f, 0.23f),// 10
        new Vector2(0.19f, 0.23f),
        new Vector2(-0.35f, -0.01f),
        new Vector2(0.35f, -0.01f),
        new Vector2(-0.1f, 0.25f),// 14按下shift后的位置
        new Vector2(0.1f, 0.25f),
        new Vector2(-0.22f, 0.1f),
        new Vector2(0.22f, 0.1f),
    };

    public int Index;//0:灵梦子机 1:魔理沙子机 2:七曜魔法子机
    private int GunNumber = 1;// 子机数量

    private bool isShifted = false;// 是否按下Shift
    public bool IsShiftedNow => isShifted;// 是否按下Shift
    public bool isExitingMagic = false; // 是否正在退出魔法状态
    private bool lastShiftHeld = false;  // 🔴 上一帧 Shift held —— 自算边沿，不依赖 edgesDown/Up

    void OnEnable()
    {
        if(Global_GameManager.Instance.character == Character.Reimu)
        {
            Index = 0;
        }
        else if(Global_GameManager.Instance.character == Character.Marisa)
        {
            Index = 1;
        }
        SwitchGun();
        GunNumber = 1;
        
        // 同步子机激活状态
        if (Index == 0) UpdateGuns(ReimuGuns);
        else if (Index == 1) UpdateGuns(MarisaGuns);
        UpdateGunPos();

        // 订阅灵力变更事件
        Global_GameManager.Instance.OnPowerChanged += UpdateGunNumber;
        Global_GameManager.Instance.OnReincarnation += CancelGun;
    }

    void OnDisable()
    {
        Global_GameManager.Instance.OnPowerChanged -= UpdateGunNumber;
        Global_GameManager.Instance.OnReincarnation -= CancelGun;
    }

    void FixedUpdate()
    {
        if(Global_GameManager.Instance.state == State.Pause || 
        Global_GameManager.Instance.state == State.FinalUI) return;
        CheckUpdate();
        Cheat();
    }

    public void SwitchGun()
    {
        switch (Index)
        {
            case 0:
                NormalGuns[0].SetActive(true);
                NormalGuns[1].SetActive(true);
                ReimuGun.SetActive(true);
                MarisaGun.SetActive(false);
                MagicGun.SetActive(false);
                ShootNormal.SetLimited(false);
                UpdateGuns(ReimuGuns);
                break;
            case 1:
                NormalGuns[0].SetActive(true);
                NormalGuns[1].SetActive(true);
                ReimuGun.SetActive(false);
                MarisaGun.SetActive(true);
                MagicGun.SetActive(false);
                UpdateGuns(MarisaGuns);
                ShootNormal.SetLimited(false);
                break;
            case 2:
                NormalGuns[0].SetActive(false);
                NormalGuns[1].SetActive(false);
                ReimuGun.SetActive(false);
                MarisaGun.SetActive(false);
                MagicGun.SetActive(true);
                ShootNormal.SetLimited(true);
                break;
        }
    }

    private void CheckUpdate()
    {
        // 🔴 用 GetKey(held) 读 Shift，自算边沿 —— 不依赖 edgesDown/Up
        // held 状态跨帧稳定，FixedUpdate 50Hz 不会漏读
        bool shiftHeld = ReplayManager.Input.GetKey(LogicalKey.Shift);
        bool justPressed = shiftHeld && !lastShiftHeld;   // 上升沿：本帧 held 上一帧没 held
        bool justReleased = !shiftHeld && lastShiftHeld;  // 下降沿：本帧没 held 上一帧 held
        lastShiftHeld = shiftHeld;

        // isShifted 直接 = held（持续型状态，射击模式靠它判断）
        isShifted = shiftHeld;

        if (justPressed)
        {
            if (Index == 0)// 如果是灵梦常态
            {
                UpdateGunPos();
            }
            if (Index == 1 && !isExitingMagic)// 如果是魔理沙常态按下Shift进入七曜态
            {
                Index = 2;
                SwitchGun();
                UpdateGunPos();
            }
        }
        if (justReleased)
        {
            if (Index == 2 && !isExitingMagic)// 如果是七曜态松开Shift进入魔理沙常态
            {
                // 不立即切换，由MagicAnime完成退出动画后调用SwitchToMarisaNormal
                isExitingMagic = true;
            }
            UpdateGunPos();
        }
    }

    // 由MagicAnime调用，完成退出动画后切换到魔理沙常态
    public void SwitchToMarisaNormal()
    {
        Index = 1;
        SwitchGun();
        UpdateGunPos();
        isExitingMagic = false;
    }

    private void UpdateGunNumber(int power)
    {
        if(GunNumber < power/100)
        {
            AddGuns();
        }
        else if(GunNumber > power/100)
        {
            DeleteGuns();
        }
    }

    public void UpdateGuns(List<GameObject> guns)
    {
        for(int i=0;i<GunNumber;i++)
        {
            guns[i].SetActive(true);
        }
        for(int i=guns.Count-1;i>=GunNumber;i--)
        {
            guns[i].SetActive(false);
        }
        UpdateGunPos();
    }

    private void AddGuns()
    {
        GunNumber++;
        if (Index == 0) ReimuGuns[GunNumber - 1].SetActive(true);
        else if (Index == 1) MarisaGuns[GunNumber - 1].SetActive(true);
        UpdateGunPos();
    }

    private void DeleteGuns()
    {
        GunNumber--;
        if (Index == 0) ReimuGuns[GunNumber].SetActive(false);
        else if (Index == 1) MarisaGuns[GunNumber].SetActive(false);
        UpdateGunPos();
    }

    public void UpdateGunPos()
    {
        if(Index==0)// 灵梦子机
        {
            if(isShifted)
            {
                switch (GunNumber)
                {
                    case 1:
                        ReimuGuns[0].transform.localPosition = GunPos[0];
                        break;
                    case 2:
                        ReimuGuns[0].transform.localPosition = GunPos[3];
                        ReimuGuns[1].transform.localPosition = GunPos[4];
                        break;
                    case 3:
                        ReimuGuns[0].transform.localPosition = GunPos[5];
                        ReimuGuns[1].transform.localPosition = GunPos[8];
                        ReimuGuns[2].transform.localPosition = GunPos[9];
                        break;
                    case 4:
                        ReimuGuns[0].transform.localPosition = GunPos[14];
                        ReimuGuns[1].transform.localPosition = GunPos[15];
                        ReimuGuns[2].transform.localPosition = GunPos[16];
                        ReimuGuns[3].transform.localPosition = GunPos[17];
                        break;
                }
            }
            else
            {
                switch (GunNumber)
                {
                    case 1:
                        ReimuGuns[0].transform.localPosition = GunPos[0];
                        break;
                    case 2:
                        ReimuGuns[0].transform.localPosition = GunPos[1];
                        ReimuGuns[1].transform.localPosition = GunPos[2];
                        break;
                    case 3:
                        ReimuGuns[0].transform.localPosition = GunPos[5];
                        ReimuGuns[1].transform.localPosition = GunPos[6];
                        ReimuGuns[2].transform.localPosition = GunPos[7];
                        break;
                    case 4:
                        ReimuGuns[0].transform.localPosition = GunPos[10];
                        ReimuGuns[1].transform.localPosition = GunPos[11];
                        ReimuGuns[2].transform.localPosition = GunPos[12];
                        ReimuGuns[3].transform.localPosition = GunPos[13];
                        break;
                }
            }
        }
        else if(Index==1)// 魔理沙子机
        {
            switch (GunNumber)
            {
                case 1:
                    MarisaGuns[0].transform.localPosition = GunPos[0];
                    MarisaGuns[0].transform.eulerAngles = new Vector3(0,0,0);
                    break;
                case 2:
                    MarisaGuns[0].transform.localPosition = GunPos[1];   
                    MarisaGuns[0].transform.eulerAngles = new Vector3(0,0,0);
                    MarisaGuns[1].transform.localPosition = GunPos[2];
                    MarisaGuns[1].transform.eulerAngles = new Vector3(0,0,0);
                    break;
                case 3:
                    MarisaGuns[0].transform.localPosition = GunPos[5];
                    MarisaGuns[0].transform.eulerAngles = new Vector3(0,0,0);
                    MarisaGuns[1].transform.localPosition = GunPos[6];
                    MarisaGuns[1].transform.eulerAngles = new Vector3(0,0,8);
                    MarisaGuns[2].transform.localPosition = GunPos[7];
                    MarisaGuns[2].transform.eulerAngles = new Vector3(0,0,-8);
                    break;
                case 4:
                    MarisaGuns[0].transform.localPosition = GunPos[10];
                    MarisaGuns[0].transform.eulerAngles = new Vector3(0,0,-4);
                    MarisaGuns[1].transform.localPosition = GunPos[11];
                    MarisaGuns[1].transform.eulerAngles = new Vector3(0,0,4);
                    MarisaGuns[2].transform.localPosition = GunPos[12];
                    MarisaGuns[2].transform.eulerAngles = new Vector3(0,0,4);
                    MarisaGuns[3].transform.localPosition = GunPos[13];
                    MarisaGuns[3].transform.eulerAngles = new Vector3(0,0,-4);
                    break;
            }
        }
    }

    private void Cheat()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Global_GameManager.Instance.AddPower(50);
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Global_GameManager.Instance.SubPower(50);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Global_GameManager.Instance.AddBomb(1);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Global_GameManager.Instance.isCheheat = !Global_GameManager.Instance.isCheheat;
        }
    }

    private void CancelGun(State state)
    {
        // 🔴 死亡时玩家 GameObject 可能被 Destroy，SetActive 要先判 activeInHierarchy
        if (NormalGuns != null && NormalGuns.Count >= 2)
        {
            if (NormalGuns[0] != null && NormalGuns[0].activeInHierarchy) NormalGuns[0].SetActive(false);
            if (NormalGuns[1] != null && NormalGuns[1].activeInHierarchy) NormalGuns[1].SetActive(false);
        }
        if (ReimuGun != null && ReimuGun.activeInHierarchy) ReimuGun.SetActive(false);
        if (MarisaGun != null && MarisaGun.activeInHierarchy) MarisaGun.SetActive(false);
        if (ShootNormal != null) ShootNormal.SetLimited(true);

        // 🔴 延迟 SwitchGun —— 用 this 捕获，回调里先判 destroyed
        var gunAnime = this;
        SimTimer.Once(() =>
        {
            if (gunAnime == null) return;  // 玩家对象已销毁
            gunAnime.SwitchGun();
        }, 50);
    }
}

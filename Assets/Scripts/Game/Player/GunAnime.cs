using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnime : MonoBehaviour
{
    public GameObject ReimuGun;// 灵梦子机
    public List<GameObject> ReimuGuns;// 灵梦子机们
    public GameObject MarisaGun;// 魔理沙子机
    public List<GameObject> MarisaGuns;// 魔理沙子机们
    public GameObject MagicGun;// 七曜魔法子机  

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

    private int Index;//0:灵梦子机 1:魔理沙子机 2:七曜魔法子机
    private int GunNumber = 0;// 子机数量

    private bool isShifted = false;// 是否按下Shift

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
        SwitchGun(Index);
        GunNumber = Global_GameManager.Instance.Power/100;
        // 同步子机激活状态
        if (Index == 0) UpdateGuns(ReimuGuns);
        else if (Index == 1) UpdateGuns(MarisaGuns);
        UpdateGunPos();

        // 订阅灵力变更事件
        Global_GameManager.Instance.OnPowerChanged += UpdateGunNumber;
    }

    void OnDisable()
    {
        Global_GameManager.Instance.OnPowerChanged -= UpdateGunNumber;
    }

    // Update is called once per frame
    void Update()
    {
        CheckUpdate();
        addPower();
        subPower();
    }

    private void SwitchGun(int index)
    {
        switch (index)
        {
            case 0:
                ReimuGun.SetActive(true);
                MarisaGun.SetActive(false);
                MagicGun.SetActive(false);
                break;
            case 1:
                ReimuGun.SetActive(false);
                MarisaGun.SetActive(true);
                MagicGun.SetActive(false);
                UpdateGuns(MarisaGuns);
                break;
            case 2:
                ReimuGun.SetActive(false);
                MarisaGun.SetActive(false);
                MagicGun.SetActive(true);
                break;
        }
    }

    private void CheckUpdate()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShifted = true;
            if(Index==1)// 如果是魔理沙常态按下Shift进入七曜态
            {
                Index = 2;
            }
            SwitchGun(Index);
            UpdateGunPos();
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            isShifted = false;
            if(Index==2)// 如果是七曜态松开Shift进入魔理沙常态
            {
                Index = 1;
            }
            SwitchGun(Index);
            UpdateGunPos();
        }
    }

    private void UpdateGunNumber(int power)
    {
        Debug.Log("触发一次火力增长与子机增减事件");
        if(GunNumber < power/100)
        {
            AddGuns();
        }
        else if(GunNumber > power/100)
        {
            DeleteGuns();
        }
    }

    private void UpdateGuns(List<GameObject> guns)
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

    private void UpdateGunPos()
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
                    MarisaGuns[2].transform.eulerAngles = new Vector3(0,0,2);
                    MarisaGuns[3].transform.localPosition = GunPos[13];
                    MarisaGuns[3].transform.eulerAngles = new Vector3(0,0,-2);
                    break;
            }
        }
    }

    private void addPower()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("测试，Power+50");
            Global_GameManager.Instance.AddPower(50);
        }
    }
    private void subPower()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("测试，Power-50");
            Global_GameManager.Instance.SubPower(50);
        }
    }
}

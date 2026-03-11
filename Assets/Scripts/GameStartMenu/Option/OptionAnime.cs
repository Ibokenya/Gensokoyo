using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionAnime : MonoBehaviour
{
    [Header("存储设置界面图片的亮暗形态")]
    public List<Image> OptionButtons;
    public List<Sprite> DarkImages;
    public List<Sprite> LightImages;

    public List<Image> Numbers;
    public List<Sprite> DarkNumbers;
    public List<Sprite> LightNumbers;

    private Vector3 PositionDelta = new(30, 10, 0);
    private readonly float ScaleDelta = 0.1f;

    private int Index = 0;
    private int LastIndex = 0;

    private int Hundred;// 百位
    private int ten;// 十位
    private int one;// 个位
    private float bgmVolume;// 音乐音量
    private float sfxVolume;// 音效音量

    private Color NoneColor = new (1f,1f,1f,0f);
    private Color FullColor = new (1f,1f,1f,1f);

    private bool IsOption = false;// 是否处于设置界面内部标志位
    public ButtonEvent Event;

    [Header("音效设置")]
    [SerializeField] private AudioClip moveoffSound;   // 取消选中音效
    [SerializeField] private AudioClip ZSound;    // Z音效
    [SerializeField] private AudioClip XSound;    // X音效
    [SerializeField] private AudioClip ErrorSound;    // 取消音效

    // Start is called before the first frame update
    void Start()
    {
        if(DarkImages.Count == 0|| LightImages.Count == 0|| DarkImages.Count != LightImages.Count)
        {
            Debug.Log("设置界面的图片不能为空或不相等");
            enabled = false;
        }
        if(DarkNumbers.Count == 0|| LightNumbers.Count == 0|| DarkNumbers.Count != LightNumbers.Count)
        {
            Debug.Log("设置界面的数字不能为空或不相等");
            enabled = false;
        }
        BeChoose(Index);
    }

    private void OnEnable()
    {
        IsOption = false;
        Index = 0;
        LastIndex = 0;
        BeChoose(Index);
        SetNumber();
    }

    // Update is called once per frame
    void Update()
    {
        CheckUpdate();
    }

    private void CheckUpdate()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(IsOption)
            {
                ChangeVolume(0.1f);
            }
            else
            {
                ButtonUp();
                if(Index!=3)
                    SetNumber();
            }
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            if(IsOption)
            {
                ChangeVolume(-0.1f);
            }
            else
            {
                ButtonDown();
                if (Index != 3)
                    SetNumber();
            }
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(IsOption)// 处于设置界面内部（执行对应Index的逻辑）
            {

            }
            else
            {
                BeClick(Index);
            }
        }
        if(Input.GetKeyDown(KeyCode.X))
        {
            if(XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(XSound, false);
            }
            if(IsOption)
            {
                Cancel();
            }
            else
            {
                Quit();
            }
        }
    }

    private void ButtonUp()
    {
        LastIndex = Index;
        Index = (Index - 1 + DarkImages.Count) % DarkImages.Count;
        BeRemove(LastIndex);
        BeChoose(Index);
    }

    private void ButtonDown()
    {
        LastIndex = Index;
        Index = (Index + 1) % DarkImages.Count;
        BeRemove(LastIndex);
        BeChoose(Index);
    }

    private void BeChoose(int index)
    {
        OptionButtons[index].transform.localPosition += PositionDelta;
        OptionButtons[index].transform.localScale += new Vector3(ScaleDelta, ScaleDelta, 0);
        OptionButtons[index].sprite = LightImages[index];
    }

    private void BeRemove(int index)
    {
        if(moveoffSound != null)
        {
            Global_AudioManager.Instance.PlaySFX(moveoffSound, false);
        }
        OptionButtons[index].transform.localPosition -= PositionDelta;
        OptionButtons[index].transform.localScale -= new Vector3(ScaleDelta, ScaleDelta, 0);
        OptionButtons[index].sprite = DarkImages[index];
    }

    private void BeClick(int index)
    {
        IsOption = true;
        if(index==2)
        {
            if(ErrorSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(ErrorSound, false);
            }
            Debug.Log("暂未实装");
            IsOption = false;
        }
        else
        {
            if(ZSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(ZSound, false);
            }
            switch(index)
            {
                case 0:// 调整音乐音量大小
                    SetNumber();
                    break;
                case 1:// 调整音效音量大小
                    SetNumber();
                    break;
                case 3:// 恢复默认
                    IsOption = false;
                    ResetDefault();
                    break;
                case 4:// 退出设置界面
                    IsOption = false;
                    Quit();
                    break;
                default:
                    Debug.Log("选项索引错误");
                    break;
            }
        }
    }

    private void Cancel()
    {
        IsOption = false;
        SetNumber();
    }

    private void Quit()
    {
        IsOption = false;
        BeRemove(Index);
        Event.Option();
    }

    private void SetNumber()
    {
        bgmVolume = Global_AudioManager.Instance.GetBGMVolume();
        sfxVolume = Global_AudioManager.Instance.GetSFXVolume();
        if(Index == 0)// 音乐音量
        {
            int percentage = Mathf.RoundToInt(bgmVolume * 100);
            Hundred = percentage / 100;
            ten = percentage / 10;
            if(ten == 10) ten =0;
            one = percentage % 10;
            if(IsOption)// 处于菜单态，高亮选中
            {
                Numbers[0].sprite = LightNumbers[ten];
                Numbers[1].sprite = LightNumbers[one];
                Numbers[2].sprite = LightNumbers[10];
                if(Hundred != 0)
                {
                    Numbers[3].sprite = LightNumbers[1];
                }
            }
            else
            {
                Numbers[0].sprite = DarkNumbers[ten];
                Numbers[1].sprite = DarkNumbers[one];
                Numbers[2].sprite = DarkNumbers[10];
                if(Hundred != 0)
                {
                    Numbers[3].sprite = DarkNumbers[1];
                }
            }
            if(Hundred == 0)
            {
                Numbers[3].color = NoneColor;
            }
            else
            {
                Numbers[3].color = FullColor;
            }
        }
        else if(Index == 1)// 音效音量
        {
            int percentage = Mathf.RoundToInt(sfxVolume * 100);
            Hundred = percentage / 100;
            ten = percentage / 10;
            if(ten == 10) ten =0;
            one = percentage % 10;
            if(IsOption)
            {
                Numbers[0].sprite = LightNumbers[ten];
                Numbers[1].sprite = LightNumbers[one];
                Numbers[2].sprite = LightNumbers[10];
                if(Hundred != 0)
                {
                    Numbers[3].sprite = LightNumbers[1];
                }
            }
            else
            {
                Numbers[0].sprite = DarkNumbers[ten];
                Numbers[1].sprite = DarkNumbers[one];
                Numbers[2].sprite = DarkNumbers[10];
                if(Hundred != 0)
                {
                    Numbers[3].sprite = DarkNumbers[1];
                }
            }
            if(Hundred == 0)
            {
                Numbers[3].color = NoneColor;
            }
            else
            {
                Numbers[3].color = FullColor;
            }
        }
        else
        {
            Numbers[0].sprite = DarkNumbers[0];
            Numbers[1].sprite = DarkNumbers[0];
            Numbers[2].sprite = DarkNumbers[10];
            Numbers[3].color = NoneColor;
        }
    }

    private void ChangeVolume(float volume)
    {
        if(Index == 0)// 音乐音量
        {
            bgmVolume += volume;
            if(bgmVolume < 0)
            {
                bgmVolume = 0;
            }
            else if(bgmVolume > 1)
            {
                bgmVolume = 1;
            }
            Global_AudioManager.Instance.SetBGMVolume(bgmVolume);
            SetNumber();
        }
        else if(Index == 1)// 音效音量
        {
            sfxVolume += volume;
            if(sfxVolume < 0)
            {
                sfxVolume = 0;
            }
            else if(sfxVolume > 1)
            {
                sfxVolume = 1;
            }
            Global_AudioManager.Instance.SetSFXVolume(sfxVolume);
            SetNumber();
        }
    } 

    private void ResetDefault()
    {
        bgmVolume = 0.70f;
        sfxVolume = 0.80f;
        Global_AudioManager.Instance.SetBGMVolume(bgmVolume);
        Global_AudioManager.Instance.SetSFXVolume(sfxVolume);
        SetNumber();
    }
}

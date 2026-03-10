using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Manual : MonoBehaviour
{
    [Header("承载说明书目录")]
    public List<TextMeshProUGUI> Texts = new();
    [Header("承载说明书页")]
    public List<TextMeshProUGUI> Panels = new();

    public GameObject Shadel;
    public GameObject manual;

    private Color darkColor = new (0.5f, 0.5f, 0.5f);
    private Color lightColor = new (1f, 1f, 1f);
    private float PanelAlpha = 0.7f;

    private int Index;
    private int LastIndex;

    private bool IsIndex=true;// 是否是索引页

    public ButtonEvent Event;

    [Header("音效设置")]
    [SerializeField] private AudioClip ZSound;   // Z音效
    [SerializeField] private AudioClip XSound;   // X音效
    [SerializeField] private AudioClip PageSound;// 翻页音效

    // Start is called before the first frame update
    void Start()
    {
        if (Texts.Count == 0 || Panels.Count == 0 || Texts.Count != Panels.Count)
        {
            Debug.LogError("索引文本和介绍文本数量不匹配或为空！请检查列表赋值");
            enabled = false; // 禁用脚本，避免报错
            return;
        }
        Index = 0;
        BeSelected(Index);
    }

    private void OnEnable()
    {
        Index = 0;
        BeSelected(Index);
    }

    // Update is called once per frame
    void Update()
    {
        CheckUpDate();
    }

    private void CheckUpDate()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            // 播放翻页音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(PageSound, false);
            }
            LastIndex =Index;
            Index = (Index - 1 + Texts.Count) % Texts.Count;
            UpdateMenu();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // 播放翻页音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(PageSound, false);
            }
            LastIndex = Index;
            Index = (Index + 1) % Texts.Count;
            UpdateMenu();
        }

        if (Input.GetKeyDown(KeyCode.Z) && IsIndex)// 是索引态，进入页态
        {
            // 播放Z音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(ZSound, false);
            }
            IsIndex = false;
            Shadel.SetActive(true);
            manual.SetActive(false);
            foreach(TextMeshProUGUI text in Texts)
            {
                text.alpha=0;// 设置所有按钮为不可见
            }
            BeClicked(Index);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 播放X音效
            if (XSound != null)
            {
                Global_AudioManager.Instance.PlaySFX(XSound, false);
            }
            if (!IsIndex)// 是页态，回退到索引态
            {
                IsIndex = true;
                Shadel.SetActive(false);
                manual.SetActive(true);
                foreach (TextMeshProUGUI text in Texts)
                {
                    text.color = darkColor;// 设置所有按钮为可见
                }
                BeCanceled(Index);
                BeSelected(Index);
            }
            else// 是索引态，回退至Menu
            {
                BeRemove(Index);
                Event.Manual();
            }
        }
    }

    private void UpdateMenu()
    {
        if (IsIndex)
        {
            BeRemove(LastIndex);
            BeSelected(Index);
        }
        else
        {
            PageTurn(LastIndex, Index);
        }
    }

    private void BeSelected(int index)
    {
        Texts[index].color = lightColor;
    }

    private void BeRemove(int index)
    {
        Texts[index].color = darkColor;
    }

    private void BeClicked(int index)
    {
        Panels[index].alpha = PanelAlpha;
    }

    private void BeCanceled(int index)
    {
        Panels[index].alpha = 0;
    }

    private void PageTurn(int last,int now)
    {
        Panels[last].alpha=0;
        Panels[now].alpha = PanelAlpha;
    }
}

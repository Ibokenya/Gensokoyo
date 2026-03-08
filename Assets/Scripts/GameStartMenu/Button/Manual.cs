using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Manual : MonoBehaviour
{
    [Header("承载说明书目录")]
    [SerializeField]
    public List<TextMeshProUGUI> Texts = new List<TextMeshProUGUI>();
    [Header("承载说明书页")]
    [SerializeField]
    public List<TextMeshProUGUI> Panels = new List<TextMeshProUGUI>();

    private Color darkColor = new Color(0.5f, 0.5f, 0.5f);
    private Color lightColor = new Color(1f, 1f, 1f);
    private float PanelAlpha = 0.7f;

    private int Index;
    private int LastIndex;

    private bool IsIndex=true;// 是否是索引页
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
            LastIndex=Index;
            Index = (Index - 1 + Texts.Count) % Texts.Count;
            UpdateMenu();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            LastIndex = Index;
            Index = (Index + 1) % Texts.Count;
            UpdateMenu();
        }

        if (Input.GetKeyDown(KeyCode.Z) && IsIndex)// 是索引态，进入页态
        {
            IsIndex = false;
            foreach(TextMeshProUGUI text in Texts)
            {
                text.alpha=0;// 设置所有按钮为不可见
            }
            BeClicked(Index);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if(!IsIndex)// 是页态，回退到索引态
            {
                IsIndex = true;
                foreach (TextMeshProUGUI text in Texts)
                {
                    text.color = darkColor;// 设置所有按钮为可见
                }
                BeSelected(Index);
            }
            else// 是索引态，回退至Menu
            {

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

    private void PageTurn(int last,int now)
    {
        Panels[last].alpha=0;
        Panels[now].alpha = PanelAlpha;
    }


}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("符卡UI组件")]
    public Image cardImage; // 符卡图片
    public List<Sprite> cardSprites; // 符卡图片列表
    public TextMeshProUGUI cardNameText_1; // 符卡名称文本
    public TextMeshProUGUI cardNameText_2; // 符卡名称文本

    public void SetCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < cardSprites.Count)
        {
            cardImage.sprite = cardSprites[cardIndex];
            ShowCard();
        }
    }

    public void SetCardName_1(string cardName)
    {
        cardNameText_1.text = cardName;
    }

    public void SetCardName_2(string cardName)
    {
        cardNameText_2.text = cardName;
    }

    public void SetCardColor(float green)
    {
        cardNameText_1.color = new Color(0,green,1,1);
        cardNameText_2.color = new Color(0,green,1,1);
    }

    public void ShowCard()
    {
        StartCoroutine(ShowCardCoroutine());
    }

    private IEnumerator ShowCardCoroutine()
    {
        cardImage.color = new Color(1f, 1f, 1f, 0f);
        float alpha = cardImage.color.a;
        while (alpha < 1f)
        {
            alpha += 0.1f;
            cardImage.color = new Color(1f, 1f, 1f, alpha);
            yield return new WaitForSeconds(0.1f);
        }
    }
}

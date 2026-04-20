using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("·û¿¨UI×é¼þ")]
    public Image cardImage; // ·û¿¨Í¼Æ¬
    public List<Sprite> cardSprites; // ·û¿¨Í¼Æ¬ÁÐ±í
    public TextMeshProUGUI cardNameText; // ·û¿¨Ãû³ÆÎÄ±¾

    public void SetCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < cardSprites.Count)
        {
            cardImage.sprite = cardSprites[cardIndex];
            ShowCard();
        }
    }

    public void SetCardName(string cardName)
    {
        cardNameText.text = cardName;
    }

    public void SetCardColor(float green)
    {
        cardNameText.color = new Color(0,green,1,1);
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

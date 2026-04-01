using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpeelCard : MonoBehaviour
{
    public List<GameObject> BombHearts; // ·û¿¨Í¼±ê
    public TextMeshProUGUI BombText; // ·û¿¨ÎÄ±¾¿ò
    public List<Sprite> BombHeartSprites; // ·û¿¨ËéÆ¬Í¼±ê¾«Áé£¨´æ´¢0 ,1/3 ,2/3 ,1×´Ì¬µÄËéÆ¬Í¼±ê£©

    public void SetBomb(int bomb, int cardPiece)
    {
        UpdateBombHearts(bomb, cardPiece);
        UpdateBombText(cardPiece);
    }
    private void UpdateBombHearts(int bomb, int cardPiece)
    {
        int i = 0;
        while(i < bomb)
        {
            BombHearts[i].GetComponent<Image>().sprite = BombHeartSprites[3]; // 1×´Ì¬µÄËéÆ¬Í¼±ê
            i++;
        }
        if(i < BombHearts.Count)
        {
            BombHearts[i].GetComponent<Image>().sprite = BombHeartSprites[cardPiece];
        }   
        i++;
        if(i < BombHearts.Count)
        {
            BombHearts[i].GetComponent<Image>().sprite = BombHeartSprites[0];
        }
    }
    private void UpdateBombText(int cardPiece)
    {
        BombText.text = cardPiece.ToString() + " / 3";
    }
}

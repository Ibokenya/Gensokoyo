using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeftLife : MonoBehaviour
{
    public List<GameObject> LifeHearts; // 生命值图标
    public TextMeshProUGUI LifeText; // 生命值文本框
    public List<Sprite> LifeHeartSprites; // 生命值图标精灵（存储0 ,1/3 ,2/3 ,1状态的生命图标）

    public void SetLife(int life, int lifePiece)
    {
        UpdateLifeHearts(life, lifePiece);
        UpdateLifeText(lifePiece);
    }
    private void UpdateLifeHearts(int life, int lifePiece)
    {
        int i = 0;
        while(i < life)
        {
            LifeHearts[i].GetComponent<Image>().sprite = LifeHeartSprites[3]; // 1状态的碎片图标
            i++;
        }
        if(i < LifeHearts.Count)// 切换之后显示碎片
        {
            LifeHearts[i].GetComponent<Image>().sprite = LifeHeartSprites[lifePiece];
        }
        i++;
        while(i < LifeHearts.Count)// 没血的时候把所有剩余位设为0
        {
            LifeHearts[i].GetComponent<Image>().sprite = LifeHeartSprites[0];
            i++;
        }
    }
    private void UpdateLifeText(int lifePiece)
    {
        LifeText.text = lifePiece.ToString() + " / 3";
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeChange : MonoBehaviour
{
    public List<Sprite> ModeSprites;
    private Image ModeImage;

    void OnEnable()
    {
        ModeImage = GetComponent<Image>();
        switch(Global_GameManager.Instance.gameMode)
        {
            case GameMode.Easy:
                ModeImage.sprite = ModeSprites[0];
                break;
            case GameMode.Normal:
                ModeImage.sprite = ModeSprites[1];
                break;
            case GameMode.Hard:
                ModeImage.sprite = ModeSprites[2];
                break;
            case GameMode.Lunatic:
                ModeImage.sprite = ModeSprites[3];
                break;
            case GameMode.Extra:
                ModeImage.sprite = ModeSprites[4];
                break;
        }
    }
}

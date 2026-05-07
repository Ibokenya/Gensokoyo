using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChangeBG : MonoBehaviour
{
    public BossBeheve bossBeheve;
    [Header("boss战对于背景的引用")]
    public GameObject none1;
    public GameObject card1;
    public GameObject none2;
    public GameObject card2;
    public GameObject balloon;
    public GameObject finalCard;
    public GameObject freeze_effect;    
    public GameObject Magic_effect;
    public Animator DeadStar_effect;

    private GameObject currentBG;// 当前符卡对应的背景


    public void ShowBg(string bgName,float finalAlpha)
    {
        switch (bgName)
        {
            case "none1":
                currentBG = none1;
                break;
            case "card1":
                currentBG = card1;
                break;
            case "none2":
                currentBG = none2;
                break;
            case "card2":
                currentBG = card2;
                break;
            case "balloon":
                currentBG = balloon;
                break;
            case "finalCard":
                currentBG = finalCard;
                break;
            default:
                Debug.LogError("未找到背景" + bgName);
                break;
        }
        currentBG.SetActive(true);
        currentBG.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        StartCoroutine(ShowBg(finalAlpha));
    }
    private IEnumerator ShowBg(float finalAlpha)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color color = currentBG.GetComponent<SpriteRenderer>().color;
            color.a = Mathf.Lerp(0, finalAlpha, elapsedTime / duration);
            currentBG.GetComponent<SpriteRenderer>().color = color;
            yield return null;
        }
        currentBG.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, finalAlpha);
        yield return null;
    }
    public void HideBg()
    {
        currentBG.SetActive(false);
    }

    public void ShowMagicEffect()
    {
        StartCoroutine(ShowMagic());
    }
    private IEnumerator ShowMagic()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color color = Magic_effect.GetComponent<SpriteRenderer>().color;
            color.a = Mathf.Lerp(0, 0.3f, elapsedTime / duration);
            Magic_effect.GetComponent<SpriteRenderer>().color = color;
            yield return null;
        }
        Magic_effect.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
        yield return null;
    }

    public void BeginDeadStarEffect()
    {
        DeadStar_effect.SetBool("IsAnime", true);
    }
    
    public void AnimationEnd()
    {
        DeadStar_effect.SetBool("IsAnime", false);
        bossBeheve.ExplosionEnd();
        Debug.Log("新星爆炸动画结束");
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    
    [Header("冻结效果")]
    public SpriteRenderer freezeSprite; // 冻结效果精灵对象
    public Image freezeUI; // 冻结效果Image对象

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
    
    /// <summary>
    /// 在指定时间内将冻结效果的透明度平滑增长到1
    /// </summary>
    /// <param name="duration">过渡时间（秒）</param>
    public void FreezeAll(float duration = 1f)
    {
        StartCoroutine(FreezeAllCoroutine(duration));
    }
    
    /// <summary>
    /// 冻结效果协程
    /// </summary>
    private IEnumerator FreezeAllCoroutine(float duration)
    {
        float elapsedTime = 0f;
        
        // 初始化透明度为0
        if (freezeSprite != null)
        {
            Color color = freezeSprite.color;
            color.a = 0f;
            freezeSprite.color = color;
        }
        
        if (freezeUI != null)
        {
            Color color = freezeUI.color;
            color.a = 0f;
            freezeUI.color = color;
        }
        
        // 平滑过渡到完全不透明
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            float scale = Mathf.Lerp(1f, 0.2f, elapsedTime / duration);
            if (freezeSprite != null)
            {
                Color color = freezeSprite.color;
                color.a = alpha;
                freezeSprite.color = color;
            }
            
            if (freezeUI != null)
            {
                Color color = freezeUI.color;
                color.a = alpha;
                freezeUI.color = color;
            }

            Time.timeScale = scale;
            
            yield return null;
        }
        
        // 确保最终透明度为1
        if (freezeSprite != null)
        {
            Color color = freezeSprite.color;
            color.a = 1f;
            freezeSprite.color = color;
        }
        
        if (freezeUI != null)
        {
            Color color = freezeUI.color;
            color.a = 1f;
            freezeUI.color = color;
        }
        bossBeheve.ShowFinalUI();
    }
    
    public void AnimationEnd()
    {
        DeadStar_effect.SetBool("IsAnime", false);
        bossBeheve.ExplosionEnd();
        Debug.Log("新星爆炸动画结束");
    }
}

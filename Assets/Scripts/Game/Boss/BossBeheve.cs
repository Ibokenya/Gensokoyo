using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBeheve : MonoBehaviour
{
    [Header("阶段脚本")]
    public none1 none1Script; // 第一阶段普通攻击脚本
    public card1 card1Script; // 第一阶段符卡脚本
    public none2 none2Script; // 第二阶段普通攻击脚本
    public card2 card2Script; // 第二阶段符卡脚本
    public FinalCard finalCardScript; // 最终符卡脚本
    
    [Header("引用")]
    public GameObject UI;
    public GameObject boss;
    public BossShootSystem bossShootSystem;

    public BossUI bossUI;
    public FreezeSystem freezeSystem;
    public GameObject FinalWordsUI;
    public BossAnime bossAnime;
    public GameObject card_UI;
    public CardUI cardUI;
    public GameObject BossBG;
    public ChangeBG changeBG;
    public SpriteRenderer IcePinion_left;
    public SpriteRenderer IcePinion_right;
    public Animator animator;
    public BossBase bossBase; // Boss基础属性引用
    
    private float currentTime = 0f;
    private bool hasPlayedCharacterAnimation = false;
    private bool hasActivatedUI = false;
    private bool hasActivatedNone1 = false;
    private bool hasCalledNone1CheckOver = false;
    private bool hasActivatedCard1 = false;
    private bool hasCalledCard1CheckOver = false;
    private bool hasActivatedNone2 = false;
    private bool hasCalledNone2CheckOver = false;
    private bool hasActivatedCard2 = false;
    private bool hasCalledCard2CheckOver = false;
    private bool hasCalledBgAndBallon = false;
    private bool hasCalledFinalAnime = false;
    private bool hasActivatedFinalCard = false;
    private bool hasCalledFinalCardCheckOver = false;

    private bool hasEndedFinalCard = false;
    public AudioClip finalOverSound;// 最终击破音效（Peng~~）
    
       private void Update()
    {
        // 获取当前音乐时间
        if (Global_AudioManager.Instance != null)
        {
            // 时间标记
            // currentTime = Global_AudioManager.Instance.CurrentBGMTime;
            currentTime += Time.deltaTime;
        }
        
        // 处理时间事件
        HandleTimeEvents();
    }
    
    /// <summary>
    /// 处理时间事件
    /// </summary>
    private void HandleTimeEvents()
    {
        // 时间为0秒时，播放角色动画
        if (currentTime >= 0f && currentTime < 1f && !hasPlayedCharacterAnimation)
        {
            BossBG.SetActive(true);
            PlayCharacterAnimation();
            hasPlayedCharacterAnimation = true;
        }
        
        if (currentTime >= 4f && currentTime < 5f && !hasActivatedUI)
        {
            animator.SetBool("isAnime", true);
            UI.SetActive(true);
            card_UI.SetActive(true);
            bossAnime.ShowHP();
            bossUI.SetCardTime(16f);
            hasActivatedUI = true;
        }

        // // 时间为5秒时，激活none1
        // if (currentTime >= 5f && currentTime < 6f && !hasActivatedNone1)
        // {
        //     boss.GetComponent<Collider2D>().enabled = true;
        //     // 设置none1阶段血量，总时长16秒（5秒激活，21秒结束）
        //     if (bossBase != null)
        //     {
        //         bossBase.SetPhaseHP_Time(0, 16f);
        //         Debug.Log("设置none1阶段血量");
        //     }
        //     if (none1Script != null)
        //     {
        //         none1Script.enabled = true;
        //         cardUI.SetCard(0);
        //         cardUI.SetCardName_1("-170℃");
        //         cardUI.SetCardName_2("");
        //         cardUI.SetCardColor(0.8f);
        //         changeBG.ShowBg("none1", 0.3f);
        //         Debug.Log("激活none1");
        //     }
        //     hasActivatedNone1 = true;
        // }
        
        // // 时间为21秒时，调用None1的CheckOver()方法
        // if (currentTime >= 21f && currentTime < 22f && !hasCalledNone1CheckOver)
        // {
        //     if (none1Script != null)
        //     {
        //         none1Script.CheckOver();
        //     }
        //     animator.SetBool("isAnime", true);
        //     Debug.Log("调用none1.CheckOver()");
        //     changeBG.HideBg();
        //     bossUI.SetCardTime(23f);
        //     // 输出none1阶段受到的伤害
        //     if (bossBase != null)
        //     {
        //         bossBase.LogPhaseDamage("none1");
        //     }
        //     hasCalledNone1CheckOver = true;
        //     bossUI.SubLife();
        // }
        
        // // 时间为22秒时，禁用none1激活card1
        // if (currentTime >= 22f && currentTime < 23f && !hasActivatedCard1)
        // {
        //     if (none1Script != null)
        //     {               
        //         none1Script.enabled = false;
        //         Debug.Log("禁用none1");
        //     }
        //     // 设置card1阶段血量，总时长23秒（22秒激活，45秒结束）
        //     if (bossBase != null)
        //     {
        //         bossBase.SetPhaseHP_Time(1, 23f);
        //         Debug.Log("设置card1阶段血量");
        //     }
        //     if (card1Script != null)
        //     {
        //         card1Script.enabled = true;
        //         cardUI.SetCard(1);
        //         cardUI.SetCardName_1("-220℃");
        //         cardUI.SetCardName_2("冰冷彗星带");
        //         cardUI.SetCardColor(0.6f);
        //         changeBG.ShowBg("card1", 0.3f);
        //         Debug.Log("激活card1");
        //     }
        //     hasActivatedCard1 = true;
        // }
        
        // // 时间为45秒时，调用card1的checkover
        // if (currentTime >= 45f && currentTime < 46f && !hasCalledCard1CheckOver)
        // {
        //     if (card1Script != null)
        //     {
        //         card1Script.CheckOver();
        //     }
        //     animator.SetBool("isAnime", true);
        //     Debug.Log("调用card1.CheckOver()");
        //     changeBG.HideBg();
        //     bossUI.SetCardTime(12f);
        //     // 输出card1阶段受到的伤害
        //     if (bossBase != null)
        //     {
        //         bossBase.LogPhaseDamage("card1");
        //     }
        //     hasCalledCard1CheckOver = true;
        //     bossUI.SubLife();
        // }
        
        // // 时间为46秒，禁用card1激活none2
        // if (currentTime >= 46f && currentTime < 47f && !hasActivatedNone2)
        // {
        //     if (card1Script != null)
        //     {
        //         card1Script.enabled = false;
        //         Debug.Log("禁用card1");
        //     }
        //     // 设置none2阶段血量，总时长12秒（46秒激活，58秒结束）
        //     if (bossBase != null)
        //     {
        //         bossBase.SetPhaseHP_Time(2, 12f);
        //         Debug.Log("设置none2阶段血量");
        //     }
        //     if (none2Script != null)
        //     {
        //         none2Script.enabled = true;
        //         cardUI.SetCard(2);
        //         cardUI.SetCardName_1("-260℃");
        //         cardUI.SetCardName_2("");
        //         cardUI.SetCardColor(0.4f);
        //         changeBG.ShowBg("none2", 0.3f);
        //         Debug.Log("激活none2");
        //     }
        //     hasActivatedNone2 = true;
        // }
        
        // // 时间为58秒，调用none2的checkover
        // if (currentTime >= 58f && currentTime < 59f && !hasCalledNone2CheckOver)
        // {
        //     if (none2Script != null)
        //     {
        //         none2Script.CheckOver();
        //     }
        //     animator.SetBool("isAnime", true);
        //     Debug.Log("调用none2.CheckOver()");
        //     changeBG.HideBg();
        //     bossUI.SetCardTime(24f);
        //     // 输出none2阶段受到的伤害
        //     if (bossBase != null)
        //     {
        //         bossBase.LogPhaseDamage("none2");
        //     }
        //     hasCalledNone2CheckOver = true;
        //     bossUI.SubLife();
        // }
        
        // // 时间为59秒，禁用none2激活card2
        // if (currentTime >= 59f && currentTime < 60f && !hasActivatedCard2)
        // {
        //     if (none2Script != null)
        //     {
        //         none2Script.enabled = false;
        //         Debug.Log("禁用none2");
        //     }
        //     // 设置card2阶段血量，总时长24秒（59秒激活，83秒结束）
        //     if (bossBase != null)
        //     {
        //         bossBase.SetPhaseHP_Time(3, 24f);
        //         Debug.Log("设置card2阶段血量");
        //     }
        //     if (card2Script != null)
        //     {
        //         card2Script.enabled = true;
        //         cardUI.SetCard(3);
        //         cardUI.SetCardName_1("-270℃");
        //         cardUI.SetCardName_2("宇宙微波辐射");
        //         cardUI.SetCardColor(0.2f);
        //         changeBG.ShowBg("card2", 0.1f);
        //         Debug.Log("激活card2");
        //     }
        //     hasActivatedCard2 = true;
        // }
        
        // // 时间为83秒，调用card2的checkover
        // if (currentTime >= 83f && currentTime < 84f && !hasCalledCard2CheckOver)
        // {
        //     if (card2Script != null)
        //     {
        //         card2Script.CheckOver();
        //     }
        //     animator.SetBool("isAnime", true);
        //     Debug.Log("调用card2.CheckOver()");
        //     changeBG.HideBg();
        //     // 输出card2阶段受到的伤害
        //     if (bossBase != null)
        //     {
        //         bossBase.LogPhaseDamage("card2");
        //     }
        //     // 重设血量条（从1%回到100%），为finalCard做准备
        //     if (bossAnime != null)
        //     {
        //         bossAnime.SetHpBar(1, 1);
        //     }
        //     hasCalledCard2CheckOver = true;
        //     bossUI.SubLife();
        // }
        
        // 时间为84秒，禁用card2并调用BgAndBallon方法
        if (currentTime >= 5f && currentTime < 85f && !hasCalledBgAndBallon)
        {
            if (card2Script != null)
            {
                card2Script.enabled = false;
                Debug.Log("禁用card2");
            }
            BgAndBallon();
            Debug.Log("调用BgAndBallon方法");
            changeBG.ShowBg("balloon", 0.5f);
            hasCalledBgAndBallon = true;
            freezeSystem.IsStop = true;
        }
        
        // 时间为88秒，调用FinalAnime方法
        if (currentTime >= 9f && currentTime < 89f && !hasCalledFinalAnime)
        {
            FinalAnime();
            cardUI.SetCard(4);
            cardUI.SetCardName_1("-273.15℃");
            cardUI.SetCardName_2("然后分子便不再运动了");
            cardUI.SetCardColor(0f);
            Debug.Log("调用FinalAnime方法");
            changeBG.HideBg();
            // 调用Conceal方法，在1秒内将Boss透明度淡出为0.5f并隐藏血条
            if (bossAnime != null)
            {
                bossAnime.Conceal();
            }
            bossUI.ShowFinalWarning();
            hasCalledFinalAnime = true;
        }
        
        // 时间为90秒，激活finalcard
        if (currentTime >= 11f && currentTime < 91f && !hasActivatedFinalCard)
        {
            if (finalCardScript != null)
            {
                finalCardScript.enabled = true;
                ShowPinion();         
                bossUI.SetCardTime(48f);
                Debug.Log("激活finalCard");
                changeBG.ShowBg("finalCard", 0.4f);
            }
            hasActivatedFinalCard = true;
            freezeSystem.IsStop = false;
        }
        
        // 时间为138秒，调用finalcard的checkover方法
        if (currentTime >= 59f && currentTime < 139f && !hasCalledFinalCardCheckOver)
        {
            // if (finalCardScript != null)
            // {
            //     finalCardScript.CheckOver();
            // }
            hasCalledFinalCardCheckOver = true;
        }

        // 时间为139秒，禁用finalcard，并调用AllOver方法
        if (currentTime >= 60f && currentTime < 140f && !hasEndedFinalCard)
        {
            if (finalCardScript != null)
            {
                finalCardScript.enabled = false;
                Debug.Log("禁用finalCard");
            }
            AllOver();
            Debug.Log("一切都结束了");
            hasEndedFinalCard = true;
        }
    }
    
    /// <summary>
    /// 播放角色动画
    /// </summary>
    private void PlayCharacterAnimation()
    {
        bossAnime.PlayShowAnime();
    }
    
    /// <summary>
    /// BgAndBallon方法
    /// </summary>
    private void BgAndBallon()
    {
        FinalWordsUI.SetActive(true);
    }
    
    /// <summary>
    /// FinalAnime方法
    /// </summary>
    private void FinalAnime()
    {
        // 空方法，内部不实现
        Debug.Log("调用FinalAnime方法");
    }
    
    /// <summary>
    /// AllOver方法
    /// 时符结束后，时间流速变缓，播放新星爆炸动画，隐藏背景，播放音效
    /// </summary>
    private void AllOver()
    {
        Time.timeScale = 0.3f;// 时间流速变缓
        changeBG.BeginDeadStarEffect(); // 新星爆炸动画
        changeBG.HideBg();// 隐藏最终符卡背景
        bossShootSystem.HideTerrain();// 隐藏地形
        bossShootSystem.isAllowAreaLimit = false; // 禁用区域限制攻击

        Global_AudioManager.Instance.PlaySFX(finalOverSound);// 时符击败音效
    }

    /// <summary>
    /// 新星爆炸结束方法
    /// </summary>
    public void ExplosionEnd()
    {
        Time.timeScale = 1f;
        Invoke("ShowFinalUI", 1f);
    }

    private void ShowFinalUI()
    {
        // 设置对应UI物体激活
    }
    
    /// <summary>
    /// 触发Boss移动
    /// </summary>
    /// <param name="direction">移动方向</param>
    public void MoveBoss(BossAnimeType direction)
    {
        if (bossAnime != null)
        {
            bossAnime.SetState(direction);
        }
    }

    /// <summary>
    /// 显示冰翼
    /// </summary>
    public void ShowPinion()
    {
        StartCoroutine(ShowIcePinion());
    }
    private IEnumerator ShowIcePinion()
    {

        if (IcePinion_left != null && IcePinion_right != null)
        {
            Color color = IcePinion_left.color;
            color.a = 0f;
            IcePinion_left.color = color;
            IcePinion_right.color = color;
        }
        float duration = 2f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float alpha = Mathf.Lerp(0f, 0.3f, t);
            if (IcePinion_left != null && IcePinion_right != null)
            {
                Color color = IcePinion_left.color;
                color.a = alpha;
                IcePinion_left.color = color;
                IcePinion_right.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void SetAnimeState()
    {
        animator.SetBool("isAnime", false);
    }
}

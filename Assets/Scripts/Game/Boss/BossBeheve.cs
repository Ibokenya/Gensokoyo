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
    public BossUI bossUI;
    public BossAnime bossAnime;
    public GameObject card_UI;
    public CardUI cardUI;
    public GameObject BossBG;
    public ChangeBG changeBG;
    
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
    private bool hasEndedFinalCard = false;
    
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
            UI.SetActive(true);
            card_UI.SetActive(true);
            bossAnime.ShowHP();
            bossUI.SetCardTime(16f);
            hasActivatedUI = true;
        }

        // 时间为5秒时，激活none1
        if (currentTime >= 5f && currentTime < 6f && !hasActivatedNone1)
        {
            if (none1Script != null)
            {
                none1Script.enabled = true;
                cardUI.SetCard(0);
                cardUI.SetCardName("-170℃");
                cardUI.SetCardColor(0.8f);
                Debug.Log("激活none1");
            }
            hasActivatedNone1 = true;
        }
        
        // 时间为21秒时，调用None1的CheckOver()方法
        if (currentTime >= 21f && currentTime < 22f && !hasCalledNone1CheckOver)
        {
            // 由于none1现在是空的，暂时注释掉
            // if (none1Script != null)
            // {
            //     none1Script.CheckOver();
            // }
            Debug.Log("调用none1.CheckOver()");
            bossUI.SetCardTime(23f);
            hasCalledNone1CheckOver = true;
        }
        
        // 时间为22秒时，禁用none1激活card1
        if (currentTime >= 22f && currentTime < 23f && !hasActivatedCard1)
        {
            if (none1Script != null)
            {
                none1Script.enabled = false;
                Debug.Log("禁用none1");
            }
            if (card1Script != null)
            {
                card1Script.enabled = true;
                cardUI.SetCard(1);
                cardUI.SetCardName("-220℃· \n冰冷彗星带");
                cardUI.SetCardColor(0.6f);
                Debug.Log("激活card1");
            }
            hasActivatedCard1 = true;
        }
        
        // 时间为45秒时，调用card1的checkover
        if (currentTime >= 45f && currentTime < 46f && !hasCalledCard1CheckOver)
        {
            // 由于card1现在是空的，暂时注释掉
            // if (card1Script != null)
            // {
            //     card1Script.CheckOver();
            // }
            Debug.Log("调用card1.CheckOver()");
            bossUI.SetCardTime(12f);
            hasCalledCard1CheckOver = true;
        }
        
        // 时间为46秒，禁用card1激活none2
        if (currentTime >= 46f && currentTime < 47f && !hasActivatedNone2)
        {
            if (card1Script != null)
            {
                card1Script.enabled = false;
                Debug.Log("禁用card1");
            }
            if (none2Script != null)
            {
                none2Script.enabled = true;
                cardUI.SetCard(2);
                cardUI.SetCardName("-260℃");
                cardUI.SetCardColor(0.4f);
                Debug.Log("激活none2");
            }
            hasActivatedNone2 = true;
        }
        
        // 时间为58秒，调用none2的checkover
        if (currentTime >= 58f && currentTime < 59f && !hasCalledNone2CheckOver)
        {
            // 由于none2现在是空的，暂时注释掉
            // if (none2Script != null)
            // {
            //     none2Script.CheckOver();
            // }
            Debug.Log("调用none2.CheckOver()");
            hasCalledNone2CheckOver = true;
        }
        
        // 时间为59秒，禁用none2激活card2
        if (currentTime >= 59f && currentTime < 60f && !hasActivatedCard2)
        {
            if (none2Script != null)
            {
                none2Script.enabled = false;
                Debug.Log("禁用none2");
            }
            if (card2Script != null)
            {
                card2Script.enabled = true;
                cardUI.SetCard(3);
                cardUI.SetCardName("-270℃· \n宇宙微波辐射");
                cardUI.SetCardColor(0.2f);
                Debug.Log("激活card2");
            }
            hasActivatedCard2 = true;
        }
        
        // 时间为83秒，调用card2的checkover
        if (currentTime >= 83f && currentTime < 84f && !hasCalledCard2CheckOver)
        {
            // 由于card2现在是空的，暂时注释掉
            // if (card2Script != null)
            // {
            //     card2Script.CheckOver();
            // }
            Debug.Log("调用card2.CheckOver()");
            hasCalledCard2CheckOver = true;
        }
        
        // 时间为84秒，禁用card2并调用BgAndBallon方法
        if (currentTime >= 84f && currentTime < 85f && !hasCalledBgAndBallon)
        {
            if (card2Script != null)
            {
                card2Script.enabled = false;
                Debug.Log("禁用card2");
            }
            BgAndBallon();
            Debug.Log("调用BgAndBallon方法");
            hasCalledBgAndBallon = true;
        }
        
        // 时间为89秒，调用FinalAnime方法
        if (currentTime >= 89f && currentTime < 90f && !hasCalledFinalAnime)
        {
            FinalAnime();
            cardUI.SetCard(4);
            cardUI.SetCardName("-273.15℃· \n然后分子便不再运动了");
            cardUI.SetCardColor(0f);
            Debug.Log("调用FinalAnime方法");
            hasCalledFinalAnime = true;
        }
        
        // 时间为91秒，激活finalcard
        if (currentTime >= 91f && currentTime < 92f && !hasActivatedFinalCard)
        {
            if (finalCardScript != null)
            {
                finalCardScript.enabled = true;
                Debug.Log("激活finalCard");
            }
            hasActivatedFinalCard = true;
        }
        
        // 时间为138秒，禁用finalcard，并调用AllOver方法
        if (currentTime >= 138f && currentTime < 139f && !hasEndedFinalCard)
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
        // 空方法，内部不实现
        Debug.Log("调用BgAndBallon方法");
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
    /// </summary>
    private void AllOver()
    {
        // 空方法，内部不实现
        Debug.Log("调用AllOver方法");
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
}

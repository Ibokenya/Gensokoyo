using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpellCardEffect : MonoBehaviour
{
    public List<GameObject> effects;// 总共四种符卡特效——灵梦常规，灵梦决死，魔理沙常规，魔理沙决死

    public Animator animator;

    private bool isHitDelayActive = false; // 是否处于受击延迟状态
    private readonly float hitDelayTime = 0.5f; // 受击延迟时间（现实时间）
    private Coroutine hitDelayCoroutine;
    public CardsRotate cardsRotate; // 引用CardsRotate脚本
    private bool isAnimating = false; // 是否正在播放动画
    public AudioClip BeHitClip;//中弹音效clip
    public AudioClip DelayClip;//决死延迟音效clip


    void Update()
    {
        if(Global_GameManager.Instance.state == State.Stop)
        {
            return;
        }
        // 处理技能释放
        if (Input.GetKeyDown(KeyCode.X))
        {
            if(Global_GameManager.Instance.BombCount <= 0)// 检查是否有符卡可用
            {
                Debug.Log("没有符卡可用");
                return;
            }
            if(isAnimating)
            {
                Debug.Log("正在播放动画");
                return;
            }
            Global_GameManager.Instance.SubBomb(1);// 减少符卡数量
            if (isHitDelayActive)
            {
                isAnimating = true;
                // 受击时释放特殊技能
                ReleaseSpecialSpellCard();
            }
            else
            {
                isAnimating = true;
                // 正常释放技能
                ReleaseNormalSpellCard();
            }
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
            Global_GameManager.Instance.AddBomb(1);
        }
    }

    /// <summary>
    /// 正常释放技能
    /// </summary>
    public void ReleaseNormalSpellCard()
    {
        // 设置无敌状态
        Global_GameManager.Instance.state = State.NoDead;
        
        if (Global_GameManager.Instance.character == Character.Reimu)
        {
            // 灵梦常规技能，AnimeIndex为1
            animator.SetInteger("AnimeIndex", 1);
        }
        else if (Global_GameManager.Instance.character == Character.Marisa)
        {
            // 魔理沙常规技能，AnimeIndex为3
            animator.SetInteger("AnimeIndex", 3);
        }
    }

    /// <summary>
    /// 受击时释放特殊技能
    /// </summary>
    public void ReleaseSpecialSpellCard()
    {
        // 取消受击延迟
        if (hitDelayCoroutine != null)
        {
            StopCoroutine(hitDelayCoroutine);
        }
        isHitDelayActive = false;
        Time.timeScale = 1f;
        
        // 设置无敌状态
        Global_GameManager.Instance.state = State.NoDead;

        if (Global_GameManager.Instance.character == Character.Reimu)
        {
            // 灵梦决死技能，AnimeIndex为2
            animator.SetInteger("AnimeIndex", 2);
        }
        else if (Global_GameManager.Instance.character == Character.Marisa)
        {
            // 魔理沙决死技能，AnimeIndex为4
            animator.SetInteger("AnimeIndex", 4);
        }
    }

    /// <summary>
    /// 开始受击延迟
    /// </summary>
    public void StartHitDelay()
    {
        // 只有在低速移动（按下shift）且有符卡时才触发延迟
        if (Input.GetKey(KeyCode.LeftShift) && Global_GameManager.Instance.BombCount > 0)
        {
            isHitDelayActive = true;
            Time.timeScale = 0f;
            Debug.Log("进入决死预备状态");
            if(DelayClip != null)
            {
                // 播放决死延迟音效
                Global_AudioManager.Instance.PlaySFX(DelayClip);
            }
            else
            {
                Debug.Log("没有决死延迟音效");
            }
            hitDelayCoroutine = StartCoroutine(HitDelayCoroutine());
        }
        else
        {
            if(BeHitClip != null)
            {
                // 播放决死延迟音效
                Global_AudioManager.Instance.PlaySFX(BeHitClip);
            }
            // 没有按shift或没有符卡时，直接执行死亡逻辑
            Global_GameManager.Instance.SubLeftLife();
        }
    }

    /// <summary>
    /// 受击延迟协程
    /// </summary>
    private IEnumerator HitDelayCoroutine()
    {
        yield return new WaitForSecondsRealtime(hitDelayTime);
        
        if (isHitDelayActive)
        {
            Time.timeScale = 1f;
            if(BeHitClip != null)
            {
                // 播放中弹音效
                Global_AudioManager.Instance.PlaySFX(BeHitClip);
            }
            isHitDelayActive = false;
            // 执行正常死亡逻辑
            Global_GameManager.Instance.SubLeftLife();
        }
    }

    /// <summary>
    /// 技能动画结束回调
    /// </summary>
    public void OnSpellCardAnimationEnd()
    {
        isAnimating = false;
        // 技能动画结束后，将AnimeIndex重置为0
        animator.SetInteger("AnimeIndex", 0);
        // 解除无敌状态
        Global_GameManager.Instance.state = State.Gaming;
    }

    public void SpellCardFadeOut()
    {
        // 调用CardsRotate脚本的FadeOut方法，淡出符卡
        if (cardsRotate != null)
        {
            cardsRotate.FadeOut();
        }
    }
}

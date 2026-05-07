using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 符卡效果管理器
/// 负责管理4种符卡技能的释放和协调
/// </summary>
public class SpellCardEffect : MonoBehaviour
{
    [Header("4种符卡攻击设置")]
    public List<GameObject> effects;// 总共四种符卡特效——灵梦常规，灵梦决死，魔理沙常规，魔理沙决死

    [Header("4个子脚本引用")]
    public ReimuNormal reimuNormal; // 灵梦常规技能脚本
    public ReimuSuper reimuSuper;   // 灵梦决死技能脚本
    public MarisaNormal marisaNormal; // 魔理沙常规技能脚本
    public MarisaSuper marisaSuper;   // 魔理沙决死技能脚本
    
    [Header("4个技能空物体引用")]
    public GameObject reimuNormalObject; // 灵梦常规技能空物体
    public GameObject reimuSuperObject;   // 灵梦决死技能空物体
    public GameObject marisaNormalObject; // 魔理沙常规技能空物体
    public GameObject marisaSuperObject;   // 魔理沙决死技能空物体

    [Header("音效设置")]
    public AudioClip BeHitClip;//中弹音效clip
    public AudioClip DelayClip;//决死延迟音效clip

    [Header("脚本引用")]
    public ClearAllBullet clearAllBullet;
    public CardsRotate cardsRotate; // 引用CardsRotate脚本
    public Graze graze; // 引用Graze脚本
    public PlayerAnime playerAnime; // 引用PlayerAnime脚本
    public EvilEyeAttack evilEyeAttack; // 引用EvilEyeAttack脚本
    public EvilShadow evilShadow; // 引用EvilShadow脚本

    [Header("物体引用")]
    public GameObject player;// 玩家物体

    private bool isHitDelayActive = false; // 是否处于受击延迟状态
    private readonly float hitDelayTime = 1f; // 受击延迟时间（现实时间）
    private Coroutine hitDelayCoroutine;
    private bool isAnimating = false; // 是否正在播放动画
    
    // 存储音乐状态
    private string currentBGMName = "";
    private float currentBGMPosition = 0f;
    
    // 存储魔理沙决死前的状态
    private bool wasEvilEyeActive = false; // 恶魔之眼是否激活
    private bool wasEvilShadowActive = false; // 暗影视界是否激活

    void OnDisable()
    {
        // 确保在禁用时取消受击延迟
        if (hitDelayCoroutine != null)
        {
            StopCoroutine(hitDelayCoroutine);
        }
        isHitDelayActive = false;
        isAnimating = false;
    }

    void Update()
    {
        if (Global_GameManager.Instance.state == State.Pause)
        {
            return;
        }

        // 处理技能释放
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (Global_GameManager.Instance.BombCount <= 0)// 检查是否有符卡可用
            {
                Debug.Log("没有符卡可用");
                return;
            }
            if (isAnimating)
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
    }

    #region 释放技能相关

    /// <summary>
    /// 正常释放技能
    /// </summary>
    public void ReleaseNormalSpellCard()
    {
        // 设置无敌状态
        Global_GameManager.Instance.state = State.NoDead;

        if (Global_GameManager.Instance.character == Character.Reimu)
        {
            Debug.Log("释放灵梦常规技能");
            // 激活灵梦常规技能空物体
            if (reimuNormalObject != null)
            {
                reimuNormalObject.SetActive(true);
            }
            // 激活灵梦常规技能脚本
            if (reimuNormal != null)
            {
                reimuNormal.IsAnime = true;
            }
        }
        else if (Global_GameManager.Instance.character == Character.Marisa)
        {
            Debug.Log("释放魔理沙常规技能");
            // 激活魔理沙常规技能空物体
            if (marisaNormalObject != null)
            {
                marisaNormalObject.SetActive(true);
            }
            // 激活魔理沙常规技能脚本
            if (marisaNormal != null)
            {
                marisaNormal.IsAnime = true;
            }
        }
    }

    /// <summary>
    /// 受击时释放特殊符卡
    /// </summary>
    public void ReleaseSpecialSpellCard()
    {
        Debug.Log("释放特殊符卡");
        // 强制停止擦弹效果
        if (graze != null)
        {
            graze.ForceStopGrazeSound();
        }
        // 取消受击延迟
        if (hitDelayCoroutine != null)
        {
            StopCoroutine(hitDelayCoroutine);
        }
        isHitDelayActive = false;
        Time.timeScale = 1f;
        
        // 停止音乐并记录状态
        if(Global_AudioManager.Instance != null)
        {
            currentBGMName = Global_AudioManager.Instance.GetCurrentBGMName();
            currentBGMPosition = Global_AudioManager.Instance.GetCurrentBGMPosition();
            
            Global_AudioManager.Instance.StopBGM();
            Global_AudioManager.Instance.StopAllSFX();
        }

        // 存储魔理沙决死前的状态
        if (Global_GameManager.Instance.character == Character.Marisa)
        {
            // 检查恶魔之眼是否激活
            if (evilEyeAttack != null)
            {
                wasEvilEyeActive = evilEyeAttack.isFadeInComplete;
            }
            // 检查暗影视界是否激活
            if (evilShadow != null && evilShadow.GetComponent<SpriteRenderer>() != null)
            {
                wasEvilShadowActive = evilShadow.isStartFadeIn;
            }
        }

        // 设置无敌状态
        Global_GameManager.Instance.state = State.NoDead;

        if (Global_GameManager.Instance.character == Character.Reimu)
        {
            Debug.Log("释放了灵梦决死技能");
            // 激活灵梦决死技能空物体
            if (reimuSuperObject != null)
            {
                reimuSuperObject.SetActive(true);
            }
            // 激活灵梦决死技能脚本
            if (reimuSuper != null)
            {
                reimuSuper.IsAnime = true;
            }
        }
        else if (Global_GameManager.Instance.character == Character.Marisa)
        {
            Debug.Log("释放了魔理沙决死技能");
            // 激活魔理沙决死技能空物体
            if (marisaSuperObject != null)
            {
                marisaSuperObject.SetActive(true);
            }
            // 激活魔理沙决死技能脚本
            if (marisaSuper != null)
            {
                marisaSuper.IsAnime = true;
            }
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
            if (DelayClip != null)
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
            if (BeHitClip != null)
            {
                // 播放中弹音效
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

        if (isHitDelayActive && Global_GameManager.Instance.state != State.NoDead)
        {
            Time.timeScale = 1f;
            if (BeHitClip != null)
            {
                // 播放中弹音效
                Global_AudioManager.Instance.PlaySFX(BeHitClip);
            }
            isHitDelayActive = false;
            // 执行正常死亡逻辑
            Global_GameManager.Instance.SubLeftLife();
        }
    }

    #endregion

    /// <summary>
    /// 子脚本动画结束回调
    /// </summary>
    /// <param name="skillType">技能类型：1-灵梦常规，2-灵梦决死，3-魔理沙常规，4-魔理沙决死</param>
    public void OnChildAnimationEnd(int skillType)
    {
        isAnimating = false;

        // 解除无敌状态
        Global_GameManager.Instance.state = State.Gaming;

        // 检测玩家按键状态并重置动画状态
        ResetPlayerAnimationState();

        // 禁用对应技能空物体
        switch (skillType)
        {
            case 1: // 灵梦常规
                if (reimuNormalObject != null)
                {
                    reimuNormalObject.SetActive(false);
                }
                break;
            case 2: // 灵梦决死
                if (reimuSuperObject != null)
                {
                    reimuSuperObject.SetActive(false);
                }
                // 恢复音乐
                ResumeMusic();
                break;
            case 3: // 魔理沙常规
                if (marisaNormalObject != null)
                {
                    marisaNormalObject.SetActive(false);
                }
                break;
            case 4: // 魔理沙决死
                if (marisaSuperObject != null)
                {
                    marisaSuperObject.SetActive(false);
                }
                // 恢复音乐
                ResumeMusic();
                break;
        }

        Debug.Log($"技能 {skillType} 动画结束");
    }
    
    /// <summary>
    /// 恢复音乐播放
    /// </summary>
    private void ResumeMusic()
    {
        // 恢复之前的BGM
        if(Global_AudioManager.Instance != null && !string.IsNullOrEmpty(currentBGMName))
        {
            Global_AudioManager.Instance.PlayBGM(currentBGMName);
            Global_AudioManager.Instance.SetBGMPosition(currentBGMPosition);
        }
    }

    /// <summary>
    /// 重置玩家动画状态
    /// 检测当前按键状态并更新玩家动画
    /// </summary>
    private void ResetPlayerAnimationState()
    {
        if (player != null)
        {
            PlayerAnime playerAnime = player.GetComponent<PlayerAnime>();
            GunAnime gunAnime = player.GetComponent<GunAnime>();
            
            if (playerAnime != null)
            {
                // 检测左shift按键状态
                bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);
                // 根据shift按键状态设置移速和动画
                if (isShiftPressed)
                {
                    // 低速态
                    playerAnime.SetMoveSpeed(playerAnime.MoveSpeed * 0.4f);
                    playerAnime.StartPandingAnime();
                }
                else
                {
                    // 快速态
                    playerAnime.SetMoveSpeed(playerAnime.MoveSpeed);
                    playerAnime.StopPandingAnime();
                }
                // 重置魔理沙的移速和动画状态
                if (Global_GameManager.Instance.character == Character.Marisa)
                {
                    // 重置技能减速状态
                    MarisaNormal.IsSkillSlowDown = false;
                    
                    // 重置恶魔之眼和暗影视界
                    ResetEvilEffects();
                }
            }
            
            // 重置GunAnime状态
            if (gunAnime != null && Global_GameManager.Instance.character == Character.Marisa)
            {
                // 检测左shift按键状态
                bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);
                
                // 重置魔法状态
                gunAnime.isExitingMagic = false;
                
                // 根据shift按键状态切换武器
                if (isShiftPressed)
                {
                    // 按下shift，切换到七曜魔法态
                    gunAnime.Index = 2;
                }
                else
                {
                    // 未按下shift，切换到魔理沙常态
                    gunAnime.Index = 1;
                }
            }           
            // 执行武器切换
            gunAnime.SwitchGun();
            gunAnime.UpdateGunPos();
        }
    }
    
    /// <summary>
    /// 重置恶魔之眼和暗影视界效果
    /// </summary>
    private void ResetEvilEffects()
    {
        // 检查是否需要重置恶魔之眼
        if (evilEyeAttack != null && wasEvilEyeActive)
        {
            // 开始恶魔之眼淡出
            evilEyeAttack.StartFadeOut();
        }
        
        // 检查是否需要重置暗影视界
        if (evilShadow != null && wasEvilShadowActive)
        {
            // 开始暗影视界淡出
            evilShadow.StartFadeOut();
        }
        
        // 重置存储的状态
        wasEvilEyeActive = false;
        wasEvilShadowActive = false;
    }

    /// <summary>
    /// 清除屏幕子弹（供子脚本调用）
    /// </summary>
    public void ClearAllBullet()
    {
        if (clearAllBullet != null)
        {
            clearAllBullet.ClearScreenBullet(false);
        }
    }
}

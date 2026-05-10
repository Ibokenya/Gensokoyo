using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreezeSystem : MonoBehaviour
{
    [Header("冻结系统组件引用")]
    public Image FrozenBar;//冻结进度条
    public Image FrozenEffect;//冻结特效
    public SpriteRenderer FrozenSprite;//冻结特效精灵

    [Header("冻结相关参数")]
    public AudioClip freezeSound;//冻结音效
    public bool IsStop;//是否停止冻结系统

    [Header("玩家动画引用")]
    public PlayerAnime playerAnime; // 引用玩家动画脚本

    private float FrozenDegree;//冻结进度 0-1
    public bool IsFrozen;//是否冻结（公开供外部检测）

    public float FrozenScale = 1f;//冻结缩放比例(默认1，有的符卡程度会加快冻结速度)

    private const float BASE_FROZEN_SPEED = 0.04f;//基础冻结速度，每秒上涨4%

    private void Awake()
    {
        FrozenDegree = 0f;
        IsFrozen = false;
    }

    void Update()
    {
        if (!IsFrozen && Global_GameManager.Instance.state == State.Gaming && !IsStop)
        {
            // 随时间增长冰冻进度
            FrozenDegree += BASE_FROZEN_SPEED * FrozenScale * Time.deltaTime;
            FrozenDegree = Mathf.Clamp01(FrozenDegree);

            Global_GameManager.Instance.SetSpeedScale(1f-(0.5f*FrozenDegree));
            // 更新UI
            UpdateUI();

            // 检查是否达到冻结条件
            if (FrozenDegree >= 1f && !IsFrozen)
            {
                Freeze();
            }
        }
        if(Global_GameManager.Instance.state == State.SpellCard || 
        Global_GameManager.Instance.state == State.Reincarnation)
        {
            FrozenDegree = 0f;
        }
    }

    /// <summary>
    /// 更新冻结相关UI
    /// </summary>
    private void UpdateUI()
    {
        // 更新冻结进度条
        if (FrozenBar != null)
        {
            FrozenBar.fillAmount = FrozenDegree;
        }

        // 计算透明度（0-0.2f的平滑插值）
        float alpha = Mathf.Lerp(0f, 0.2f, FrozenDegree);

        // 更新冻结特效透明度
        if (FrozenEffect != null)
        {
            Color effectColor = FrozenEffect.color;
            effectColor.a = alpha;
            FrozenEffect.color = effectColor;
        }

        // 更新冻结精灵透明度
        if (FrozenSprite != null)
        {
            Color spriteColor = FrozenSprite.color;
            spriteColor.a = alpha;
            FrozenSprite.color = spriteColor;
        }
    }

    /// <summary>
    /// 减少冻结进度（擦弹时调用）
    /// </summary>
    /// <param name="amount">减少的量（默认0.01即1%）</param>
    public void ReduceFrozenDegree(float amount = 0.01f)
    {
        if (!IsFrozen)
        {
            FrozenDegree -= amount;
            FrozenDegree = Mathf.Max(0f, FrozenDegree);
            UpdateUI();
        }
    }

    /// <summary>
    /// 增加冻结进度（冰云碰撞时调用）
    /// </summary>
    /// <param name="amount">增加的量</param>
    public void IncreaseFrozenDegree(float amount)
    {
        if (!IsFrozen)
        {
            FrozenDegree += amount;
            FrozenDegree = Mathf.Min(1f, FrozenDegree);
            UpdateUI();
            
            // 检查是否达到冻结条件
            if (FrozenDegree >= 1f)
            {
                Freeze();
            }
        }
    }

    /// <summary>
    /// 冻结方法
    /// </summary>
    private void Freeze()
    {
        IsFrozen = true;
        StartCoroutine(FreezeCoroutine());
        Color color = FrozenSprite.color;
        color.a = 0.4f;
        FrozenSprite.color = color;
    }

    private IEnumerator FreezeCoroutine()
    {
        float duration =0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color color = FrozenEffect.color;
            float alpha = color.a;
            color.a = Mathf.Lerp(alpha, 0.6f, elapsedTime / duration);
            FrozenEffect.color = color;
            yield return null;
        }
        // 播放冻结音效
        if (freezeSound != null)
        {
            Global_AudioManager.Instance.PlaySFX(freezeSound);
        }      
        // 激活PlayerAnime中的Ice物体并启动QTE
        if (playerAnime != null)
        {
            playerAnime.ActivateFrozenQTE();
        }
        Global_GameManager.Instance.state = State.Frozen;
    }

    /// <summary>
    /// 重置冻结系统
    /// </summary>
    public void ResetFreeze()
    {
        FrozenDegree = 0.3f;
        IsFrozen = false;
        UpdateUI();
    }

    /// <summary>
    /// 设置冻结缩放比例
    /// </summary>
    /// <param name="scale">缩放比例值</param>
    public void SetFrozenScale(float scale)
    {
        FrozenScale = scale;
    }

    /// <summary>
    /// 重置冻结缩放比例为默认值1
    /// </summary>
    public void ResetFrozenScale()
    {
        FrozenScale = 1f;
    }
}
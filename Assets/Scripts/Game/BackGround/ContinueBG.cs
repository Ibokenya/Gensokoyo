using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinueBG : MonoBehaviour
{
    [Header("背景图片物体引用")]
    public GameObject Star;
    public GameObject DarkStar;
    public GameObject DreamRoad;
    public GameObject DarkCloud;

    [Header("过渡速度配置")]
    public float StartargetSpeedY = 0.01f;
    public float DarkStartargetSpeedY = 0.012f;
    public float DreamRoadargetSpeedY = -0.1f;
    public float transitionDuration = 5f;// 过渡时间
    public float recoveryDuration = 3f;// 恢复时间

    private Vector2 starOriginalSpeed = new (0,0.1f);
    private Vector2 darkstarOriginalSpeed = new (0,0.12f);
    private Vector2 dreamroadOriginalSpeed = new (0,-1f);

    
    // 存储协程引用，用于控制协程的执行
    private Coroutine transitionCoroutine;
    private Coroutine recoveryCoroutine;

    private IEnumerator TransitionToTargetSpeed()
    {
        float elapsedTime = 0f;
        float startAlpha = DarkCloud != null ? DarkCloud.GetComponent<BGImageScroll>().Alpha : 0f;
        float targetAlpha = 0f;

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;

            if (Star != null)
            {
                Vector2 currentSpeed = Star.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(starOriginalSpeed.y, StartargetSpeedY, t);
                Star.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }

            if (DarkStar != null)
            {
                Vector2 currentSpeed = DarkStar.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(darkstarOriginalSpeed.y, DarkStartargetSpeedY, t);
                DarkStar.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }

            if (DreamRoad != null)
            {
                Vector2 currentSpeed = DreamRoad.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(dreamroadOriginalSpeed.y, DreamRoadargetSpeedY, t);
                DreamRoad.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }

            if (DarkCloud != null)
            {
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                DarkCloud.GetComponent<BGImageScroll>().Alpha = currentAlpha;
                DarkCloud.GetComponent<BGImageScroll>().GetMaterialInstance().SetFloat("_Alpha", currentAlpha);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        EnsureFinalValues();
    }

    private IEnumerator RecoveryToOriginalSpeed()
    {
        float elapsedTime = 0f;
        float startAlpha = DarkCloud != null ? DarkCloud.GetComponent<BGImageScroll>().Alpha : 0f;
        float dreamroadStartAlpha = DreamRoad != null ? DreamRoad.GetComponent<BGImageScroll>().Alpha : 1f;

        while (elapsedTime < recoveryDuration)
        {
            float t = elapsedTime / recoveryDuration;

            if (Star != null)
            {
                Vector2 currentSpeed = Star.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(StartargetSpeedY, starOriginalSpeed.y, t);
                Star.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }

            if (DarkStar != null)
            {
                Vector2 currentSpeed = DarkStar.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(DarkStartargetSpeedY, darkstarOriginalSpeed.y, t);
                DarkStar.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }

            // 不再恢复dreamroad的速度，而是将其透明度淡出
            if (DreamRoad != null)
            {
                float currentAlpha = Mathf.Lerp(dreamroadStartAlpha, 0f, t);
                DreamRoad.GetComponent<BGImageScroll>().Alpha = currentAlpha;
                DreamRoad.GetComponent<BGImageScroll>().GetMaterialInstance().SetFloat("_Alpha", currentAlpha);
            }

            if (DarkCloud != null)
            {
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, t);
                DarkCloud.GetComponent<BGImageScroll>().Alpha = currentAlpha;
                DarkCloud.GetComponent<BGImageScroll>().GetMaterialInstance().SetFloat("_Alpha", currentAlpha);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        EnsureFinalRecoveryValues();
    }

    /// <summary>
    /// 确保最终速度为目标速度
    /// </summary>
    private void EnsureFinalValues()
    {
        if (Star != null)
        {
            Vector2 finalSpeed = Star.GetComponent<BGImageScroll>().scrollSpeed;
            finalSpeed.y = StartargetSpeedY;
            Star.GetComponent<BGImageScroll>().scrollSpeed = finalSpeed;
        }

        if (DarkStar != null)
        {
            Vector2 finalSpeed = DarkStar.GetComponent<BGImageScroll>().scrollSpeed;
            finalSpeed.y = DarkStartargetSpeedY;
            DarkStar.GetComponent<BGImageScroll>().scrollSpeed = finalSpeed;
        }

        if (DreamRoad != null)
        {
            Vector2 finalSpeed = DreamRoad.GetComponent<BGImageScroll>().scrollSpeed;
            finalSpeed.y = DreamRoadargetSpeedY;
            DreamRoad.GetComponent<BGImageScroll>().scrollSpeed = finalSpeed;
        }

        if (DarkCloud != null)
        {
            DarkCloud.GetComponent<BGImageScroll>().Alpha = 0f;
            DarkCloud.GetComponent<BGImageScroll>().GetMaterialInstance().SetFloat("_Alpha", 0f);
        }
    }

    /// <summary>
    /// 确保恢复到原始速度
    /// </summary>
    private void EnsureFinalRecoveryValues()
    {
        if (Star != null)
        {
            Star.GetComponent<BGImageScroll>().scrollSpeed = starOriginalSpeed;
        }

        if (DarkStar != null)
        {
            DarkStar.GetComponent<BGImageScroll>().scrollSpeed = darkstarOriginalSpeed;
        }

        // 不再恢复dreamroad的速度，而是确保其透明度为0
        if (DreamRoad != null)
        {
            DreamRoad.GetComponent<BGImageScroll>().Alpha = 0f;
            DreamRoad.GetComponent<BGImageScroll>().GetMaterialInstance().SetFloat("_Alpha", 0f);
        }

        if (DarkCloud != null)
        {
            DarkCloud.GetComponent<BGImageScroll>().Alpha = 0f;
            DarkCloud.GetComponent<BGImageScroll>().GetMaterialInstance().SetFloat("_Alpha", 0f);
        }
    }


    /// <summary>
    /// 开始过渡到目标速度（5秒内平滑降速，同时DarkCloud淡出）
    /// </summary>
    public void StartTransition()
    {
        // 停止之前可能正在运行的协程
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        if (recoveryCoroutine != null)
        {
            StopCoroutine(recoveryCoroutine);
        }
        
        // 启动过渡协程
        transitionCoroutine = StartCoroutine(TransitionToTargetSpeed());
    }

    /// <summary>
    /// 恢复到原始速度（3秒内平滑恢复）
    /// </summary>
    public void StartRecovery()
    {
        // 停止之前可能正在运行的协程
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        if (recoveryCoroutine != null)
        {
            StopCoroutine(recoveryCoroutine);
        }
        
        // 启动恢复协程
        recoveryCoroutine = StartCoroutine(RecoveryToOriginalSpeed());
    }
}

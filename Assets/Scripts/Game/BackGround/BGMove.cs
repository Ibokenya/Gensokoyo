using System.Collections;
using UnityEngine;

public class BGMove : MonoBehaviour
{
    [Header("背景图片物体引用")]
    public GameObject Star;// 群星
    public GameObject DarkStar;// 黑暗群星
    public GameObject DreamRoad;// 槐安大道
    
    public GameObject DarkCloud;// 黑暗物质

    [Header("粒子系统引用")]
    public ParticleSystem SpaceDust;// 宇宙尘埃粒子系统

    [Header("鸟鸣音效")]
    public AudioClip BirdSound;// 鸟鸣音效
    [Header("脚本引用")]
    public ContinueBG continueBG;// 继续背景脚本

    // 目标速度常量
    private const float STAR_TARGET_SPEED = 1.0f; // 群星目标速度
    private const float DARKSTAR_TARGET_SPEED = 1.2f; // 黑暗群星目标速度
    private const float DREAMROAD_TARGET_SPEED = -10f; // 槐安大道目标速度

    // 存储原始速度
    private Vector2 starOriginalSpeed;
    private Vector2 darkstarOriginalSpeed;
    private Vector2 dreamroadOriginalSpeed;
    private Vector2 darkcloudOriginalSpeed;

    // 存储原始粒子最大数量
    private int originalMaxParticles;
    
    // 存储协程引用
    private Coroutine controlStarSpeedCoroutine;
    private Coroutine controlDarkCloudCoroutine;
    private Coroutine controlParticleSystemCoroutine;

    // 动画事件触发方法（48s时触发）
    public void StartAnimationEvents()
    {
        // 48s时启动第一个协程：控制star、darkstar、dreamroad的速度
        controlStarSpeedCoroutine = StartCoroutine(ControlStarSpeed());
        // 48s时启动第二个协程：控制darkcloud的透明度和速度
        controlDarkCloudCoroutine = StartCoroutine(ControlDarkCloud());
        // 48s时启动第三个协程：控制粒子系统最大数量
        controlParticleSystemCoroutine = StartCoroutine(ControlParticleSystem());
    }

    // 第一个协程：控制star、darkstar、dreamroad的速度平滑过渡
    private IEnumerator ControlStarSpeed()
    {
        // 获取原始速度
        if (Star != null) starOriginalSpeed = Star.GetComponent<BGImageScroll>().scrollSpeed;
        if (DarkStar != null) darkstarOriginalSpeed = DarkStar.GetComponent<BGImageScroll>().scrollSpeed;
        if (DreamRoad != null) dreamroadOriginalSpeed = DreamRoad.GetComponent<BGImageScroll>().scrollSpeed;

        // 35秒内平滑过渡到目标速度
        float duration = 35f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            if (Star != null)
            {
                Vector2 currentSpeed = Star.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(starOriginalSpeed.y, STAR_TARGET_SPEED, t);
                Star.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }
            
            if (DarkStar != null)
            {
                Vector2 currentSpeed = DarkStar.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(darkstarOriginalSpeed.y, DARKSTAR_TARGET_SPEED, t);
                DarkStar.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }
            
            if (DreamRoad != null)
            {
                Vector2 currentSpeed = DreamRoad.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(dreamroadOriginalSpeed.y, DREAMROAD_TARGET_SPEED, t);
                DreamRoad.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 等待35秒
        yield return new WaitForSeconds(35f);

        // 35秒内平滑回到原速度
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            if (Star != null)
            {
                Vector2 currentSpeed = Star.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(STAR_TARGET_SPEED, starOriginalSpeed.y, t);
                Star.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }
            
            if (DarkStar != null)
            {
                Vector2 currentSpeed = DarkStar.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(DARKSTAR_TARGET_SPEED, darkstarOriginalSpeed.y, t);
                DarkStar.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }
            
            if (DreamRoad != null)
            {
                Vector2 currentSpeed = DreamRoad.GetComponent<BGImageScroll>().scrollSpeed;
                currentSpeed.y = Mathf.Lerp(DREAMROAD_TARGET_SPEED, dreamroadOriginalSpeed.y, t);
                DreamRoad.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Debug.Log("ControlStarSpeed协程完成");
    }

    // 第二个协程：控制darkcloud的透明度和速度变化
    private IEnumerator ControlDarkCloud()
    {
        // 获取原始速度
        if (DarkCloud != null) darkcloudOriginalSpeed = DarkCloud.GetComponent<BGImageScroll>().scrollSpeed;

        // 35秒内平滑将透明度上升到1f
        float duration = 35f;
        float elapsedTime = 0f;
        float startAlpha = DarkCloud != null ? DarkCloud.GetComponent<BGImageScroll>().Alpha : 0f;
        float targetAlpha = 0.3f; // 可根据测试需要修改此值

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            if (DarkCloud != null)
            {
                // 确保速度保持为原始速度
                DarkCloud.GetComponent<BGImageScroll>().scrollSpeed = darkcloudOriginalSpeed;
                               
                // 平滑调整透明度
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                DarkCloud.GetComponent<BGImageScroll>().Alpha = currentAlpha;
                // 更新材质透明度
                DarkCloud.GetComponent<BGImageScroll>().GetMaterialInstance().SetFloat("_Alpha", currentAlpha);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 等待35秒
        yield return new WaitForSeconds(35f);

        // 35秒内平滑将速度降低至0
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            if (DarkCloud != null)
            {
                Vector2 currentSpeed = Vector2.Lerp(darkcloudOriginalSpeed, Vector2.zero, t);
                DarkCloud.GetComponent<BGImageScroll>().scrollSpeed = currentSpeed;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Debug.Log("ControlDarkCloud协程完成");
    }

    // 第三个协程：控制粒子系统最大数量
    private IEnumerator ControlParticleSystem()
    {
        // 存储原始最大粒子数
        if (SpaceDust != null)
        {
            ParticleSystem.MainModule mainModule = SpaceDust.main;
            originalMaxParticles = (int)mainModule.maxParticles;
        }

        // 35秒期间，每秒令最大粒子数+1
        float duration = 35f;
        float elapsedTime = 0f;
        int currentMaxParticles = originalMaxParticles;

        while (elapsedTime < duration)
        {
            // 每秒递增一次
            if (SpaceDust != null)
            {
                ParticleSystem.MainModule mainModule = SpaceDust.main;
                currentMaxParticles++;
                mainModule.maxParticles = currentMaxParticles;
            }
            
            // 等待1秒
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }
    }

    public void AudioBirdSound()
    {
        Global_AudioManager.Instance.PlaySFX(BirdSound);
    }

    public void StartContinueBG()
    {
        // 停止自身的速度控制协程，避免与ContinueBG的控制冲突
        if (controlStarSpeedCoroutine != null)
        {
            StopCoroutine(controlStarSpeedCoroutine);
        }
        if (controlDarkCloudCoroutine != null)
        {
            StopCoroutine(controlDarkCloudCoroutine);
        }
        
        // 调用ContinueBG的StartTransition方法
        continueBG.StartTransition();
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AimPointAttack : MonoBehaviour
{
    [Header("旋转参数")]
    public float rotationSpeed = 180f; // 旋转速度，单位：度/秒

    [Header("魔法珠参数")]
    public float xMin = 20f; // x轴最小值
    public float xMax = 30f; // x轴最大值
    public float yMin = 21.8f; // y轴最小值
    public float yMax = 32.7f; // y轴最大值
    public float zMin = 34.4f; // z轴最小值
    public float zMax = 51.6f; // z轴最大值
    public float PearlSpeed = 20f; // 魔法珠飞行速度
    public float spawnDelayMin = 1f; // 生成魔法珠的最小延迟时间
    public float spawnDelayMax = 2f; // 生成魔法珠的最大延迟时间
    public float reuseDelayMin = 0.1f; // 再次召唤魔法珠的最小延迟时间
    public float reuseDelayMax = 0.5f; // 再次召唤魔法珠的最大延迟时间

    [Header("爆炸动画")]
    public Sprite[] markerSprites; // 瞄准点精灵数组（4张：1张瞄准点 + 3张爆炸动画）
    public float explosionFrameInterval = 10f; // 爆炸动画帧间隔

    private GameObject pearlPrefab; // 魔法珠预制体（通过脚本传递）
    private GameObject pearl; // 当前魔法珠实例
    private readonly float fadeInDuration = 0.5f; // 魔法珠的淡入时间
    private SpriteRenderer spriteRenderer; // 瞄准点的SpriteRenderer组件

    void OnDisable()
    {
        CancelInvoke();

        // 回收当前魔法珠实例
        if (pearl != null && Global_ObjectPool.Instance != null)
        {
            Global_ObjectPool.Instance.Recycle(pearl);
            pearl = null;
        }
    }

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && markerSprites != null && markerSprites.Length > 0)
        {
            spriteRenderer.sprite = markerSprites[0];
        }
    }

    /// <summary>
    /// 从对象池获取魔法珠
    /// </summary>
    private void SpawnPearl()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (pearlPrefab != null && Global_ObjectPool.Instance != null)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            pearl = Global_ObjectPool.Instance.GetObject(pearlPrefab, spawnPosition, Quaternion.identity);

            if (pearl != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(FadeInPearl(pearl));
            }
            else
            {
                Debug.LogWarning("[AimPointAttack] 无法从对象池获取魔法珠！");
            }
        }
        else
        {
            Debug.LogWarning("[AimPointAttack] pearlPrefab或Global_ObjectPool.Instance未设置！");
        }
    }

    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        float x;
        if (Random.value > 0.5f)
        {
            x = Random.Range(xMin, xMax);
        }
        else
        {
            x = Random.Range(-xMax, -xMin);
        }

        float y = Random.Range(yMin, yMax);
        float z = Random.Range(zMin, zMax);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// 魔法珠的淡入协程
    /// </summary>
    private IEnumerator FadeInPearl(GameObject pearlObj)
    {
        if (pearlObj == null) yield break;

        if (!pearlObj.TryGetComponent<SpriteRenderer>(out var sr)) yield break;

        Color originalColor = sr.color;
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // 每帧更新魔法珠状态，确保流畅移动
        UpdatePearl();
    }

    /// <summary>
    /// 更新魔法珠状态
    /// </summary>
    private void UpdatePearl()
    {
        if (pearl == null)
        {
            return;
        }

        Vector3 direction = (transform.position - pearl.transform.position).normalized;
        float distance = Vector3.Distance(pearl.transform.position, transform.position);

        if (distance < 0.5f)
        {
            int damage = CalculateDamage();
            DealDamageToEnemy(damage);

            if (Global_ObjectPool.Instance != null)
            {
                Global_ObjectPool.Instance.Recycle(pearl);
                pearl = null;
            }

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(PlayExplosionAnimation());
            }

            return;
        }

        pearl.transform.Rotate(0f, 0f, 360f * Time.deltaTime);
        Vector3 moveAmount = direction * PearlSpeed * Time.deltaTime;
        pearl.transform.position += moveAmount;
    }

    /// <summary>
    /// 计算伤害
    /// </summary>
    private int CalculateDamage()
    {
        int baseDamage = 120;
        int powerBonus = 0;

        if (Global_GameManager.Instance != null)
        {
            powerBonus = Global_GameManager.Instance.Power / 100 * 20;
        }

        return baseDamage + powerBonus;
    }

    /// <summary>
    /// 对敌人造成伤害
    /// </summary>
    private void DealDamageToEnemy(int damage)
    {
        if (transform.parent != null)
        {
            Enemy enemy = transform.parent.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Damage(damage);
            }
        }
    }

    /// <summary>
    /// 播放爆炸动画
    /// </summary>
    private IEnumerator PlayExplosionAnimation()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && markerSprites != null && markerSprites.Length >= 4)
        {
            for (int i = 1; i < 4; i++)
            {
                spriteRenderer.sprite = markerSprites[i];
                for (int j = 0; j < explosionFrameInterval; j++)
                {
                    yield return null;
                }
            }
            spriteRenderer.sprite = markerSprites[0];
        }

        float randomDelay = Random.Range(reuseDelayMin, reuseDelayMax);
        yield return new WaitForSeconds(randomDelay);

        if (gameObject.activeInHierarchy)
        {
            SpawnPearl();
        }
    }

    /// <summary>
    /// 设置魔法珠预制件（通过脚本传递）
    /// </summary>
    public void SetPearlPrefab(GameObject prefab)
    {
        CancelInvoke(nameof(SpawnPearl));
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 存储预制体引用（而不是实例）
        pearlPrefab = prefab;

        // 如果当前没有魔法珠实例，立即生成
        if (pearl == null && pearlPrefab != null)
        {
            SpawnPearl();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 恶魔之眼攻击脚本
/// 负责处理恶魔之眼的攻击逻辑
/// </summary>
public class EvilEyeAttack : MonoBehaviour
{
    public float fadeDuration = 1f; // 淡出时间
    public float laserWidth = 0.3f; // 连线宽度
    public Material laserMaterial; // 连线材质
    
    private SpriteRenderer spriteRenderer;
    private List<GameObject> activeLasers = new List<GameObject>(); // 活跃的连线列表
    private bool isFadeInComplete = false; // 淡入是否完成

    // Start is called before the first frame update
    void Start()
    {
        // 初始化逻辑
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // 攻击逻辑
        if (isFadeInComplete)
        {
            UpdateLasers();
            DealDamageToEnemies();
        }
    }
    
    void OnDisable()
    {
        // 清理逻辑
        ClearAllLasers();
    }
    
    /// <summary>
    /// 开始淡入
    /// </summary>
    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }
    
    /// <summary>
    /// 淡入协程
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        originalColor.a = 0f;
        spriteRenderer.color = originalColor;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            originalColor.a = alpha;
            spriteRenderer.color = originalColor;
            yield return null;
        }
        
        // 确保最终透明度为1
        originalColor.a = 1f;
        spriteRenderer.color = originalColor;
        
        // 淡入完成，开始攻击
        isFadeInComplete = true;
        CreateLasers();
    }
    
    /// <summary>
    /// 开始淡出
    /// </summary>
    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// 淡出协程
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - elapsedTime / fadeDuration);
            originalColor.a = alpha;
            spriteRenderer.color = originalColor;
            yield return null;
        }
        
        // 确保最终透明度为0并禁用
        originalColor.a = 0f;
        spriteRenderer.color = originalColor;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 创建连线
    /// </summary>
    private void CreateLasers()
    {
        // 清理之前的连线
        ClearAllLasers();
        
        // 从 GameManager 获取所有活跃敌人
        List<GameObject> enemies = GetActiveEnemies();
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                // 创建连线对象
                GameObject laserObj = new GameObject("EvilEyeLaser");
                laserObj.transform.parent = transform;
                laserObj.transform.position = transform.position;
                
                // 添加 LineRenderer 组件
                LineRenderer lineRenderer = laserObj.AddComponent<LineRenderer>();
                lineRenderer.startWidth = laserWidth;
                lineRenderer.endWidth = laserWidth;
                lineRenderer.material = laserMaterial ? laserMaterial : new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = new Color(1f, 0f, 0f, 0.5f);
                lineRenderer.endColor = new Color(1f, 0f, 0f, 0.5f);
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true; // 使用世界空间坐标
                
                // 设置连线的两个点
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, enemy.transform.position);
                
                // 设置排序层级
                lineRenderer.sortingLayerName = "Effect"; // 设置为效果层
                lineRenderer.sortingOrder = 9; // 设置排序顺序
                
                // 添加到活跃连线列表
                activeLasers.Add(laserObj);
            }
        }
    }
    
    /// <summary>
    /// 更新连线
    /// </summary>
    private void UpdateLasers()
    {
        // 每帧重新创建连线，确保连线到所有敌人
        CreateLasers();
    }
    
    /// <summary>
    /// 清理所有连线
    /// </summary>
    private void ClearAllLasers()
    {
        // 创建临时列表来存储需要删除的对象
        List<GameObject> lasersToDestroy = new List<GameObject>(activeLasers);
        
        // 遍历临时列表并销毁对象
        foreach (GameObject laserObj in lasersToDestroy)
        {
            if (laserObj != null)
            {
                Destroy(laserObj);
            }
        }
        
        // 清空活跃连线列表
        activeLasers.Clear();
    }
    
    /// <summary>
    /// 对敌人造成伤害
    /// </summary>
    private void DealDamageToEnemies()
    {
        // 从 GameManager 获取所有活跃敌人
        List<GameObject> enemies = GetActiveEnemies();
        
        // 创建临时列表来存储有效的敌人
        List<GameObject> validEnemies = new List<GameObject>();
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                validEnemies.Add(enemy);
            }
        }
        
        // 遍历有效敌人列表并造成伤害
        foreach (GameObject enemy in validEnemies)
        {
            if (enemy.TryGetComponent<Enemy>(out var enemyComponent))
            {
                // 计算伤害
                int damage = CalDamage();
                // 对敌人造成伤害
                enemyComponent.Damage(damage);
            }
        }
    }
    
    /// <summary>
    /// 从 GameManager 获取活跃敌人
    /// </summary>
    /// <returns>活跃敌人列表</returns>
    private List<GameObject> GetActiveEnemies()
    {
        if (Global_GameManager.Instance != null)
        {
            // 创建临时列表来存储有效的敌人
            List<GameObject> validEnemies = new List<GameObject>();
            foreach (GameObject enemy in Global_GameManager.Instance.EnemyList)
            {
                if (enemy != null)
                {
                    validEnemies.Add(enemy);
                }
            }
            return validEnemies;
        }
        return new List<GameObject>();
    }
    
    /// <summary>
    /// 计算伤害
    /// </summary>
    /// <returns>每帧伤害值</returns>
    private int CalDamage()
    {
        // 具体伤害计算逻辑留空
        // 这里返回一个默认值
        return 1;
    }
}

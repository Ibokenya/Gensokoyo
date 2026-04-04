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
    public GameObject blackHole; // 黑洞对象（恶魔之眼的子对象）
    public float blackHoleRotationSpeed = 180f; // 黑洞旋转速度
    public List<GameObject> shadowBulletPrefabs; // 暗影弹预制件列表（共3种）
    public float spawnInterval = 0.5f; // 暗影弹生成间隔
    
    // 空心矩形范围（通过4组坐标界定）
    public Vector2 innerRectBottomLeft; // 小矩形左下角坐标
    public Vector2 innerRectTopRight; // 小矩形右上角坐标
    public Vector2 outerRectBottomLeft; // 大矩形左下角坐标
    public Vector2 outerRectTopRight; // 大矩形右上角坐标
    
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer blackHoleRenderer;
    private List<GameObject> activeLasers = new List<GameObject>(); // 活跃的连线列表
    private bool isFadeInComplete = false; // 淡入是否完成
    private float spawnTimer = 0f; // 暗影弹生成计时器
    private int LaserInterval = 3;// 激光伤害间隔帧（每秒20帧出伤）

    void Awake()
    {
        // 使用Global_ObjectPool初始化3种暗影弹的对象池
        InitializeShadowBulletPools();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 初始化逻辑
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 初始化黑洞
        if (blackHole != null)
        {
            blackHoleRenderer = blackHole.GetComponent<SpriteRenderer>();
            if (blackHoleRenderer != null)
            {
                Color color = blackHoleRenderer.color;
                color.a = 0f;
                blackHoleRenderer.color = color;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 攻击逻辑
        if (isFadeInComplete)
        {
            UpdateBlackHole();
            
            // 检测Z键按下和抬起
            if (Input.GetKey(KeyCode.Z))
            {
                // Z键按下时，执行攻击逻辑
                UpdateLasers();
                LaserInterval-=1;
                if(LaserInterval == 0)
                {
                    DealDamageToEnemies();
                    LaserInterval = 3;
                }
                UpdateShadowBullets();
            }
            else if (Input.GetKeyUp(KeyCode.Z))
            {
                // Z键抬起时，清空所有连线
                ClearAllLasers();
            }
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
        
        // 激活黑洞
        if (blackHole != null)
        {
            if (blackHoleRenderer == null)
            {
                blackHoleRenderer = blackHole.GetComponent<SpriteRenderer>();
            }
            if (blackHoleRenderer != null)
            {
                Color color = blackHoleRenderer.color;
                color.a = 0f;
                blackHoleRenderer.color = color;
            }
        }
        
        float elapsedTime = 0f;
        Color originalColor = spriteRenderer.color;
        originalColor.a = 0f;
        spriteRenderer.color = originalColor;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            // 恶魔之眼淡入
            originalColor.a = alpha;
            spriteRenderer.color = originalColor;
            
            // 黑洞淡入：在同样的时间内由0透明度涨为0.5，0.5是上限
            if (blackHoleRenderer != null)
            {
                Color blackHoleColor = blackHoleRenderer.color;
                blackHoleColor.a = alpha * 0.5f;
                blackHoleRenderer.color = blackHoleColor;
            }
            
            yield return null;
        }
        
        // 确保最终透明度为1
        originalColor.a = 1f;
        spriteRenderer.color = originalColor;
        
        // 确保黑洞最终透明度为0.5
        if (blackHoleRenderer != null)
        {
            Color blackHoleColor = blackHoleRenderer.color;
            blackHoleColor.a = 0.5f;
            blackHoleRenderer.color = blackHoleColor;
        }
        
        // 淡入完成，开始攻击
        isFadeInComplete = true;
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
        Color blackHoleColor = Color.black;
        if (blackHoleRenderer != null)
        {
            blackHoleColor = blackHoleRenderer.color;
        }
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - elapsedTime / fadeDuration);
            
            // 恶魔之眼淡出
            originalColor.a = alpha;
            spriteRenderer.color = originalColor;
            
            // 黑洞淡出：从0.5淡出到0
            if (blackHoleRenderer != null)
            {
                blackHoleColor.a = alpha * 0.5f;
                blackHoleRenderer.color = blackHoleColor;
            }
            
            yield return null;
        }
        
        // 确保最终透明度为0并禁用
        originalColor.a = 0f;
        spriteRenderer.color = originalColor;
        gameObject.SetActive(false);
        
        // 黑洞是EvilEye的子物体，会随着EvilEye的禁用激活而禁用激活，无需手动禁用
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
                
                // 更新敌人红色度
                enemyComponent.UpdateRedIntensity();
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
    
    /// <summary>
    /// 更新黑洞
    /// </summary>
    private void UpdateBlackHole()
    {
        // 黑洞旋转
        if (blackHole != null && blackHole.activeInHierarchy)
        {
            blackHole.transform.Rotate(0f, 0f, blackHoleRotationSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 更新暗影弹
    /// </summary>
    private void UpdateShadowBullets()
    {
        // 检查黑洞是否存在且激活
        if (blackHole == null || !blackHole.activeInHierarchy)
        {
            return;
        }
        
        // 检查是否有暗影弹预制件
        if (shadowBulletPrefabs == null || shadowBulletPrefabs.Count == 0)
        {
            return;
        }
        
        // 计时生成暗影弹
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnShadowBullet();
        }
    }
    
    /// <summary>
    /// 生成暗影弹
    /// </summary>
    private void SpawnShadowBullet()
    {
        // 检查是否有暗影弹预制件
        if (shadowBulletPrefabs == null || shadowBulletPrefabs.Count == 0)
        {
            return;
        }
        
        // 随机选择一种暗影弹类型
        int randomIndex = Random.Range(0, shadowBulletPrefabs.Count);
        GameObject selectedPrefab = shadowBulletPrefabs[randomIndex];
        
        if (selectedPrefab == null)
        {
            return;
        }
        
        // 在空心矩形范围内随机生成位置
        Vector3 spawnPosition = GetRandomPositionInHollowRect();
        
        // 计算朝向黑洞的方向
        Vector2 direction = (blackHole.transform.position - spawnPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        
        // 从Global_ObjectPool获取暗影弹
        GameObject bullet = Global_ObjectPool.Instance.GetObject(selectedPrefab, spawnPosition, rotation);
        
        if (bullet == null)
        {
            return;
        }
        
        // 初始化暗影弹
        DarkFO darkFO = bullet.GetComponent<DarkFO>();
        if (darkFO != null)
        {
            darkFO.Initialize(blackHole.transform, outerRectBottomLeft, outerRectTopRight);
        }
    }
    
    /// <summary>
    /// 获取空心矩形范围内的随机位置
    /// </summary>
    /// <returns>随机位置</returns>
    private Vector3 GetRandomPositionInHollowRect()
    {
        float x, y;
        
        // 随机决定是在左右边还是上下边
        bool isHorizontal = Random.value > 0.5f;
        
        if (isHorizontal)
        {
            // 左右边
            // 随机选择左边或右边
            bool isLeft = Random.value > 0.5f;
            x = isLeft ? outerRectBottomLeft.x : outerRectTopRight.x;
            
            // 上下范围内随机
            y = Random.Range(outerRectBottomLeft.y, outerRectTopRight.y);
        }
        else
        {
            // 上下边
            // 随机选择上边或下边
            bool isTop = Random.value > 0.5f;
            y = isTop ? outerRectTopRight.y : outerRectBottomLeft.y;
            
            // 左右范围内随机
            x = Random.Range(outerRectBottomLeft.x, outerRectTopRight.x);
        }
        
        // 转换为世界坐标
        return transform.position + new Vector3(x, y, 0f);
    }
    
    /// <summary>
    /// 初始化暗影弹对象池
    /// </summary>
    private void InitializeShadowBulletPools()
    {
        // 检查是否有暗影弹预制件
        if (shadowBulletPrefabs == null || shadowBulletPrefabs.Count == 0)
        {
            return;
        }
        
        // 使用Global_ObjectPool为每种暗影弹创建对象池（每种15个）
        for (int i = 0; i < shadowBulletPrefabs.Count; i++)
        {
            if (shadowBulletPrefabs[i] != null)
            {
                Global_ObjectPool.Instance.InitPool(shadowBulletPrefabs[i], 15);
            }
        }
    }
}

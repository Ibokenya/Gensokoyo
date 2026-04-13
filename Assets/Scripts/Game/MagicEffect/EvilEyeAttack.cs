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
    
    [Header("处决效果")]
    public GameObject giantHandPrefab; // 巨手预制件
    public Sprite openHandSprite; // 张开的手精灵
    public Sprite closedHandSprite; // 握住的手精灵
    public float handSpawnYOffset = 6f; // 巨手生成位置的Y轴偏移
    public float handSpawnXOffset = -0.3f; // 巨手生成位置的X轴偏移
    public float handTargetYOffset = 1.6f; // 巨手目标位置的Y轴偏移
    public float handFadeInDuration = 1f; // 巨手淡入时间
    public float handFadeOutDuration = 0.5f; // 巨手淡出时间
    public float handMinY = 0.52f; // 巨手最低Y坐标
    
    // 空心矩形范围（通过4组坐标界定）
    public Vector2 innerRectBottomLeft; // 小矩形左下角坐标
    public Vector2 innerRectTopRight; // 小矩形右上角坐标
    public Vector2 outerRectBottomLeft; // 大矩形左下角坐标
    public Vector2 outerRectTopRight; // 大矩形右上角坐标

    [Header("处决音效")]
    public AudioClip deathClip; // 处决音效
    
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer blackHoleRenderer;
    private List<GameObject> activeLasers = new List<GameObject>(); // 活跃的连线列表
    public bool isFadeInComplete = false; // 淡入是否完成
    private float spawnTimer = 0f; // 暗影弹生成计时器
    private int LaserInterval = 10;// 激光伤害间隔帧（每6帧出伤）
    private Coroutine fadeCoroutine;
    
    // 巨手对象池
    private Queue<GameObject> giantHandPool = new Queue<GameObject>();
    private const int handPoolSize = 4; // 巨手对象池大小

    void Awake()
    {
        // 使用Global_ObjectPool初始化3种暗影弹的对象池
        InitializeShadowBulletPools();
        
        // 初始化巨手对象池
        InitializeGiantHandPool();
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

    void OnDisable()
    {
        // 停止正在运行的协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // 清理活跃的激光
        ClearAllLasers();
        
        // 重置状态
        isFadeInComplete = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 攻击逻辑
        if (isFadeInComplete && Input.GetKey(KeyCode.LeftShift))
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
                    LaserInterval = 10;
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
    
    /// <summary>
    /// 开始淡入
    /// </summary>
    public void StartFadeIn()
    {
        if (gameObject.activeInHierarchy)
        {
            fadeCoroutine = StartCoroutine(FadeIn());
        }
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
        if (gameObject.activeInHierarchy)
        {
            // 停止正在运行的协程（包括淡入协程）
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            
            fadeCoroutine = StartCoroutine(FadeOut());
        }
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
        float currentAlpha = originalColor.a; // 获取当前透明度
        float actualFadeDuration = fadeDuration * currentAlpha; // 计算实际淡出时间
        Color blackHoleColor = Color.black;
        if (blackHoleRenderer != null)
        {
            blackHoleColor = blackHoleRenderer.color;
        }
        
        while (elapsedTime < actualFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / actualFadeDuration);
            float alpha = Mathf.Clamp01(1 - t);
            
            // 恶魔之眼淡出
            originalColor.a = alpha;
            spriteRenderer.color = originalColor;
            
            // 黑洞淡出：从当前透明度淡出到0
            if (blackHoleRenderer != null)
            {
                blackHoleColor.a = alpha * 0.5f;
                blackHoleRenderer.color = blackHoleColor;
            }
            
            yield return null;
        }
        
        // 确保最终透明度为0
        originalColor.a = 0f;
        spriteRenderer.color = originalColor;
        
        // 确保黑洞最终透明度为0
        if (blackHoleRenderer != null)
        {
            blackHoleColor.a = 0f;
            blackHoleRenderer.color = blackHoleColor;
        }
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
    public void ClearAllLasers()
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
    /// <returns>每伤害帧伤害值</returns>
    private int CalDamage()
    {
        return 2+(Global_GameManager.Instance.Power/100);
    }
    
    /// <summary>
    /// 更新黑洞
    /// </summary>
    private void UpdateBlackHole()
    {
        // 黑洞旋转
        if (blackHole != null && spriteRenderer != null && spriteRenderer.color.a >= 0.99f)
        {
            blackHole.transform.Rotate(0f, 0f, blackHoleRotationSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 更新暗影弹
    /// </summary>
    private void UpdateShadowBullets()
    {
        // 检查黑洞是否存在
        if (blackHole == null)
        {
            return;
        }
        
        // 检查恶魔之眼是否完全显示（透明度为1）
        if (spriteRenderer != null && spriteRenderer.color.a < 0.99f)
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
    
    /// <summary>
    /// 初始化巨手对象池
    /// </summary>
    private void InitializeGiantHandPool()
    {
        if (giantHandPrefab == null)
        {
            return;
        }
        
        for (int i = 0; i < handPoolSize; i++)
        {
            GameObject hand = Instantiate(giantHandPrefab);
            hand.SetActive(false);
            giantHandPool.Enqueue(hand);
        }
    }
    
    /// <summary>
    /// 从对象池获取巨手
    /// </summary>
    /// <returns>巨手对象</returns>
    private GameObject GetGiantHandFromPool()
    {
        if (giantHandPool.Count > 0)
        {
            return giantHandPool.Dequeue();
        }
        else if (giantHandPrefab != null)
        {
            // 如果对象池为空，创建新的巨手
            GameObject hand = Instantiate(giantHandPrefab);
            hand.SetActive(false);
            return hand;
        }
        return null;
    }
    
    /// <summary>
    /// 回收巨手到对象池
    /// </summary>
    /// <param name="hand">要回收的巨手</param>
    private void RecycleGiantHand(GameObject hand)
    {
        if (hand != null)
        {
            hand.SetActive(false);
            giantHandPool.Enqueue(hand);
        }
    }
    
    /// <summary>
    /// 执行处决效果
    /// </summary>
    /// <param name="enemyPosition">敌人位置</param>
    public void ExecuteEnemy(Vector3 enemyPosition)
    {
        // 从对象池获取巨手
        GameObject hand = GetGiantHandFromPool();
        if (hand == null)
        {
            return;
        }
        
        // 计算巨手的生成位置
        Vector3 spawnPosition = enemyPosition;
        spawnPosition.y += handSpawnYOffset;
        
        // 计算巨手的目标位置
        Vector3 targetPosition = enemyPosition;
        targetPosition.x += handSpawnXOffset;
        targetPosition.y += handTargetYOffset;
        // 确保巨手的Y坐标不低于最低值
        targetPosition.y = Mathf.Max(targetPosition.y, handMinY);
        
        // 初始化巨手
        hand.transform.position = spawnPosition;
        hand.transform.localScale = new Vector3(0, 0, 1);
        
        SpriteRenderer handRenderer = hand.GetComponent<SpriteRenderer>();
        if (handRenderer != null)
        {
            Color color = handRenderer.color;
            color.a = 0f;
            handRenderer.color = color;
        }
        
        hand.SetActive(true);
        
        // 开始处决动画
        StartCoroutine(ExecuteAnimation(hand, targetPosition));
    }
    
    /// <summary>
    /// 处决动画协程
    /// </summary>
    /// <param name="hand">巨手对象</param>
    /// <param name="targetPosition">目标位置</param>
    private IEnumerator ExecuteAnimation(GameObject hand, Vector3 targetPosition)
    {
        SpriteRenderer handRenderer = hand.GetComponent<SpriteRenderer>();
        if (handRenderer == null)
        {
            RecycleGiantHand(hand);
            yield break;
        }
        // 播放处决音效
        if (Global_AudioManager.Instance != null && deathClip != null)
        {
            Global_AudioManager.Instance.PlaySFX(deathClip,false);
        }
        // 设置初始精灵为张开的手
        if (openHandSprite != null)
        {
            handRenderer.sprite = openHandSprite;
        }
        
        float elapsedTime = 0f;
        Vector3 startPosition = hand.transform.position;
        Vector3 startScale = hand.transform.localScale;
        Color startColor = handRenderer.color;
        
        // 淡入和移动到目标位置
        while (elapsedTime < handFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / handFadeInDuration);
            
            // 移动
            hand.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            // 缩放
            hand.transform.localScale = Vector3.Lerp(startScale, new Vector3(2.25f, 2.25f, 1), t);
            
            // 淡入
            Color color = startColor;
            color.a = t/2;
            handRenderer.color = color;
            
            yield return null;
        }
        
        // 切换到第二帧（握住的手）
        if (closedHandSprite != null)
        {
            handRenderer.sprite = closedHandSprite;
        }
        
        // 等待一小段时间
        yield return new WaitForSeconds(0.1f);
        
        // 淡出
        elapsedTime = 0f;
        startColor = handRenderer.color;
        while (elapsedTime < handFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / handFadeOutDuration);
            
            // 淡出
            Color color = startColor;
            color.a = 1f - t;
            handRenderer.color = color;
            
            yield return null;
        }
        
        // 回收巨手
        RecycleGiantHand(hand);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShootSystem : MonoBehaviour
{
    public GameObject player;
    public GameObject boss;
    public GameObject IceTerrain;// 冰刺地形
    public SpriteRenderer IceTerrainSprite;
    public AudioClip FrozeSound; // 冰冻音效
    private const float animationSpeed = 0.1f; // 动画速度
    private int currentSpriteIndex = 0; // 当前动画帧索引
    private Vector2 Center = new Vector2(-3, 0);
#region none1参数
    public GameObject IcePoint;
    public List<GameObject> IcePoints;
    public List<Sprite> icePointSprites; // icepoint 帧动画素材
#endregion
#region card1参数
    // card1相关参数由card1脚本提供
    List<GameObject> stones = new ();
    List<float> initialAngles = new (); // 保存每个陨石的初始角度
    List<float> speedOffsets = new (); // 保存每个陨石的旋转速度偏移
    List<Vector2> stonePositions = new (); // 保存已生成的陨石位置
    List<float> individualAngles = new (); // 保存每个陨石的独立角度
    Vector2 targetPosition;
    private int randomIcePickBulletCount = 5; // 随机射击的子弹数量
#endregion  
#region card2参数
    // 雪花生成点列表（24个）
    private List<Vector2> FlakePos = new List<Vector2>
    {
        // 上边框 (y=6)
        new Vector2(-12, 6),
        new Vector2(-9, 6),
        new Vector2(-6, 6),
        new Vector2(-3, 6),
        new Vector2(0, 6),
        new Vector2(3, 6),
        new Vector2(6, 6),
        // 下边框 (y=-6)
        new Vector2(-12, -6),
        new Vector2(-9, -6),
        new Vector2(-6, -6),
        new Vector2(-3, -6),
        new Vector2(0, -6),
        new Vector2(3, -6),
        new Vector2(6, -6),
        // 左边框 (x=-12, 排除上下边框重复的点)
        new Vector2(-12,4),
        new Vector2(-12, 2),
        new Vector2(-12, 0),
        new Vector2(-12, -2),
        new Vector2(-12, -4),
        // 右边框 (x=6, 排除上下边框重复的点)
        new Vector2(6, 4),
        new Vector2(6, 2),
        new Vector2(6, 0),
        new Vector2(6, -2),
        new Vector2(6, -4)
    };
    // 存储活跃的雪花子弹
    private List<GameObject> activeSnowFlakes = new List<GameObject>();

    // 冰云生成参数
    private Vector2 cloudSpawnMin; // 生成范围左下角
    private Vector2 cloudSpawnMax; // 生成范围右上角
    private int cloudCount; // 冰云数量
    private GameObject cloudPrefab; // 冰云预制件
#endregion


#region 定位扇形射击（一非）
    public void Pos_FanShaped_Shoot(GameObject bullet, float shoot_interval)
    {
        StartCoroutine(FanShapedShootCoroutine(bullet, shoot_interval));
    }
    
    private IEnumerator FanShapedShootCoroutine(GameObject bullet, float shoot_interval)
    {
        while (true)
        {
            if (player != null && boss != null && bullet != null)
            {
                // 计算玩家相对于 boss 的方向向量
                Vector3 direction = player.transform.position - boss.transform.position;
                direction.z = 0; // 只考虑 2D 平面
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                
                // 确保角度在 0-360 度范围内
                if (angle < 0)
                {
                    angle += 360f;
                }
                
                // 计算射击范围（120度）
                float startAngle = angle - 60f;
                float endAngle = angle + 60f;
                
                // 随机子弹数量（6-10枚）
                int bulletCount = Random.Range(6, 11);
                
                // 计算每枚子弹的角度间隔
                float angleStep = (endAngle - startAngle) / (bulletCount - 1);
                
                // 发射子弹
                for (int i = 0; i < bulletCount; i++)
                {
                    float currentAngle = startAngle + i * angleStep;
                    Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                    
                    // 使用对象池获取子弹
                    GameObject bulletInstance = Global_ObjectPool.Instance.GetObject(bullet, boss.transform.position, rotation);
                }
            }
            
            // 等待射击间隔
            yield return new WaitForSeconds(shoot_interval);
        }
    }
#endregion
#region 发射冰点并爆炸（一非）
    public void IcePointAttack()
    {
        IcePoint.SetActive(true);
        
        // 确保 IcePoints 列表不为空
        if (IcePoints == null)
        {
            IcePoints = new List<GameObject>();
        }
        
        // 启动帧动画
        StartCoroutine(IcePointAnimationCoroutine());
    }
    
    private IEnumerator IcePointAnimationCoroutine()
    {
        while (true)
        {
            if (icePointSprites.Count > 0)
            {
                // 切换所有 icepoint 的 sprite
                foreach (var icePoint in IcePoints)
                {
                    if (icePoint != null)
                    {
                        SpriteRenderer spriteRenderer = icePoint.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.sprite = icePointSprites[currentSpriteIndex];
                        }
                    }
                }
                
                // 更新动画帧索引
                currentSpriteIndex = (currentSpriteIndex + 1) % icePointSprites.Count;
            }
            
            yield return new WaitForSeconds(animationSpeed);
        }
    }
    
    public void IcePoint_Shoot(GameObject bullet1, GameObject bullet2, float interval1, float interval2, float rotationSpeed, float bullet1Speed, float bullet2Speed)
    {
        // 首先发射预制件1的弹幕（360° 12个），不使用速度偏移，不是冰珠
        for (int i = 0; i < IcePoints.Count; i++)
        {
            var icePoint = IcePoints[i];
            if (icePoint != null)
            {
                FireBulletRing(icePoint, bullet1, 12, false, bullet1Speed, false, rotationSpeed, i);
            }
        }
        
        // 启动射击逻辑
        StartCoroutine(IcePointShootCoroutine(bullet2, interval1, interval2, bullet2Speed, rotationSpeed));
    }
    
    // 冰点发射一圈环状子弹
    private void FireBulletRing(GameObject icePoint, GameObject bullet, int count, bool useSpeedOffset = true, float speed = -1f, bool isIcePearl = false, float rotationSpeed = 0f, int icePointIndex = 0)
    {
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            // 使用对象池获取子弹
            GameObject bulletInstance = Global_ObjectPool.Instance.GetObject(bullet, icePoint.transform.position, rotation);
            
            if (bulletInstance != null)
            {
                if (isIcePearl)
                {
                    // 设置 IcePearl 组件参数
                    IcePearl icePearl = bulletInstance.GetComponent<IcePearl>();
                    if (icePearl != null)
                    {
                        icePearl.icePoint = icePoint;
                        // 根据冰点索引决定旋转方向
                        if(icePointIndex < 3)
                        {
                            icePearl.rotationSpeed = rotationSpeed;
                        }
                        else if(icePointIndex >= 3)
                        {
                            icePearl.rotationSpeed = -rotationSpeed;
                        }
                        icePearl.moveSpeed = speed;
                    }
                }
                else
                {
                    // 设置子弹是否使用速度偏移
                    NormalIce normalIce = bulletInstance.GetComponent<NormalIce>();
                    if (normalIce != null)
                    {
                        normalIce.useSpeedOffset = useSpeedOffset;
                        // 如果指定了速度，则设置子弹速度
                        if (speed > 0)
                        {
                            normalIce.SetSpeed(speed);
                        }
                    }
                }
            }
        }
    }
    
    private IEnumerator IcePointShootCoroutine(GameObject bullet2, float interval1, float interval2, float bullet2Speed = -1f, float rotationSpeed = 60f)
    {
        // 等待 interval1 后开始发射预制件2
        yield return new WaitForSeconds(interval1);
        
        while (true)
        {
            // 发射预制件2的弹幕（360° 12个），使用速度偏移，是冰珠
            for (int i = 0; i < IcePoints.Count; i++)
            {
                var icePoint = IcePoints[i];
                if (icePoint != null)
                {
                    FireBulletRing(icePoint, bullet2, 12, true, bullet2Speed, true, rotationSpeed, i);
                }
            }
            
            // 等待 interval2 后再次发射
            yield return new WaitForSeconds(interval2);
        }
    }
    


    public void CancelIcePoint()
    {
        IcePoint.SetActive(false);
    }
#endregion
#region 陨石冰冻旋转攻击（一符）
    /// <summary>
    /// 陨石冰冻旋转攻击
    /// </summary>
    /// <param name="stoneBullet">陨石子弹预制体</param>
    /// <param name="frozenIceBullet">冰冻子弹预制体</param>
    /// <param name="normalIceBullet">普通冰子弹预制体（用于冰块破裂）</param>
    /// <param name="stoneCount">陨石数量</param>
    /// <param name="rotationSpeed">旋转速度</param>
    public void StoneFrozenAttack(GameObject stoneBullet, GameObject frozenIceBullet, GameObject normalIceBullet, int stoneCount, float rotationSpeed)
    {
        StartCoroutine(StoneFrozenAttackCoroutine(stoneBullet, frozenIceBullet, normalIceBullet, stoneCount, rotationSpeed));
    }
    
    private IEnumerator StoneFrozenAttackCoroutine(GameObject stoneBullet, GameObject frozenIceBullet, GameObject normalIceBullet, int stoneCount, float rotationSpeed)
    {


        // 生成随机目标点并发射陨石
        for (int i = 0; i < stoneCount; i++)
        {
            bool validPosition = false;
            int attempts = 0;
            
            // 尝试生成有效的陨石位置
            while (!validPosition && attempts < 50)
            {
                attempts++;
                // 生成随机角度
                float angle = Random.Range(0, Mathf.PI * 2);
                // 使用二次方分布，使陨石更可能出现在离圆心较远的地方
                float randomValue = Random.value; // 0-1之间的随机值
                // 映射到1-6的半径范围，使用二次方分布
                float radius = 1f + (5f * randomValue * randomValue);
                targetPosition = new Vector2(
                    Center.x + Mathf.Cos(angle) * radius,
                    Center.y + Mathf.Sin(angle) * radius
                );
                
                // 检查与已有陨石的距离
                validPosition = true;
                foreach (var existingPos in stonePositions)
                {
                    if (Vector2.Distance(targetPosition, existingPos) < 1f)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }
            
            // 如果无法找到有效位置，使用默认位置
            if (!validPosition)
            {
                float angle = Random.Range(0, Mathf.PI * 2);
                float radius = 3f + Random.Range(0, 3f);
                targetPosition = new Vector2(
                    Center.x + Mathf.Cos(angle) * radius,
                    Center.y + Mathf.Sin(angle) * radius
                );
            }
            
            // 保存位置
            stonePositions.Add(targetPosition);
            
            // 创建陨石子弹
            GameObject stone = Global_ObjectPool.Instance.GetObject(stoneBullet, new Vector3(targetPosition.x, 7f, 0), Quaternion.identity);
            if (stone != null)
            {
                Stone stoneScript = stone.GetComponent<Stone>();
                if (stoneScript != null)
                {
                    stoneScript.Initialize(targetPosition.x, targetPosition.y);
                    stones.Add(stone);
                    // 保存初始角度（弧度）
                    initialAngles.Add(Mathf.Atan2(targetPosition.y - Center.y, targetPosition.x - Center.x));
                    // 为每个陨石生成旋转速度偏移（-5到5之间）
                    speedOffsets.Add(Random.Range(-5f, 5f));
                    // 为每个陨石初始化独立角度
                    individualAngles.Add(0f);
                }
            }
            
            // 稍微延迟生成下一个陨石
            yield return new WaitForSeconds(0.1f);
        }
        
        // 等待所有陨石到达目标位置
        bool allStonesReached = false;
        while (!allStonesReached)
        {
            allStonesReached = true;
            foreach (var stone in stones)
            {
                if (stone != null && stone.activeInHierarchy)
                {
                    Stone stoneScript = stone.GetComponent<Stone>();
                    if (stoneScript != null && !stoneScript.IsReachedTarget)
                    {
                        allStonesReached = false;
                    }
                }
            }
            yield return null;
        }
        
        // 为每个陨石创建冰冻效果并开始旋转
        foreach (var stone in stones)
        {
            if (stone != null && stone.activeInHierarchy)
            {
                // 创建冰冻子弹
                GameObject frozenIce = Global_ObjectPool.Instance.GetObject(frozenIceBullet, stone.transform.position, Quaternion.identity);
                if (frozenIce != null)
                {
                    FrozenIce frozenIceScript = frozenIce.GetComponent<FrozenIce>();
                    if (frozenIceScript != null)
                    {
                        frozenIce.transform.parent = stone.transform;
                        frozenIceScript.normalIcePrefab = normalIceBullet;
                        frozenIceScript.bossShootSystem = this;
                    }
                }
            }
        }
        
        // 开始旋转所有陨石
        while (true)
        {
            for (int i = 0; i < stones.Count; i++)
            {
                var stone = stones[i];
                if (stone != null && stone.activeInHierarchy && i < initialAngles.Count && i < speedOffsets.Count && i < individualAngles.Count)
                {
                    // 计算每个陨石的旋转速度（基础速度加上偏移）
                    float stoneRotationSpeed = rotationSpeed + speedOffsets[i];
                    // 更新每个陨石的独立角度
                    individualAngles[i] += stoneRotationSpeed * Time.deltaTime;
                    // 计算每个陨石的旋转位置（使用初始角度加上独立旋转角度）
                    float angle = individualAngles[i] * Mathf.Deg2Rad + initialAngles[i];
                    float radius = Vector2.Distance(new Vector2(stone.transform.position.x, stone.transform.position.y), Center);
                    float x = Center.x + Mathf.Cos(angle) * radius;
                    float y = Center.y + Mathf.Sin(angle) * radius;
                    
                    stone.transform.position = new Vector3(x, y, 0);
                }
            }
            
            yield return null;
        }
    }
#endregion
#region 随机射击（一符）
    /// <summary>
    /// 随机射击方法（一符专用）
    /// </summary>
    /// <param name="bullet">子弹预制件</param>
    /// <param name="bulletSpeed">射击速度</param>
    /// <param name="shootInterval">射击间隔</param>
    /// <param name="bulletCount">每轮射击子弹数</param>
    public void randomIcePick(GameObject bullet, float bulletSpeed, float shootInterval, int bulletCount = 5)
    {
        randomIcePickBulletCount = bulletCount;
        StartCoroutine(RandomIcePickCoroutine(bullet, bulletSpeed, shootInterval));
    }
    
    private IEnumerator RandomIcePickCoroutine(GameObject bullet, float bulletSpeed, float shootInterval)
    {
        while (true)
        {
            // 发射一波随机角度的子弹
            for (int i = 0; i < randomIcePickBulletCount; i++)
            {
                // 随机生成0-360度的角度
                float randomAngle = Random.Range(0f, 360f);
                Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
                
                // 使用对象池获取子弹
                GameObject bulletInstance = Global_ObjectPool.Instance.GetObject(bullet, boss.transform.position, rotation);
                
                if (bulletInstance != null)
                {
                    // 检查是否是NormalIce
                    NormalIce normalIce = bulletInstance.GetComponent<NormalIce>();
                    if (normalIce != null)
                    {
                        normalIce.SetSpeed(bulletSpeed);
                    }
                }
            }
            
            // 等待射击间隔
            yield return new WaitForSeconds(shootInterval);
        }
    }
    
    #endregion
#region 冰块破裂攻击（一符）
    /// <summary>
    /// 冰块破裂攻击——以指定位置为中心发射一圈NormalIce子弹
    /// </summary>
    /// <param name="position">冰块摧毁位置</param>
    /// <param name="normalIcePrefab">NormalIce子弹预制件</param>
    public void FrozenIceExplode(Vector3 position, GameObject normalIcePrefab)
    {
        // 发射8枚均匀分布的NormalIce子弹
        int bulletCount = 8;
        float angleStep = 360f / bulletCount;
        
        if (normalIcePrefab != null)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep;
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                
                // 使用对象池获取子弹
                GameObject bulletInstance = Global_ObjectPool.Instance.GetObject(normalIcePrefab, position, rotation);
                
                if (bulletInstance != null)
                {
                    NormalIce normalIce = bulletInstance.GetComponent<NormalIce>();
                    if (normalIce != null)
                    {
                        normalIce.SetSpeed(3f);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 当FrozenIce被摧毁时调用，增加陨石旋转速度和随机子弹数量
    /// </summary>
    public void OnFrozenIceDestroyed()
    {
        // 增加随机射击的子弹数量
        randomIcePickBulletCount++;
        
        // 增加所有陨石的旋转速度
        for (int i = 0; i < speedOffsets.Count; i++)
        {
            speedOffsets[i] += 2f;
        }
    }
#endregion
#region 随机射击（二非）
    private Coroutine none2ShootingCoroutine;
    
    /// <summary>
    /// 二非随机射击方法
    /// </summary>
    /// <param name="bullet">子弹预制件（miniIceBall）</param>
    /// <param name="bulletSpeed">射击速度</param>
    /// <param name="shootInterval">射击间隔</param>
    /// <param name="bulletCount">每轮射击子弹数</param>
    public void none2RandomShoot(GameObject bullet, float bulletSpeed, float shootInterval, int bulletCount = 5)
    {
        // 停止之前的射击协程
        if (none2ShootingCoroutine != null)
        {
            StopCoroutine(none2ShootingCoroutine);
        }
        
        // 启动新的射击协程
        none2ShootingCoroutine = StartCoroutine(None2RandomShootCoroutine(bullet, bulletSpeed, shootInterval, bulletCount));
    }
    
    private IEnumerator None2RandomShootCoroutine(GameObject bullet, float bulletSpeed, float shootInterval, int bulletCount)
    {
        while (true)
        {
            // 发射一波子弹
            List<GameObject> waveBullets = new List<GameObject>();
            
            // 获取当前boss位置作为目标位置
            Vector3 currentBossPosition = boss.transform.position;
            
            for (int i = 0; i < bulletCount; i++)
            {
                // 随机生成0-360度的角度
                float randomAngle = Random.Range(0f, 360f);
                Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
                
                // 使用对象池获取子弹
                GameObject bulletInstance = Global_ObjectPool.Instance.GetObject(bullet, currentBossPosition, rotation);
                
                if (bulletInstance != null)
                {
                    waveBullets.Add(bulletInstance);
                    
                    // 设置miniIceBall参数
                    miniIceBall miniIce = bulletInstance.GetComponent<miniIceBall>();
                    if (miniIce != null)
                    {
                        miniIce.moveSpeed = bulletSpeed;
                        // 不覆盖TurnInterval，使用预制体中设置的值
                        miniIce.TargetPosition = currentBossPosition;
                    }
                }
            }
            
            // 启动这一波子弹的融合处理
            if (waveBullets.Count > 0)
            {
                StartBulletWave(waveBullets, currentBossPosition);
            }
            
            // 等待射击间隔
            yield return new WaitForSeconds(shootInterval);
        }
    }
#endregion
#region 子弹融合攻击（二非）
    /// <summary>
    /// 子弹波次数据类
    /// </summary>
    private class BulletWave
    {
        public int waveId;
        public List<GameObject> bullets = new List<GameObject>();
        public GameObject firstBullet = null;
        public int totalBullets;
        public int arrivedBullets = 0;
        public Vector3 targetPosition;
        public bool hasLaunched = false;
    }
    
    private List<BulletWave> activeWaves = new List<BulletWave>();
    private int currentWaveId = 0;
    
    /// <summary>
    /// 开始单波子弹融合攻击
    /// </summary>
    /// <param name="bullets">子弹列表</param>
    /// <param name="targetPosition">目标位置</param>
    public void StartBulletWave(List<GameObject> bullets, Vector3 targetPosition)
    {
        if (bullets == null || bullets.Count == 0)
        {
            return;
        }
        
        // 创建新的波次
        BulletWave newWave = new BulletWave();
        newWave.waveId = currentWaveId++;
        newWave.bullets = new List<GameObject>(bullets);
        newWave.totalBullets = bullets.Count;
        newWave.targetPosition = targetPosition;
        activeWaves.Add(newWave);
        
        // 启动该波次的融合协程
        StartCoroutine(ProcessBulletWaveCoroutine(newWave));
    }
    
    private IEnumerator ProcessBulletWaveCoroutine(BulletWave wave)
    {
        // 存储波次中所有子弹的属性
        int totalBulletsCount = wave.totalBullets;
        int totalHP = 0;
        float originalSpeed = 0;
        // 记录原始速度（取第一个子弹的速度）
        if (wave.bullets.Count > 0)
        {
            miniIceBall firstMiniIce = wave.bullets[0].GetComponent<miniIceBall>();
            if (firstMiniIce != null)
            {
                originalSpeed = firstMiniIce.moveSpeed;Debug.Log($"[BossShootSystem] 波次 {wave.waveId} 记录到原始速度: {originalSpeed}");
            }
        }
        
        // 记录所有子弹的HP
        foreach (var bullet in wave.bullets)
        {
            if (bullet != null && bullet.activeInHierarchy)
            {
                miniIceBall miniIce = bullet.GetComponent<miniIceBall>();
                if (miniIce != null)
                {
                    totalHP += miniIce.hp;
                }
            }
        }
        
        // 等待3秒，让子弹完成飞行和折返
        yield return new WaitForSeconds(3f);
        
        // 回收所有剩余的子弹
        foreach (var bullet in wave.bullets)
        {
            if (bullet != null && bullet.activeInHierarchy)
            {
                miniIceBall miniIce = bullet.GetComponent<miniIceBall>();
                if (miniIce != null)
                {
                    miniIce.Recycle();
                }
            }
        }
        
        // 创建一个新的大子弹
        if (totalBulletsCount > 0 && player != null && wave.bullets.Count > 0)
        { 
            // 计算大子弹的属性
            float scaleIncrease = totalBulletsCount * 0.2f;
            int finalHP = totalHP;
            float finalSpeed = Mathf.Max(1, originalSpeed - totalBulletsCount * 0.2f);  
            // 从对象池获取一个子弹作为大子弹
            GameObject bigBullet = Global_ObjectPool.Instance.GetObject(wave.bullets[0].gameObject, wave.targetPosition, Quaternion.identity);
            if (bigBullet != null)
            {
                miniIceBall bigIce = bigBullet.GetComponent<miniIceBall>();
                if (bigIce != null)
                {
                    // 设置大子弹属性
                    bigIce.isMini = false; // 非mini态，无折返
                    bigIce.moveSpeed = finalSpeed;
                    bigIce.hp = finalHP;
                    bigIce.transform.localScale = new Vector3(1 + scaleIncrease, 1 + scaleIncrease, 1);
                    
                    // 瞄准玩家发射
                    Vector2 direction = (player.transform.position - bigBullet.transform.position).normalized;
                    bigIce.FireInDirection(direction);
                }
            }
        }
        
        wave.hasLaunched = true; 
        // 从活动波次列表中移除
        activeWaves.Remove(wave);
    }
    
    /// <summary>
    /// 停止所有子弹波次
    /// </summary>
    public void StopAllBulletWaves()
    {
        foreach (var wave in activeWaves)
        {
            foreach (var bullet in wave.bullets)
            {
                if (bullet != null && bullet.activeInHierarchy)
                {
                    miniIceBall miniIce = bullet.GetComponent<miniIceBall>();
                    if (miniIce != null)
                    {
                        miniIce.Recycle();
                    }
                }
            }
        }
        activeWaves.Clear();
    }
#endregion
#region 雪花攻击（二符）
    /// <summary>
    /// 雪花攻击方法
    /// </summary>
    /// <param name="flakePrefab">雪花子弹预制体</param>
    /// <param name="totalCount">生成的雪花总数</param>
    public void SnowFlakeAttack(GameObject flakePrefab, int totalCount)
    {
        StartCoroutine(SnowFlakeAttackCoroutine(flakePrefab, totalCount));
    }
    
    private IEnumerator SnowFlakeAttackCoroutine(GameObject flakePrefab, int totalCount)
    {
        // 生成指定数量的雪花子弹
        for (int i = 0; i < totalCount; i++)
        {
            // 随机选择一个生成点
            int randomPosIndex = Random.Range(0, FlakePos.Count);
            Vector2 spawnPosition = FlakePos[randomPosIndex];
            
            // 使用对象池获取雪花子弹
            GameObject flakeInstance = Global_ObjectPool.Instance.GetObject(flakePrefab, spawnPosition, Quaternion.identity);
            
            if (flakeInstance != null)
            {
                // 添加到活跃雪花列表
                activeSnowFlakes.Add(flakeInstance);
                
                SnowFlake snowFlake = flakeInstance.GetComponent<SnowFlake>();
                if (snowFlake != null)
                {
                    // 根据生成点位置确定移动方向
                    Vector2 direction = GetSnowFlakeDirection(spawnPosition);
                    snowFlake.SetDirection(direction);
                }
            }
            
            // 短暂延迟，避免所有雪花同时生成
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// 根据生成点位置确定雪花移动方向
    /// </summary>
    /// <param name="position">生成点位置</param>
    /// <returns>移动方向向量</returns>
    private Vector2 GetSnowFlakeDirection(Vector2 position)
    {
        float x = position.x;
        float y = position.y;
        
        // 上边框 (y=6)
        if (y == 6)
        {
            if (x <= -9)
            {
                return new Vector2(1, -1).normalized; // 右下
            }
            else if (x >= 3)
            {
                return new Vector2(-1, -1).normalized; // 左下
            }
            else
            {
                return new Vector2(0, -1).normalized; // 向下
            }
        }
        // 下边框 (y=-6)
        else if (y == -6)
        {
            if (x <= -9)
            {
                return new Vector2(1, 1).normalized; // 右sahng
            }
            else if (x >= 3)
            {
                return new Vector2(-1, 1).normalized; // 左下
            }
            else
            {
                return new Vector2(0, 1).normalized; // 向上
            }
        }
        // 左边框 (x=-12)
        else if (x == -12)
        {
            if (y >= 4)
            {
                return new Vector2(1, -1).normalized; // 右下
            }
            else if (y <= -4)
            {
                return new Vector2(1, 1).normalized; // 右上
            }
            else
            {
                return new Vector2(1, 0).normalized; // 向右
            }
        }
        // 右边框 (x=6)
        else if (x == 6)
        {
            if (y >= 4)
            {
                return new Vector2(-1, -1).normalized; // 左下
            }
            else if (y <= -4)
            {
                return new Vector2(-1, 1).normalized; // 左上
            }
            else
            {
                return new Vector2(-1, 0).normalized; // 向左
            }
        }
        
        // 默认方向（向下）
        return new Vector2(0, -1).normalized;
    }
#endregion
#region 冰云攻击（二符）
    /// <summary>
    /// 创建冰云方法
    /// </summary>
    /// <param name="prefab">冰云预制件</param>
    /// <param name="minPos">生成范围左下角</param>
    /// <param name="maxPos">生成范围右上角</param>
    /// <param name="count">生成数量</param>
    public void CreateCloud(GameObject prefab, Vector2 minPos, Vector2 maxPos, int count)
    {
        cloudPrefab = prefab;
        cloudSpawnMin = minPos;
        cloudSpawnMax = maxPos;
        cloudCount = count;
        
        // 生成初始冰云
        for (int i = 0; i < count; i++)
        {
            SpawnCloud();
        }
    }
    
    /// <summary>
    /// 生成单个冰云
    /// </summary>
    private void SpawnCloud()
    {
        if (cloudPrefab == null)
            return;
        
        // 在指定范围内随机生成位置
        float x = Random.Range(cloudSpawnMin.x, cloudSpawnMax.x);
        float y = Random.Range(cloudSpawnMin.y, cloudSpawnMax.y);
        Vector2 spawnPosition = new (x, y);
        
        // 使用对象池获取冰云
        GameObject cloudInstance = Global_ObjectPool.Instance.GetObject(cloudPrefab, spawnPosition, Quaternion.identity);
        cloudInstance.GetComponent<IceCloud>().bossShootSystem = this;
    }
    
    /// <summary>
    /// 当冰云被回收时调用，生成新的冰云
    /// </summary>
    public void OnCloudRecycled()
    {
        // 生成新的冰云
        SpawnCloud();
    }
#endregion
   
    
    /// <summary>
    /// 停止所有射击协程
    /// </summary>
    public void StopAllShooting()
    {
        // 停止所有协程
        StopAllCoroutines();
        
        // 取消所有 Invoke 调用
        CancelInvoke();
        
        // 停止所有子弹波次
        StopAllBulletWaves();
    }

    public void ShowTerrain()
    {
        StartCoroutine(ShowTerrainCoroutine());
    }
    private IEnumerator ShowTerrainCoroutine()
    {
        // 播放冰冻音效
        if (FrozeSound != null)
        {
            Global_AudioManager.Instance.PlaySFX(FrozeSound);
        }
        float duration = 0.5f;
        float elapsedTime = 0f;
        if (IceTerrainSprite != null)
        {
            Color color = IceTerrainSprite.color;
            color.a = 0f;
            IceTerrainSprite.color = color;
        }
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float alpha = Mathf.Lerp(0f, 0.4f, t);
            if (IceTerrainSprite != null)
            {
                Color color = IceTerrainSprite.color;
                color.a = alpha;
                IceTerrainSprite.color = color;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        IceTerrain.GetComponent<Collider2D>().enabled = true;
    }

    public void HideTerrain()
    {
        StartCoroutine(HideTerrainCoroutine());
    }
    private IEnumerator HideTerrainCoroutine()
    {
        IceTerrain.GetComponent<Collider2D>().enabled = false;
        float duration = 0.5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Color color = IceTerrainSprite.color;
            color.a = Mathf.Lerp(0.4f, 0f, t);
            IceTerrainSprite.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        IceTerrain.GetComponent<Collider2D>().enabled = false;
    }

    /// <summary>
    /// 恢复所有陨石的重力
    /// </summary>
    public void ResumeAllStonesGravity()
    {
        foreach (var stone in stones)
        {
            if (stone != null && stone.activeInHierarchy)
            {
                Rigidbody2D rb2D = stone.GetComponent<Rigidbody2D>();
                if (rb2D != null)
                {
                    rb2D.isKinematic = false;
                }
            }
        }
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShootSystem : MonoBehaviour
{
    public GameObject player;
    public GameObject boss;
    public GameObject IcePoint;
    public List<GameObject> IcePoints;
    public List<Sprite> icePointSprites; // icepoint 帧动画素材
    private const float animationSpeed = 0.1f; // 动画速度
    private int currentSpriteIndex = 0; // 当前动画帧索引
    private Vector2 Center = new Vector2(-3, 0);
    private int randomIcePickBulletCount = 5; // 随机射击的子弹数量
    
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
        List<GameObject> stones = new List<GameObject>();
        List<float> initialAngles = new List<float>(); // 保存每个陨石的初始角度
        
        // 生成随机目标点并发射陨石
        for (int i = 0; i < stoneCount; i++)
        {
            // 在中心点半径5范围内随机生成目标坐标
            float angle = Random.Range(0, Mathf.PI * 2);
            float radius = Random.Range(0, 5f);
            float targetX = Center.x + Mathf.Cos(angle) * radius;
            float targetY = Center.y + Mathf.Sin(angle) * radius;
            
            // 创建陨石子弹
            GameObject stone = Global_ObjectPool.Instance.GetObject(stoneBullet, new Vector3(targetX, 7f, 0), Quaternion.identity);
            if (stone != null)
            {
                Stone stoneScript = stone.GetComponent<Stone>();
                if (stoneScript != null)
                {
                    stoneScript.Initialize(targetX, targetY);
                    stones.Add(stone);
                    // 保存初始角度（弧度）
                    initialAngles.Add(angle);
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
                        frozenIceScript.ParentOb = stone;
                        frozenIceScript.normalIcePrefab = normalIceBullet;
                    }
                }
            }
        }
        
        // 开始旋转所有陨石
        float currentAngle = 0f;
        while (true)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            
            for (int i = 0; i < stones.Count; i++)
            {
                var stone = stones[i];
                if (stone != null && stone.activeInHierarchy && i < initialAngles.Count)
                {
                    // 计算每个陨石的旋转位置（使用初始角度加上当前旋转角度）
                    float angle = currentAngle * Mathf.Deg2Rad + initialAngles[i];
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
    /// 随机射击方法
    /// </summary>
    /// <param name="bullet">子弹预制件</param>
    /// <param name="bulletSpeed">射击速度</param>
    /// <param name="shootInterval">射击间隔</param>
    public void randomIcePick(GameObject bullet, float bulletSpeed, float shootInterval)
    {
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
    
    /// <summary>
    /// 当FrozenIce被摧毁时调用，增加随机射击的子弹数量
    /// </summary>
    public void OnFrozenIceDestroyed()
    {
        randomIcePickBulletCount++;
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
        // 发射12枚均匀分布的NormalIce子弹
        int bulletCount = 12;
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
                        normalIce.SetSpeed(5f);
                    }
                }
            }
        }
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
    }

}

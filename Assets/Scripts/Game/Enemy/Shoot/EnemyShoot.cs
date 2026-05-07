using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Track;
using static Tail;
using static Remain;
using static Normal;
using static Invisible;

public class EnemyShoot : MonoBehaviour
{
    [Header("射击配置列表")]
    public List<ShootMode> shootConfigs = new List<ShootMode>();
    
    private int currentShootConfigIndex = 0;
    private float shootTimer = 0f;
    private float modeDurationTimer = 0f;
    private GameObject player;
    private float currentSprialAngle = 0f;
    private int bulletColorIndex = 0; // 当前波次的子弹颜色索引
    private int enemyIndex = 0; // 敌人在波次中的索引
    private float angleOffset = 0f; // 角度偏移
    private float timeOffset = 0f; // 时间偏移
    
    void OnEnable()
    {
        // 重置射击计时器
        shootTimer = 0f;
        // 重置模式持续时间计时器
        modeDurationTimer = 0f;
        // 重置螺旋角度
        currentSprialAngle = 0f;
        // 重置配置索引
        currentShootConfigIndex = 0;
        // 随机初始化子弹颜色索引
        bulletColorIndex = Random.Range(0, 6); // 生成一个用于决定子弹变体的随机数
        // 基于enemyIndex计算随机偏移值
        Random.InitState(enemyIndex * 37); // 使用固定的种子确保相同索引的敌人有相同的偏移
        angleOffset = Random.Range(-20f, 20f); // 角度偏移范围：-20度到20度
        timeOffset = Random.Range(-0.1f, 0.1f); // 时间偏移范围：-0.1秒到0.1秒
        
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }
    
    void Update()
    {
        if(Global_GameManager.Instance.state == State.SpellCard)
        {
            return;
        }
        if (shootConfigs.Count > 0)
        {
            // 获取当前射击配置
            ShootMode currentConfig = shootConfigs[currentShootConfigIndex];
            
            if (currentConfig.shootMode != Shoot_Mode.none && currentConfig.bulletPrefab != null)
            {
                shootTimer += Time.deltaTime;
                if (shootTimer >= currentConfig.shootInterval + timeOffset)
                {
                    shootTimer = 0f;
                    // 发射子弹
                    FireBullet();
                }
            }
            
            // 检查模式持续时间
            modeDurationTimer += Time.deltaTime;
            if (modeDurationTimer >= currentConfig.duration && shootConfigs.Count > 1)
            {
                // 切换到下一个射击模式
                currentShootConfigIndex = (currentShootConfigIndex + 1) % shootConfigs.Count;
                modeDurationTimer = 0f;
                shootTimer = 0f;
                currentSprialAngle = 0f;
            }
        }
    }
    
    /// <summary>
    /// 发射子弹
    /// </summary>
    private void FireBullet()
    {
        if (shootConfigs.Count == 0) {
            Debug.LogWarning("No shoot configs available");
            return;
        }
        
        ShootMode currentConfig = shootConfigs[currentShootConfigIndex];
        
        switch (currentConfig.shootMode)
        {
            case Shoot_Mode.diverge:
                FireDivergeBullets();
                break;
            case Shoot_Mode.random:
                FireRandomBullets();
                break;
            case Shoot_Mode.sprial:
                FireSprialBullets();
                break;
        }
    }
    
    /// <summary>
    /// 发射发散圆子弹
    /// 需要的参数有：子弹数量、起始角度、角度范围、发射间隔
    /// </summary>
    private void FireDivergeBullets()
    {
        if (shootConfigs.Count == 0) return;
        
        ShootMode currentConfig = shootConfigs[currentShootConfigIndex];
        int bulletCount = currentConfig.bulletCount;
        if (bulletCount <= 0) return;
        
        float startAngle = currentConfig.shootAngle + angleOffset;
        float angleRange = currentConfig.angleRange;
        float angleStep = angleRange / bulletCount;
        
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            // 从对象池获取子弹
            GameObject bullet = Global_ObjectPool.Instance.GetObject(currentConfig.bulletPrefab, transform.position, rotation);
            if (bullet != null)
            {
                // 先禁用子弹
                bullet.SetActive(false);
                // 设置子弹参数
                SetBulletParameters(bullet);
                // 再激活子弹
                bullet.SetActive(true);
            } else {
                Debug.LogError($"Failed to get bullet from pool: {currentConfig.bulletPrefab?.name}");
            }
        }
    }
    
    /// <summary>
    /// 发射随机子弹
    /// 需要的参数有：子弹数量、起始角度、角度范围（在范围内随机化）、发射间隔
    /// </summary>
    private void FireRandomBullets()
    {
        if (shootConfigs.Count == 0) return;
        
        ShootMode currentConfig = shootConfigs[currentShootConfigIndex];
        int bulletCount = currentConfig.bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            // 随机角度
            float randomAngle = currentConfig.shootAngle + angleOffset + Random.Range(0f, currentConfig.angleRange);
            Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
            
            // 生成子弹
            GameObject bullet = Global_ObjectPool.Instance.GetObject(currentConfig.bulletPrefab, transform.position, rotation);
            if (bullet != null)
            {
                // 先禁用子弹
                bullet.SetActive(false);
                // 设置子弹参数
                SetBulletParameters(bullet);
                // 再激活子弹
                bullet.SetActive(true);
            }
        }
    }
    

    
    /// <summary>
    /// 发射螺旋子弹
    /// 需要的参数有：起始角度、角度范围（每次旋转的角度差）、发射间隔
    /// </summary>
    private void FireSprialBullets()
    {
        if (shootConfigs.Count == 0) return;
        
        ShootMode currentConfig = shootConfigs[currentShootConfigIndex];
        // 计算当前螺旋角度
        float angle = currentConfig.shootAngle + angleOffset + currentSprialAngle;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
        // 从对象池获取子弹
        GameObject bullet = Global_ObjectPool.Instance.GetObject(currentConfig.bulletPrefab, transform.position, rotation);
        if (bullet != null)
        {
            // 先禁用子弹
            bullet.SetActive(false);
            // 设置子弹参数
            SetBulletParameters(bullet);
            // 再激活子弹
            bullet.SetActive(true);
        }
        
        // 更新螺旋角度
        currentSprialAngle += currentConfig.angleRange;
    }
    

    
    /// <summary>
    /// 设置子弹参数
    /// </summary>
    /// <param name="bullet">子弹对象</param>
    private void SetBulletParameters(GameObject bullet)
    {
        if (shootConfigs.Count == 0) return;
        
        ShootMode currentConfig = shootConfigs[currentShootConfigIndex];
        
        // 设置跟踪子弹参数
        Track trackBullet = bullet.GetComponent<Track>();
        if (trackBullet != null)
        {
            trackBullet.Speed = currentConfig.trackBulletConfig.Speed;
            trackBullet.WindUp = currentConfig.trackBulletConfig.WindUp;
            // 传递玩家对象
            if (player != null)
            {
                trackBullet.SetTarget(player);
            }
            // 设置子弹颜色变体
            SetBulletSpriteVariant(trackBullet.spriteRenderer, trackBullet.spriteVariants);
        }
        
        // 设置拖尾子弹参数
        Tail tailBullet = bullet.GetComponent<Tail>();
        if (tailBullet != null)
        {
            tailBullet.CanClone = currentConfig.tailBulletConfig.CanClone;
            tailBullet.Speed = currentConfig.tailBulletConfig.Speed;
            tailBullet.CloneSpeed = currentConfig.tailBulletConfig.CloneSpeed;
            tailBullet.attenuation = currentConfig.tailBulletConfig.attenuation;
            tailBullet.MinSpeed = currentConfig.tailBulletConfig.MinSpeed;
            // 设置子弹颜色变体
            SetBulletSpriteVariant(tailBullet.spriteRenderer, tailBullet.spriteVariants);
        }
        
        // 设置滞留子弹参数
        Remain remainBullet = bullet.GetComponent<Remain>();
        if (remainBullet != null && currentConfig.remainBulletConfig != null)
        {
            remainBullet.LifeTime = currentConfig.remainBulletConfig.LifeTime;
            remainBullet.Speed = currentConfig.remainBulletConfig.Speed;
            remainBullet.WindUp = currentConfig.remainBulletConfig.WindUp;
            // 更新子弹速度向量
            Rigidbody2D remainRb = bullet.GetComponent<Rigidbody2D>();
            if (remainRb != null)
            {
                Vector2 direction = bullet.transform.up;
                remainRb.velocity = direction * remainBullet.Speed;
                remainRb.isKinematic = false;
            }
            // 设置子弹颜色变体
            SetBulletSpriteVariant(remainBullet.spriteRenderer, remainBullet.spriteVariants);
        }
        
        // 设置普通子弹参数
        Normal normalBullet = bullet.GetComponent<Normal>();
        if (normalBullet != null)
        {
            normalBullet.Speed = currentConfig.normalBulletConfig.Speed;
            // 更新子弹速度向量
            Rigidbody2D normalRb = bullet.GetComponent<Rigidbody2D>();
            if (normalRb != null)
            {
                Vector2 direction = bullet.transform.up;
                normalRb.velocity = direction * normalBullet.Speed;
                normalRb.isKinematic = false;
            }
            // 设置子弹颜色变体
            SetBulletSpriteVariant(normalBullet.spriteRenderer, normalBullet.spriteVariants);
        }
        
        // 设置不可见子弹参数
        Invisible invisibleBullet = bullet.GetComponent<Invisible>();
        if (invisibleBullet != null)
        {
            invisibleBullet.isVisible = currentConfig.invisibleBulletConfig.isVisible;
            invisibleBullet.Speed = currentConfig.invisibleBulletConfig.Speed;
            invisibleBullet.ShowDistance = currentConfig.invisibleBulletConfig.ShowDistance;
            invisibleBullet.ShowTime = currentConfig.invisibleBulletConfig.ShowTime;
            // 传递玩家对象
            if (player != null)
            {
                invisibleBullet.SetPlayer(player);
            }
            // 更新子弹速度向量
            Rigidbody2D invisibleRb = bullet.GetComponent<Rigidbody2D>();
            if (invisibleRb != null)
            {
                Vector2 direction = bullet.transform.up;
                invisibleRb.velocity = direction * invisibleBullet.Speed;
                invisibleRb.isKinematic = false;
            }
            // 设置子弹颜色变体
            SetBulletSpriteVariant(invisibleBullet.spriteRenderer, invisibleBullet.spriteVariants);
        }
        
        // 设置跟踪子弹速度向量
        // 获取Track组件用于设置速度向量（前面已设置过参数，这里只处理Rigidbody2D）
        Track trackBulletForVelocity = bullet.GetComponent<Track>();
        if (trackBulletForVelocity != null)
        {
            Rigidbody2D trackRb = bullet.GetComponent<Rigidbody2D>();
            if (trackRb != null)
            {
                Vector2 direction = bullet.transform.up;
                trackRb.velocity = direction * trackBulletForVelocity.Speed;
                trackRb.isKinematic = false;
            }
        }
        
        // 设置拖尾子弹速度向量
        Tail tailBulletForVelocity = bullet.GetComponent<Tail>();
        if (tailBulletForVelocity != null)
        {
            Rigidbody2D tailRb = bullet.GetComponent<Rigidbody2D>();
            if (tailRb != null)
            {
                Vector2 direction = bullet.transform.up;
                tailRb.velocity = direction * tailBulletForVelocity.Speed;
                tailRb.isKinematic = false;
            }
        }
        

    }
    
    /// <summary>
    /// 设置子弹的精灵变体
    /// </summary>
    /// <param name="spriteRenderer">精灵渲染器</param>
    /// <param name="spriteVariants">精灵变体列表</param>
    private void SetBulletSpriteVariant(SpriteRenderer spriteRenderer, List<Sprite> spriteVariants)
    {
        if (spriteRenderer != null && spriteVariants != null && spriteVariants.Count > 0)
        {
            // 使用bulletColorIndex确保同一波次的子弹颜色相同
            int index = bulletColorIndex % spriteVariants.Count;
            spriteRenderer.sprite = spriteVariants[index];
        }
    }
    
    /// <summary>
    /// 设置射击配置列表
    /// </summary>
    /// <param name="configs">射击配置列表</param>
    public void SetShootConfig(List<ShootMode> configs)
    {
        shootConfigs = configs;
    }
    
    /// <summary>
    /// 设置玩家对象
    /// </summary>
    /// <param name="playerObj">玩家对象</param>
    public void SetPlayer(GameObject playerObj)
    {
        player = playerObj;
    }
    
    /// <summary>
    /// 设置敌人在波次中的索引
    /// </summary>
    /// <param name="index">敌人在波次中的索引</param>
    public void SetEnemyIndex(int index)
    {
        enemyIndex = index;
    }
}

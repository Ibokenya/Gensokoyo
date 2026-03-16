using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 全局“池”型单例类
/// 管理妖精池，弹幕池，得点道具池
/// 都是为了优化性能
/// </summary>
public class Global_ObjectPool : Singleton<Global_ObjectPool>   
{
    // 存储不同类型的弹幕池
    private Dictionary<string, Queue<GameObject>> BulletPool = new();
    [Header("预生成数量")]
    public int BulletsInPool_Count = 40;

    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 初始化弹幕池
    /// </summary>
    public void InitPool(GameObject bulletPrefab)
    {
        string poolKey = bulletPrefab.name;
        if (BulletPool.ContainsKey(poolKey)) return;// 如果已经初始化过，直接返回
        Queue<GameObject> pool = new();
        BulletPool.Add(poolKey, pool);
        for (int i = 0; i < BulletsInPool_Count; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    /// <summary>
    /// 从弹幕池获取弹幕
    /// </summary>
    /// <param name="bulletPrefab">弹幕预制体</param>
    /// <param name="position">生成位置</param>
    /// <param name="rotation">生成旋转</param>
    /// <returns>获取到的弹幕</returns>
    public GameObject GetBullet(GameObject bulletPrefab, Vector3 position, Quaternion rotation)
    {
        string poolKey = bulletPrefab.name;
        GameObject bullet;
        // 检查是否有初始化过该类型的池
        if (!BulletPool.ContainsKey(poolKey))
        {
            InitPool(bulletPrefab); // 未初始化则自动初始化
            Debug.Log($"创建一个不在弹幕池的弹幕：{poolKey}");
        }
        if(BulletPool[poolKey].Count > 0)
        {
            bullet = BulletPool[poolKey].Dequeue();
        }
        else
        {
            bullet = Instantiate(bulletPrefab,transform);
        }
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.SetActive(true);
        return bullet;
    }

    /// <summary>
    /// 回收弹幕到池子里
    /// </summary>
    /// <param name="bullet">要回收的弹幕</param>
    public void RecycleBullet(GameObject bullet)
    {
        string poolKey = bullet.name.Replace("(Clone)", ""); // 移除克隆后缀，匹配预制体名
        bullet.SetActive(false);
        bullet.transform.SetParent(transform); // 归位到对象池父物体

        // 确保池存在，再放回
        if (BulletPool.ContainsKey(poolKey))
        {
            BulletPool[poolKey].Enqueue(bullet);
        }
        else
        {
            // 未初始化的池：直接销毁
            Destroy(bullet);
            Debug.Log($"回收一个不在弹幕池的弹幕：{poolKey}");
        }
    }

    /// <summary>
    /// 清空所有弹幕池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in BulletPool.Values)
        {
            while (pool.Count > 0)
            {
                GameObject bullet = pool.Dequeue();
                Destroy(bullet);
            }
        }
        BulletPool.Clear();
        Debug.Log("清空所有弹幕池");
    }
}

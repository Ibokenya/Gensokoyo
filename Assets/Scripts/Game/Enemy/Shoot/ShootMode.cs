using UnityEngine;

public enum Shoot_Mode
{
    diverge,// 发散圆
    track,// 跟踪
    remain,// 滞留
    sprial,// 螺旋
    bevel,// 固定斜角
    none// 无射击
}

[System.Serializable]
public class TrackBulletConfig
{
    [Header("移动速度")]
    public float Speed = 5f;
}

[System.Serializable]
public class TailBulletConfig
{
    [Header("是否可以自我复制")]
    public bool CanClone = false;
    [Header("移动速度")]
    public float Speed = 5f;
    [Header("复制间隔")]
    public float CloneSpeed = 1f;
    [Header("衰减系数")]
    public float attenuation = 0.5f;
    [Header("最小速度")]
    public float MinSpeed = 3f;
}

[System.Serializable]
public class RemainBulletConfig
{
    [Header("生存时间")]
    public float LifeTime = 5f;
}

[System.Serializable]
public class NormalBulletConfig
{
    [Header("移动速度")]
    public float Speed = 5f;
}

[System.Serializable]
public class InvisibleBulletConfig
{
    [Header("是否可见")]
    public bool isVisible = true;
    [Header("移动速度")]
    public float Speed = 5f;
    [Header("靠近显形距离")]
    public float ShowDistance = 10f;
    [Header("淡入时间")]
    public float ShowTime = 1f;
}

[System.Serializable]
public class ShootMode
{
    [Header("射击模式")]
    public Shoot_Mode shootMode = Shoot_Mode.none;
    
    [Header("射击速度")]
    public float shootSpeed = 5f;
    
    [Header("射击间隔")]
    public float shootInterval = 1f;
    
    [Header("子弹数量")]
    public int bulletCount = 1;
    
    [Header("子弹预制体")]
    public GameObject bulletPrefab;
    
    [Header("射击角度")]
    public float shootAngle = 0f;
    
    [Header("角度范围")]
    public float angleRange = 360f;
    
    [Header("跟踪子弹参数")]
    public TrackBulletConfig trackBulletConfig = new TrackBulletConfig();
    
    [Header("拖尾子弹参数")]
    public TailBulletConfig tailBulletConfig = new TailBulletConfig();
    
    [Header("滞留子弹参数")]
    public RemainBulletConfig remainBulletConfig = new RemainBulletConfig();
    
    [Header("普通子弹参数")]
    public NormalBulletConfig normalBulletConfig = new NormalBulletConfig();
    
    [Header("不可见子弹参数")]
    public InvisibleBulletConfig invisibleBulletConfig = new InvisibleBulletConfig();
}

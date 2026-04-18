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
                
                // 随机子弹数量（4-8枚）
                int bulletCount = Random.Range(4, 9);
                
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
    
    public void IcePoint_Shoot(GameObject bullet1, GameObject bullet2, float interval1, float interval2, float rotationSpeed, float bullet1Speed = -1f, float bullet2Speed = -1f)
    {
        // 首先发射预制件1的弹幕（360° 12个），不使用速度偏移
        foreach (var icePoint in IcePoints)
        {
            if (icePoint != null)
            {
                FireBulletRing(icePoint, bullet1, 12, false, bullet1Speed);
            }
        }
        
        // 启动后续射击和旋转逻辑
        StartCoroutine(IcePointShootCoroutine(bullet2, interval1, interval2, rotationSpeed, bullet2Speed));
    }
    
    // 冰点发射一圈环状子弹
    private void FireBulletRing(GameObject icePoint, GameObject bullet, int count, bool useSpeedOffset = true, float speed = -1f)
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
    
    private IEnumerator IcePointShootCoroutine(GameObject bullet2, float interval1, float interval2, float rotationSpeed, float bullet2Speed = -1f)
    {
        // 等待 interval1 后开始发射预制件2
        yield return new WaitForSeconds(interval1);
        
        while (true)
        {
            // 发射预制件2的弹幕（360° 12个），使用速度偏移
            foreach (var icePoint in IcePoints)
            {
                if (icePoint != null)
                {
                    FireBulletRing(icePoint, bullet2, 12, true, bullet2Speed);
                }
            }
            
            // 旋转 icepoint
            for (int i = 0; i < IcePoints.Count; i++)
            {
                var icePoint = IcePoints[i];
                if (icePoint != null)
                {
                    // 0-2 正常旋转，3-5 反向旋转
                    float rotateDirection = (i < 3) ? 1f : -1f;
                    icePoint.transform.Rotate(Vector3.up, rotateDirection * rotationSpeed * Time.deltaTime);
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

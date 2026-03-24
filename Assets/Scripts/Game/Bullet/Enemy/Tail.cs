using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拖尾弹Tail
/// 移动时会不断以更低的速度复制自己
/// 姬虫百百世同款
/// 每次复制令速度-1直到为0
/// 子弹
/// </summary>
public class Tail : MonoBehaviour
{
    public bool CanClone = false;// 是否可以自我复制（仅母弹可以）
    public float Speed = 5f;// 移动速度
    public float CloneSpeed = 1f;// 复制间隔
    public float attenuation = 0.5f;// 衰减系数
    public float MinSpeed = 3f;// 最小速度(小于该值将不再复制)
}

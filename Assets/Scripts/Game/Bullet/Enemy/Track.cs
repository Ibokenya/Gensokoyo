using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 跟踪子弹Track
/// 移动时会根据目标机的位置进行移动（仅追踪一次）
/// 箭头弹，符箓弹，苦无弹
/// </summary>
public class Track : MonoBehaviour
{
    public float Speed = 5f;// 移动速度
    public GameObject Target;// 目标机对象
}

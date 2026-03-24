using UnityEngine;

/// <summary>
/// 不可见子弹Invisible
/// 当靠近自机时才会显现
/// 中玉，大玉
/// </summary>
public class Invisible : MonoBehaviour
{
    public bool isVisible = true;
    public float Speed = 5f;
    public float ShowDistance = 10f;// 靠近显形距离
    public float ShowTime = 1f;// 淡入时间

}

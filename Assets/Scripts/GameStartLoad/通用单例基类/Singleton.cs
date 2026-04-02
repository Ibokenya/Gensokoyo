using UnityEngine;

/// <summary>
/// Unity MonoBehaviour通用单例基类
/// 特点：全局唯一、跨场景存活、外部可通过 Instance 访问
/// </summary>
/// <typeparam name="T">要做成单例的脚本类型（如GameManager、AudioManager）</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 静态实例（核心：外部通过 Instance 访问）
    private static T _instance;

    // 公开的实例访问器（加锁保证线程安全，可选但推荐）
    public static T Instance
    {
        get
        {
            // 1. 如果实例为空，先在场景中查找
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                // 2. 场景中找不到，自动创建一个空物体挂载该脚本
                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject(typeof(T).Name);
                    _instance = singletonObj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    // 初始化逻辑（Awake是Unity最早的生命周期，适合做单例初始化）
    protected virtual void Awake()
    {
        // 保证单例唯一：如果已有实例，销毁当前重复的
        if (_instance == null)
        {
            _instance = this as T;
            // 标记为跨场景不销毁（核心！）
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
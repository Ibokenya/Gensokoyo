using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 全局游戏管理单例
/// 存储机体，难度，残机，得点等数据
/// 以及状态机，还有生成敌人相关的波次管理
/// </summary>
public class Global_GameManager : Singleton<Global_GameManager>
{
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
}

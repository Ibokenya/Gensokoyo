using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 全局BGM播放给管理单例类
/// 游戏是一定要有音乐的，自然得有个地方来管理歌单
/// 之后还要对照音乐的节点设计敌人波次，想想都麻烦
/// 这里还可以设置音量（应该）
/// </summary>
public class Global_AudioManager : Singleton<Global_AudioManager>
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;



[Hotfix]// 热更新类，用于在运行时动态加载和卸载脚本
public class koishi : MonoBehaviour
{
    public float RotationSpeed = 100f;

    void Start()
    {
    }

    void Update()
    {
        this.transform.Rotate(0, 0, RotationSpeed * Time.deltaTime);
    }
}

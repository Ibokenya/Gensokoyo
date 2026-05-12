using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarisaShoot : MonoBehaviour
{
    [Header("激光配置")]
    public GameObject laserPrefab; // 激光预制体

    private Laser laser; // 激光组件引用
    private bool isLaserActive = false; // 激光是否激活

    void OnEnable()
    {
        if(Input.GetKey(KeyCode.Z))
            CreatLaser();
    }
    void OnDisable()
    {
        CleanLaser();
    }

    // Update is called once per frame
    void Update()
    {
        if(Global_GameManager.Instance != null && 
        Global_GameManager.Instance.state != State.Gaming && 
        Global_GameManager.Instance.state != State.NoDead) return;
        // 检测 Z 键按下
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CreatLaser();
        }
        // 检测 Z 键抬起
        else if (Input.GetKeyUp(KeyCode.Z))
        {
            CleanLaser();
        }
    }

    private void CreatLaser()
    {
        if (!isLaserActive && laserPrefab != null)
        {
            // 实例化激光预制体
            GameObject laserObj = Instantiate(laserPrefab, transform.position, transform.rotation);
            // 设置激光对象为发射点对象的子对象
            laserObj.transform.parent = transform;
            // 重置本地位置为(0,0,0)
            laserObj.transform.localPosition = Vector3.zero;
            laser = laserObj.GetComponent<Laser>();
            
            if (laser != null)
            {
                laser.ActivateLaser();
                isLaserActive = true;
            }
            else
            {
                Debug.LogError("Laser component not found on instantiated prefab!");
                Destroy(laserObj);
            }
        }
    }
    private void CleanLaser()
    {
        if (isLaserActive && laser != null)
        {
            laser.StopLaser();
            // 延迟销毁激光对象，确保视觉效果完成
            Destroy(laser.gameObject, 0.1f);
            laser = null;
            isLaserActive = false;
        }
    }
}

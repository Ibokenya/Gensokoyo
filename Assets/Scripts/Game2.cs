using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 检测R键或ESC键按下，返回菜单界面
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReturnToMenu();
        }
    }

    /// <summary>
    /// 返回菜单界面
    /// </summary>
    private void ReturnToMenu()
    {
        // 回收所有敌人
        if (Global_GameManager.Instance != null)
        {
            Global_GameManager.Instance.RecycleAllEnemies();
        }
        
        // 切换到菜单场景
        Global_SceneManager.Instance.IntoNextScene("GameStartMenu", false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game2 : MonoBehaviour
{
    public GameObject Msg1;
    public GameObject Msg2;

    public Animator FinalAnime;

    public BGImageScroll starBgScroll;

    void Start()
    {
        // 启动10秒延迟协程
        StartCoroutine(DelayAfterActivate());
    }

    IEnumerator DelayAfterActivate()
    {
        yield return new WaitForSeconds(10f);

        // 禁用Msg1和Msg2
        if (Msg1 != null)
        {
            Msg1.SetActive(false);
        }
        if (Msg2 != null)
        {
            Msg2.SetActive(false);
        }

        // 设置Animator的IsAnime为true
        if (FinalAnime != null)
        {
            FinalAnime.SetBool("IsAnime", true);
        }
    }

    public void StartBgMove()
    {
        // 开启Star的纹理偏移
        if (starBgScroll != null)
        {
            starBgScroll.enabled = true;
        }
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

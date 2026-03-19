using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


/// <summary>
/// 全局场景管理单例类
/// 管理场景之间的切换，回退，加载与删除
/// 以及切换场景的动画（如果有的话）
/// </summary>
/// 该类目前提供的对外函数有：
/// IntoNextScene（进入下一个场景，函数内部会自动处理删除旧场景，重置数据等操作）
public class Global_SceneManager : Singleton<Global_SceneManager>
{
    private bool _isAllSceneLoaded = false;// 全部场景预加载完成的标志位
    public bool IsAllSceneLoaded => _isAllSceneLoaded;// 外界可以只读访问_isAllSceneLoaded

    public string CurrentSceneName = "GameStartLoading";// 当前场景名称(初始为首个场景)

    private List<string> _LoadedSceneNames = new ();// 存储已加载场景的名称用于检索以及回退

    [Header("下一场景配置")]// 游戏启动后默认跳转的菜单场景名称（Inspector面板可配置）
    [SerializeField] private string _menuSceneName = "GameStartMenu";

    [SerializeField]
    private List<string> _sceneToPreload = new ()// 所有需要预加载的场景名称
    {
        "GameStartMenu","Game1","Game2","GameOver"
    };


    [Header("需要重置数据，且不卸载的场景")]// 标记需要重置业务数据的场景（目前只有：菜单）
    [SerializeField] private List<string> _needResetScenes = new () { "GameStartMenu" };

    protected override void Awake()
    {
        base.Awake(); // 调用基类的Awake，保证单例生效

        // 检测当前实际场景，更新 CurrentSceneName
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.IsValid())
        {
            CurrentSceneName = currentScene.name;
            Debug.Log($"全局场景管理器初始化，当前场景：{CurrentSceneName}");
        }
        
        // 只有当当前场景是 GameStartLoading 时才预加载场景
        // 这样直接运行其他场景时不会重复预加载
        if (CurrentSceneName == "GameStartLoading")
        {
            StartCoroutine(PreLoadAllScenes());
        }
        else
        {
            Debug.Log("非初始场景，跳过批量预加载");
            _isAllSceneLoaded = true; // 标记为已加载，避免影响后续操作
        }
    }

    /// <summary>
    /// 异步预加载所有的场景
    /// </summary>
    /// <returns></returns>
    private IEnumerator PreLoadAllScenes()
    {
        Debug.Log("开始批量加载所有场景...");
        _LoadedSceneNames.Clear();// 清空已加载场景列表名单（毕竟还没开始加载对吧？）
        _isAllSceneLoaded = false;// 置加载完毕标志位为false

        // 逐个加载配置的场景（不激活）
        foreach (string sceneName in _sceneToPreload)
        {
            // 跳过已加载的场景（按理说不该出现这种情况）
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                _LoadedSceneNames.Add(sceneName);
                Debug.Log("场景" + sceneName + "已加载，跳过批量预加载环节。话说不该这样的");
                continue;// 直接跳到下一个foreach循环
            }

            // 异步加载场景（就这两行够学的了）
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = false;

            // 等待加载完成（Unity中进度到0.9即表示资源加载完毕）
            while (asyncOp.progress < 0.9f)
            {
                Debug.Log($"预加载场景 {sceneName} 进度：{asyncOp.progress * 100:F1}%");
                yield return null;
            }
            Debug.Log($"预加载场景 {sceneName} 完成：{asyncOp.progress * 100:F1}%");

            // 关键：允许场景激活并等待完全加载
            asyncOp.allowSceneActivation = true;
            while (!asyncOp.isDone)
            {
                yield return null;
            }
            
            // 场景刚加载完成，立即禁用所有根物体（在同一帧内）
            DisableAllRootObjects(sceneName);

            _LoadedSceneNames.Add(sceneName);
            Debug.Log($"场景 {sceneName} 预加载完成（未激活）");
        }//foreach循环尾

        // 所有场景均加载完毕了
        _isAllSceneLoaded = true;
        Debug.Log($"所有场景均加载完毕，共{_LoadedSceneNames.Count}个");

    // 只有当当前场景是 GameStartLoading 时才自动切换到 menu 场景
    // 这样直接运行 menu 场景时就不会自动切换回 menu 场景
    if (CurrentSceneName == "GameStartLoading" && !string.IsNullOrEmpty(_menuSceneName))
    {
        // 这里设置默认的最小加载时间，确保场景切换时有足够的过渡时间
        IntoNextScene(_menuSceneName, false, 3f); // 3秒的最小加载时间       
    }
    }

    /// 单独加载下一个场景
    /// </summary>
    /// <param name="nextSceneName">需要加载的下一场景的名称</param>
    /// <param name="isActiveNow">是否在加载后立刻激活，true为立刻激活选项</param>
    /// <param name="minLoadTime">最小加载时间（秒），确保加载过程至少持续指定时间</param>
    /// <returns></returns>
    public IEnumerator LoadNextScene(string nextSceneName, bool isActiveNow, float minLoadTime = 0.1f)
    {
        float startTime = Time.time;
        AsyncOperation asyncOp;

        if (SceneManager.GetSceneByName(nextSceneName).isLoaded)
        {
            Debug.Log($"场景{nextSceneName}已经加载过了呀！");
            if (isActiveNow)
            {
                ActivateSceneWithReset(nextSceneName);// 连加载带重置
            }
            
            // 确保最小加载时间
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minLoadTime)
            {
                yield return new WaitForSeconds(minLoadTime - elapsedTime);
            }
            
            yield break;// 如果场景已加载则直接激活并跳出该协程
        }

        // 异步加载单个场景
        Debug.Log($"开始加载单个场景{nextSceneName}");
        asyncOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        asyncOp.allowSceneActivation = false; // 不激活场景

        // 等待加载完成 - 当allowSceneActivation=false时，isDone永远不会为true
        // 所以改为检查进度是否达到0.9f（Unity中0.9表示资源加载完成）
        while (asyncOp.progress < 0.9f)
        {
            Debug.Log($"加载场景 {nextSceneName} 进度：{asyncOp.progress * 100:F1}%");
            yield return null;
        }
        
        Debug.Log($"加载场景 {nextSceneName} 资源完成，进度：{asyncOp.progress * 100:F1}%");

        // 关键：允许场景激活并等待完全加载
        // 如果不这样做，SceneManager.SetActiveScene会失败，因为场景还没有完全加载
        asyncOp.allowSceneActivation = true;
        
        // 等待场景完全加载
        while (!asyncOp.isDone)
        {
            yield return null;
        }
        Debug.Log($"场景 {nextSceneName} 已完全加载");

        // 将该场景记录入已加载场景列表
        if (!_LoadedSceneNames.Contains(nextSceneName))
        {
            _LoadedSceneNames.Add(nextSceneName);
        }

        // 激活/禁用控制
        if (isActiveNow)
        {
            ActivateSceneWithReset(nextSceneName);
        }
        else
        {
            DisableAllRootObjects(nextSceneName);
        }

        // 确保最小加载时间
        float totalElapsedTime = Time.time - startTime;
        if (totalElapsedTime < minLoadTime)
        {
            yield return new WaitForSeconds(minLoadTime - totalElapsedTime);
        }

        Debug.Log($"已完成对场景{nextSceneName}的单独加载");
    }

    /// <summary>
    /// 删除/隐藏当前场景
    /// </summary>
    /// <param name="sceneName">要处理的场景的名称（通常是当前场景名称）</param>
    /// <param name="isHide">是否隐藏，当该值为true时只隐藏刚当前场景（可能回退），为false时删除</param>
    public void DeleteCurrentScene(string sceneName, bool isHide)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            Debug.Log($"场景{sceneName}不存在！立刻自检！");
            return; // 场景不存在直接返回，避免后续报错
        }

        if (!isHide)// 把场景卸载掉
        {
            SceneManager.UnloadSceneAsync(sceneName);
            _LoadedSceneNames.Remove(sceneName);
            Debug.Log($"已删除场景{sceneName}");
        }
        else// isHide=true时，隐藏场景（禁用所有根物体）
        {
            DisableAllRootObjects(sceneName);
            Debug.Log($"已隐藏场景{sceneName}");
        }
    }

    /// <summary>
    /// 跳转下一个场景（该函数启动协程，协程内部则再调用删除/隐藏当前场景的逻辑）
    /// </summary>
    /// <param name="NextSceneName">跳转的目标场景名称</param>
    /// <param name="isHide">处理当前场景时是否隐藏当前场景（或者删除）</param>
    /// <param name="minLoadTime">最小加载时间（秒），确保加载过程至少持续指定时间</param>
    public void IntoNextScene(string NextSceneName, bool isHide, float minLoadTime = 0f)
    {
        StartCoroutine(IntoNextSceneCoroutine(NextSceneName, isHide, minLoadTime));
    }

    private IEnumerator IntoNextSceneCoroutine(string NextSceneName, bool isHide, float minLoadTime = 0.1f)
    {
        float startTime = Time.time;
        Debug.Log($"【场景切换】开始：从{CurrentSceneName}到{NextSceneName}，isHide={isHide}");

        SimpleWaitingAnime(CurrentSceneName, NextSceneName);// 播放动画

        // 先确保新场景已加载
        Debug.Log($"【场景切换】检测新场景{NextSceneName}是否加载");
        if (!SceneManager.GetSceneByName(NextSceneName).isLoaded)// 未加载
        {
            yield return StartCoroutine(LoadNextScene(NextSceneName, false, minLoadTime));
            Debug.Log($"【场景切换】新场景{NextSceneName}已加载");
        }
        else
        {
            // 确保最小加载时间
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minLoadTime)
            {
                yield return new WaitForSeconds(minLoadTime - elapsedTime);
            }
            Debug.Log($"【场景切换】新场景{NextSceneName}已预加载，等待完成");
        }

        // 等待最小加载时间
        float totalElapsedTime = Time.time - startTime;
        if (totalElapsedTime < minLoadTime)
        {
            yield return new WaitForSeconds(minLoadTime - totalElapsedTime);
        }

        // 处理旧场景（删除/隐藏）
        if (!string.IsNullOrEmpty(CurrentSceneName))
        {
            Debug.Log($"【场景切换】处理旧场景：{CurrentSceneName}");
            DeleteCurrentScene(CurrentSceneName, isHide);
        }

        // 激活下一场景并重置状态
        ActivateSceneWithReset(NextSceneName);

        // 更新当前场景名称
        CurrentSceneName = NextSceneName;
        switch(CurrentSceneName)
        {
            case "GameStartMenu":
                Global_AudioManager.Instance.PlayBGM("Menu");
                break;
            case "Game1":
                Global_AudioManager.Instance.PlayBGM("Game1");
                break;
            default:
                Debug.LogWarning($"未配置场景{CurrentSceneName}的背景音乐");
                break;
        }
        Debug.Log($"已跳转到场景{CurrentSceneName}");
    }

    /// <summary>
    /// 激活场景 + 重置场景数据
    /// </summary>
    private void ActivateSceneWithReset(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) return;

        // 在写了JSON文件后，激活物体以及数据重置都应该参照JSON内容进行
        // 先激活场景中的所有根物体
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            rootObj.SetActive(true);
        }
        
        // 然后设置为活动场景
        SceneManager.SetActiveScene(scene);
        
        // 对需要重置的场景从JSON文件读取数据进行重置
        if (_needResetScenes.Contains(sceneName))
        {
            ResetSceneFromJson(sceneName);
        }
    }

    /// <summary>
    /// 从JSON文件读取场景数据并进行重置
    /// </summary>
    /// <param name="sceneName">需要重置的场景名称</param>
    private void ResetSceneFromJson(string sceneName)
    {
        // 1. 拼接JSON文件路径（Assets/Touho/JSON/[场景名]_ResetConfig.json）
        string jsonFileName = sceneName == "GameStartMenu" ? "MenuScene_ResetConfig.json" : $"{sceneName}_ResetConfig.json";
        string jsonFilePath = Path.Combine(Application.dataPath, "Touho/JSON", jsonFileName);

        // 2. 检查文件是否存在
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogWarning($"未找到JSON文件：{jsonFilePath}");
            return;
        }

        // 3. 读取JSON文件内容
        string jsonContent = File.ReadAllText(jsonFilePath);

        // 4. 反序列化为C#对象（修复JSON格式兼容问题）
        SceneResetConfig config = JsonUtility.FromJson<SceneResetConfig>(jsonContent);
        if (config == null || config.objectStates == null)
        {
            Debug.LogError($"JSON文件格式错误，无法解析：{jsonFilePath}");
            return;
        }

        // 5. 获取目标场景
        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        if (!targetScene.IsValid())
        {
            Debug.LogError($"场景 {sceneName} 无效！");
            return;
        }

        // 6. 遍历配置，设置物体激活状态（修复Transform命名冲突）
        foreach (ObjectState objState in config.objectStates)
        {
            GameObject targetObj = FindObjectByPath(targetScene, objState.objectPath);
            if (targetObj != null)
            {
                targetObj.SetActive(objState.isActive);
                Debug.Log($"已设置 {objState.objectPath} 为 {(objState.isActive ? "激活" : "隐藏")}");
            }
            else
            {
                Debug.LogWarning($"场景 {sceneName} 中未找到物体：{objState.objectPath}");
            }
        }

        // 7. 设置默认选中按钮
        if (config.resetData != null && !string.IsNullOrEmpty(config.resetData.defaultSelectedButton))
        {
            GameObject defaultBtn = FindObjectByPath(targetScene, config.resetData.defaultSelectedButton);
            if (defaultBtn != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(defaultBtn);
                Debug.Log($"默认选中按钮：{config.resetData.defaultSelectedButton}");
            }
        }
    }

    /// <summary>
    /// 辅助方法：根据层级路径在指定场景中查找物体
    /// </summary>
    private GameObject FindObjectByPath(Scene scene, string objectPath)
    {
        string[] pathParts = objectPath.Split('/');
        if (pathParts.Length == 0) return null;

        // 查找根物体
        GameObject rootObj = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == pathParts[0])
            {
                rootObj = root;
                break;
            }
        }
        if (rootObj == null) return null;

        // 递归查找子物体（明确指定UnityEngine.Transform，避免命名冲突）
        UnityEngine.Transform currentTrans = rootObj.transform;
        for (int i = 1; i < pathParts.Length; i++)
        {
            currentTrans = currentTrans.Find(pathParts[i]);
            if (currentTrans == null) return null;
        }

        return currentTrans.gameObject;
    }

    /// <summary>
    /// 禁用场景所有根物体（预加载/隐藏时用）
    /// </summary>
    private void DisableAllRootObjects(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) return;

        // 禁用所有根物体
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            rootObj.SetActive(false);
        }
    }

    /// <summary>
    /// 场景切换之间的简易小动画（我还得做技美？？！）
    /// 按照不同场景进行不同的动画设计
    /// 1.加载转菜单———
    /// 2.关卡跳转———截屏，缩放，体现分数，熄屏，少女祈祷中
    /// 3.关卡返回菜单———
    /// </summary>
    public void SimpleWaitingAnime(string currentSceneName, string nextSceneName)
    {
        switch (currentSceneName)
        {
            case "GameStartLoading":// 进入游戏菜单，最初始的动画
                Debug.Log("场景动画之。。当前位于" + currentSceneName);
                break;
            case "GameStartMenu":
                Debug.Log("场景动画之。。当前位于"+currentSceneName);
                break;
            case "Game1":
                if (nextSceneName == "Game2")// 关卡跳转
                {

                }
                else if (nextSceneName == "GameStartMenu")// 回退菜单
                {

                }
                break;
            case "Game2":
                break;
            default:// 其他
                Debug.Log("出了问题，定位在-全局场景单例类的简易动画函数");
                break;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using XLua;
using System.Collections.Generic;

// Addressables + XLua 远程热更加载器
// Addressables 初始化 → 更新 Catalog → 下载远程 Lua + 帧图 → xlua.hotfix 替换 C# 方法
public class AAHotfixLoader : MonoBehaviour
{
    public static AAHotfixLoader Instance { get; private set; }

    public bool LoadSucceeded { get; private set; }
    public LuaEnv LuaEnv { get; private set; }
    public List<Sprite> Frames { get; private set; } = new List<Sprite>();

    // Addressables Group 里的 Address 要和这里一致
    const string HOTFIX_LUA_ADDRESS = "Assets/HotfixScripts/koishi_hotfix.lua.txt";
    const string SPRITE_LABEL = "koishi_frames";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(InitHotfixRoutine());
    }

    void OnDestroy()
    {
        if (LuaEnv != null)
        {
            try
            {
                LuaEnv.Dispose();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[AAHotfixLoader] LuaEnv.Dispose 忽略异常（app 退出中）: " + ex.Message);
            }
            LuaEnv = null;
        }
    }

    IEnumerator InitHotfixRoutine()
    {
        Debug.Log("[AAHotfixLoader] 热更流程开始");

        // 1. 初始化 Addressables
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        Debug.Log("[AAHotfixLoader] Addressables 初始化完成");

        // 2. 加载帧图（可选，V1 回滚包可能没有）
        var spritesHandle = Addressables.LoadAssetsAsync<Sprite>(SPRITE_LABEL, null);
        yield return spritesHandle;
        if (spritesHandle.Status == AsyncOperationStatus.Succeeded && spritesHandle.Result.Count > 0)
        {
            Frames = new List<Sprite>(spritesHandle.Result);
            // 按 Sprite.name 数字排序（"1"→"2"→...→"12"，避免字符串排序陷阱）
            Frames.Sort((a, b) => {
                if (int.TryParse(a.name, out int na) && int.TryParse(b.name, out int nb))
                    return na.CompareTo(nb);
                return string.Compare(a.name, b.name);
            });
            Debug.Log($"[AAHotfixLoader] 帧图加载排序成功，共 {Frames.Count} 张");
        }
        else
        {
            // 帧图不存在（V1 回滚场景），跳过，lua 里自动 fallback
            Debug.LogWarning($"[AAHotfixLoader] 帧图不存在或加载失败（Status={spritesHandle.Status}），跳过帧图注入，回退为 v0.1.0 版本");
        }

        // 3. 创建 LuaEnv
        LuaEnv = new LuaEnv();
        Debug.Log("[AAHotfixLoader] LuaEnv 创建成功");

        // 4. 加载远程 Lua 热更脚本
        var luaHandle = Addressables.LoadAssetAsync<TextAsset>(HOTFIX_LUA_ADDRESS);
        yield return luaHandle;
        if (luaHandle.Status != AsyncOperationStatus.Succeeded || luaHandle.Result == null)
        {
            Debug.LogError($"[AAHotfixLoader] Lua 脚本加载失败！Address={HOTFIX_LUA_ADDRESS} Status={luaHandle.Status}");
            LoadSucceeded = false;
            yield break;
        }
        string luaContent = luaHandle.Result.text;
        Debug.Log("[AAHotfixLoader] Lua 脚本下载成功，大小 " + luaContent.Length + " 字符");

        // 5. 把帧图注入 Lua 全局变量
        LuaEnv.Global.Set("frames", Frames);
        LuaEnv.Global.Set("frame_count", Frames.Count);

        // 6. 执行热更脚本（内部调 xlua.hotfix 替换 C# 方法）
        try
        {
            LuaEnv.DoString(luaContent);
            Debug.Log("[AAHotfixLoader] Lua hotfix 执行成功");
            LoadSucceeded = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AAHotfixLoader] Lua hotfix 执行异常: {e.Message}\n{e.StackTrace}");
            LoadSucceeded = false;
        }
    }
}
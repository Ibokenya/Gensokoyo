using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using XLua;
using System.Collections.Generic;

/// <summary>
/// 热更加载器
/// 挂在 GameStartLoading 场景的空 GameObject 上
/// 流程：AA 初始化 → 更新 Catalog → 下载远程 Lua + 帧图 → xlua.hotfix 替换
/// 
/// 远程热更资源（都要打 Addressables 远程包）：
///   - Assets/HotfixScripts/koishi_hotfix.lua.txt  → TextAsset
///   - Assets/AA_assets/1.png ~ 12.png              → Sprite（Label: koishi_frames）
/// </summary>
public class AAHotfixLoader : MonoBehaviour
{
    // ── 单例 ──────────────────────────────────────────────
    public static AAHotfixLoader Instance { get; private set; }

    // ── 核心状态 ──────────────────────────────────────────
    public bool LoadSucceeded { get; private set; }
    public LuaEnv LuaEnv { get; private set; }
    public List<Sprite> Frames { get; private set; } = new List<Sprite>();

    // ── Addressables 地址（要和 Addressables Group 里的 Address 一致） ──
    const string HOTFIX_LUA_ADDRESS = "Assets/HotfixScripts/koishi_hotfix.lua.txt";
    const string SPRITE_LABEL = "koishi_frames";

    // ── 生命周期 ──────────────────────────────────────────
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
                // XLua Dispose 会抛 callback 未释放异常（xlua.hotfix 注册的回调）
                // 应用退出或场景销毁时，OS 会清理资源，忽略即可
                Debug.LogWarning("[AAHotfixLoader] LuaEnv.Dispose 忽略异常（app 退出中）: " + ex.Message);
            }
            LuaEnv = null;
        }
    }

    // ── 热更初始化协程 ────────────────────────────────────
    IEnumerator InitHotfixRoutine()
    {
        Debug.Log("[AAHotfixLoader] === Lua 热更流程开始 ===");

        // 1. 初始化 Addressables
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;
        Debug.Log("[AAHotfixLoader] ① Addressables 初始化完成");

        // 2. 加载帧图（可选，V1 回滚包可能没有）
        var spritesHandle = Addressables.LoadAssetsAsync<Sprite>(SPRITE_LABEL, null);
        yield return spritesHandle;
        if (spritesHandle.Status == AsyncOperationStatus.Succeeded && spritesHandle.Result.Count > 0)
        {
            Frames = new List<Sprite>(spritesHandle.Result);
            // 按 Sprite.name 排序，确保 1→2→3→...→12 的正确顺序
            Frames.Sort((a, b) => {
                // 提取数字部分比较，支持 "1.png" "10.png" 这种字符串排序陷阱
                if (int.TryParse(a.name, out int na) && int.TryParse(b.name, out int nb))
                    return na.CompareTo(nb);
                return string.Compare(a.name, b.name);
            });
            Debug.Log($"[AAHotfixLoader] ② 帧图加载并排序成功，共 {Frames.Count} 张");
        }
        else
        {
            // 帧图不存在（V1 回滚场景），跳过，lua 里会自动 fallback
            Debug.LogWarning($"[AAHotfixLoader] ② 帧图不存在或加载失败（Status={spritesHandle.Status}），跳过帧图注入，回退为非动画状态");
        }

        // 3. 创建 LuaEnv
        LuaEnv = new LuaEnv();
        Debug.Log("[AAHotfixLoader] ③ LuaEnv 创建成功");

        // 4. 加载远程 Lua 热更脚本（TextAsset）
        var luaHandle = Addressables.LoadAssetAsync<TextAsset>(HOTFIX_LUA_ADDRESS);
        yield return luaHandle;
        if (luaHandle.Status != AsyncOperationStatus.Succeeded || luaHandle.Result == null)
        {
            Debug.LogError($"[AAHotfixLoader] ④ Lua 脚本加载失败！Address={HOTFIX_LUA_ADDRESS} Status={luaHandle.Status}");
            LoadSucceeded = false;
            yield break;
        }
        string luaContent = luaHandle.Result.text;
        Debug.Log("[AAHotfixLoader] ④ Lua 脚本下载成功，大小 " + luaContent.Length + " 字符");

        // 5. 把帧图注入 Lua 全局变量
        LuaEnv.Global.Set("frames", Frames);
        LuaEnv.Global.Set("frame_count", Frames.Count);

        // 6. 执行热更脚本（内部会调用 xlua.hotfix 替换 C# 方法）
        try
        {
            LuaEnv.DoString(luaContent);
            Debug.Log("[AAHotfixLoader] ✅ Lua hotfix 执行成功！koishi 的 Start/Update 已被 Lua 接管");
            LoadSucceeded = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AAHotfixLoader] ❌ Lua hotfix 执行异常: {e.Message}\n{e.StackTrace}");
            LoadSucceeded = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 魔理沙的七曜攻击之~
/// 超级炫酷的七曜运动
/// 话说七曜有8个，是因为计算机里的索引从0开始
/// 这边绑定在“七曜主体”上，只管理七曜珠子的初始动画和环绕特效
/// 七曜珠会先旋转排列，这将耗费几秒（期间无法攻击，算作是蓄力起手阶段）
/// 旋转完毕后，生成珠子间连线，并保持缓慢继续转动
/// 此时开始特殊攻击
/// </summary>
public class MagicAnime : MonoBehaviour
{
    public List<GameObject> MagicBalls = new();// 这里是七曜贤者之石仓库
    public Animator animator;// 七曜主体的动画组件
    public GunAnime gunAnime;// 武器动画组件

    private bool isExiting = false; // 是否正在退出

    // 连线材质（可在Inspector面板赋值）
    public Material lineMaterial;
    // 连线宽度
    public float lineWidth = 0.1f;
    // 存储连线组和对应的球体索引对
    private readonly Dictionary<LineRenderer, (int startIdx, int endIdx)> linePairs = new();
    // 定义需要连线的球体索引组合
    private readonly (int, int)[] connectPairs = new (int, int)[]
    {
        (0,2), (2,4), (4,6), (6,0),
        (1,3), (3,5), (5,7), (7,1)
    };

    void OnEnable()
    {
        animator.SetBool("IsShift", true);
        isExiting = false;
        // 初始化所有连线
        InitLines();
        Global_GameManager.Instance.OnReincarnation += CancelMagic;
    }
    void OnDisable()
    {
        Global_GameManager.Instance.OnReincarnation -= CancelMagic;
    }
    
    void Update()
    {
        if(Global_GameManager.Instance.state != State.Gaming && 
        Global_GameManager.Instance.state != State.NoDead) return;
        if (!isExiting && Input.GetKeyUp(KeyCode.LeftShift))
        {
            CancelMagic(Global_GameManager.Instance.state);
        }
        // 实时更新所有连线位置
        UpdateLinePositions();
    }

    private void CancelMagic(State state)
    {
        ClearLines();
        StartCoroutine(ExitMagic());
    }
    
    private IEnumerator ExitMagic()
    {
        isExiting = true;
        animator.SetBool("IsShift", false);
        yield return new WaitForSeconds(1f);

        // 调用 GunAnime 中的方法切换到魔理沙常态
        if (gunAnime != null)
        {
            gunAnime.SwitchToMarisaNormal();
        }
        isExiting = false;
        // 退出后清理所有连线
        ClearLines();
    }

    /// <summary>
    /// 初始化所有连线
    /// </summary>
    private void InitLines()
    {
        // 先清理旧连线
        ClearLines();
        // 为每个索引对创建连线
        foreach (var pair in connectPairs)
        {
            CreateLine(pair.Item1, pair.Item2);
        }
    }

    /// <summary>
    /// 清理所有连线
    /// </summary>
    private void ClearLines()
    {
        foreach (var kvp in linePairs)
        {
            if (kvp.Key != null)
            {
                Destroy(kvp.Key.gameObject);
            }
        }
        linePairs.Clear();
    }

    /// <summary>
    /// 创建单条连线
    /// </summary>
    /// <param name="startIdx">起始球体索引</param>
    /// <param name="endIdx">结束球体索引</param>
    private void CreateLine(int startIdx, int endIdx)
    {
        // 创建空物体承载LineRenderer
        GameObject lineObj = new ($"Line_{startIdx+1}_to_{endIdx+1}");
        lineObj.transform.SetParent(transform); // 设为子物体方便管理

        // 添加LineRenderer组件
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial ? lineMaterial : new Material(Shader.Find("Unlit/Color"));
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.positionCount = 2; // 连线只需要两个点
        lineRenderer.useWorldSpace = true; // 使用世界坐标

        lineRenderer.sortingLayerName = "PanDing"; // 改为你的前景层名称（如UI、Player、Effect）
        lineRenderer.sortingOrder = 5; // 数值越大越上层（确保大于背景的Order）

        // 存储连线和对应的索引
        linePairs.Add(lineRenderer, (startIdx, endIdx));
    }

        /// <summary>
    /// 实时更新所有连线的位置
    /// </summary>
    private void UpdateLinePositions()
    {
        foreach (var kvp in linePairs)
        {
            LineRenderer line = kvp.Key;
            int startIdx = kvp.Value.startIdx;
            int endIdx = kvp.Value.endIdx;

            // 检查索引是否有效
            if (startIdx >= 0 && startIdx < MagicBalls.Count && endIdx >= 0 && endIdx < MagicBalls.Count)
            {
                GameObject startBall = MagicBalls[startIdx];
                GameObject endBall = MagicBalls[endIdx];

                // 更新连线的两个端点为球体中心位置
                if (startBall != null && endBall != null)
                {
                    line.SetPosition(0, startBall.transform.position);
                    line.SetPosition(1, endBall.transform.position);
                }
            }
        }
    }
}

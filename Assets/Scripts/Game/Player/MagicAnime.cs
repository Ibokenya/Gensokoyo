using System.Collections;
using System.Collections.Generic;
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
    
}

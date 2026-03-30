using System.Collections;
using UnityEngine;

public class AboutItem : MonoBehaviour
{
    [Header("道具种类:0:HP+,1:HP,2:Power+,3:Power,4:Bomb+,5:Bomb,6:Grade,7:Grade-,8:Grade--")]
    [SerializeField]
    private int itemType;
    
    [Header("收集速度")]
    public float collectSpeed = 10f;
    
    private Rigidbody2D rb2D;
    private bool isCollecting = false;

    void OnEnable()
    {
        // 获取刚体组件
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("Rigidbody2D component not found on Item object!");
        }
    }

    void Update()
    {
        CheckPos();
    }
    private void CheckPos()
    {
        // 检查道具是否在边界内
        if (transform.position.y < -8f)
        {
            // 道具超出边界，销毁道具
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 碰撞器事件代表进入玩家收集范围
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(transform.name +"碰撞到"+collision.name);
        if (!isCollecting)
        {
            FlyToPlayer(collision.transform);
        }
    }
    /// <summary>
    /// 道具自动被玩家收取
    /// </summary>
    private void FlyToPlayer(Transform playerTransform)
    {
        isCollecting = true;
        StartCoroutine(CollectItem(playerTransform));
    }
    
    /// <summary>
    /// 收集道具的协程
    /// </summary>
    private IEnumerator CollectItem(Transform playerTransform)
    {
        // 禁用重力，使道具不受重力影响
        if (rb2D != null)
        {
            rb2D.gravityScale = 0f;
            rb2D.velocity = Vector2.zero;
        }
        
        // 目标位置为玩家当前位置
        Vector3 targetPosition = playerTransform.position;
        
        // 持续移动直到到达玩家位置
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            // 计算移动方向
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            // 移动道具
            transform.position += collectSpeed * Time.deltaTime * direction;
            
            yield return null;
        }
        
        // 道具到达玩家位置，触发效果
        Effect();
        
        // 销毁道具
        Global_ObjectPool.Instance.Recycle(this.gameObject);
    }
    private void Effect()
    {
        switch(itemType)
        {
            case 0:// 道具类型为HP+
                Global_GameManager.Instance.AddLeftLife(1,0);
                break;
            case 1:// 道具类型为HP
                Global_GameManager.Instance.AddLeftLife(0,1);
                break;
            case 2:// 道具类型为Power+
                Global_GameManager.Instance.AddPower(100);
                break;
            case 3:// 道具类型为Power
                Global_GameManager.Instance.AddPower(1);
                break;
            case 4:// 道具类型为Bomb+
                Global_GameManager.Instance.AddBomb(1,0);
                break;
            case 5:// 道具类型为Bomb
                Global_GameManager.Instance.AddBomb(0,1);
                break;
            case 6:// 道具类型为Grade
                Global_GameManager.Instance.AddGrade(1);
                break;
            case 7:// 道具类型为Grade-
                Global_GameManager.Instance.AddScore(100);
                break;
            case 8:// 道具类型为Grade--
                Global_GameManager.Instance.AddScore(10);
                break;
        }
    }
}

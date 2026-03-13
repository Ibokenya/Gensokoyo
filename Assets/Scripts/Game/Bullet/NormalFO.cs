using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一般飞行物（Normal Flying Object）
/// 啊，这就是NFO!
/// </summary>
public class NormalFO : MonoBehaviour
{
    public float speed;
    private float minX = -8.4f;
    private float maxX = 3f;
    private float minY = -5.3f;
    private float maxY = 4.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move(speed);
        MoveCheck();
    }

    /// <summary>
    /// 普通飞行物的移动
    /// </summary>
    /// <param name="speed">移动速度</param>
    public void Move(float speed)
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    public void MoveCheck()
    {
        if(transform.position.x<minX || transform.position.x>maxX || transform.position.y<minY || transform.position.y>maxY)
        {
            Destroy(this.gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : MonoBehaviour
{
    public GameObject myself;
    public float ScaleSpeed = 0.02f;
    public float Scale_a = -0.00005f;// 加速度，缩放的速度应该越来越慢才对
    private float TrueSpeed;

    private void Start()
    {
        TrueSpeed = ScaleSpeed;
}
    // Update is called once per frame
    void Update()
    {
        myself.transform.localScale += new Vector3(TrueSpeed, TrueSpeed);
        TrueSpeed += Scale_a;
        if(myself.transform.localScale.x>=4)
        {
            myself.transform.transform.localScale = new Vector3(0,0);
            TrueSpeed = ScaleSpeed;
        }
    }
}

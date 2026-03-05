using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleAndSize : MonoBehaviour
{
    public GameObject myself;
    public float circleSpeed = 100f;
    public float sizeSpeed = 0.01f;
    // Update is called once per frame
    void Update()
    {
        myself.transform.Rotate(0, 0, circleSpeed * Time.deltaTime);
        myself.transform.localScale += new Vector3(sizeSpeed, sizeSpeed);
        if(myself.transform.localScale.x >= 4f)
        {
            myself.transform.localScale = new Vector3(1,1);
        } 
    }
}

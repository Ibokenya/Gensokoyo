using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class circle_BySelf : MonoBehaviour
{
    public GameObject myself;
    public float circleSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        myself.transform.Rotate(0, 0, circleSpeed * Time.deltaTime);
    }
}

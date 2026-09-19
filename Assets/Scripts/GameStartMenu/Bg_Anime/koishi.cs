using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;



[Hotfix]
public class koishi : MonoBehaviour
{
    public float RotationSpeed = 100f;

    void Start()
    {
    }

    void Update()
    {
        this.transform.Rotate(0, 0, RotationSpeed * Time.deltaTime);
    }
}

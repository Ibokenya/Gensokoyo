using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transparent : MonoBehaviour
{
    public GameObject myself;
    public float TransSpeed = 0.001f;// 按照1->255来计算，最大值是1
    private bool isAdd = false;
    private Color InitColor;

    private void Start()
    {
        InitColor = myself.GetComponent<Image>().color;
    }
    // Update is called once per frame
    void Update()
    {
        if(!isAdd)
        {
            myself.GetComponent<Image>().color -= new Color(0, 0, 0, TransSpeed);
            if(myself.GetComponent<Image>().color.a<=0)
            {
                isAdd = true;
            }
        }
        else
        {
            myself.GetComponent<Image>().color += new Color(0, 0, 0, TransSpeed);
            if (myself.GetComponent<Image>().color.a >= InitColor.a)
            {
                isAdd = false;
            }
        }
    }
}

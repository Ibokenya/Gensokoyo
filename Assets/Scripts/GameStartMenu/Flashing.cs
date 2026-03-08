using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Flashing : MonoBehaviour
{
    public GameObject targetObject;
    private TextMeshProUGUI text;

    private bool isAdd = false;         
    public float minTransparent = 0.2f; // 最小透明度
    public float maxTransparent = 1f;   // 最大透明度
    public float delta = 0.005f;        // 每帧透明度变化值

    // Start is called before the first frame update
    void Start()
    {
        text = targetObject.GetComponent<TextMeshProUGUI>();

        // 初始化文字颜色
        Color initColor = text.color;
        initColor.a = maxTransparent;
        text.color = initColor;
    }

    // Update is called once per frame
    void Update()
    {
        flashing();
    }

    private void flashing()
    {
        Color currentColor = text.color;

        if (isAdd)
        {
            currentColor.a += delta;
            if (currentColor.a >= maxTransparent)
            {
                currentColor.a = maxTransparent; // 修正到最大值，避免溢出
                isAdd = false;
            }
        }
        else
        {
            currentColor.a -= delta;
            if (currentColor.a <= minTransparent)
            {
                currentColor.a = minTransparent; // 修正到最小值，避免溢出
                isAdd = true;
            }
        }
        text.color = currentColor;
    }
}

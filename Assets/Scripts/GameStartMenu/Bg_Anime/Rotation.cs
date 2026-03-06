using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public GameObject myself;
    public float RotationSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        myself.transform.Rotate(0, 0, RotationSpeed * Time.deltaTime);
    }
}

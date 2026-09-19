using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float RotationSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(0, 0, RotationSpeed * Time.deltaTime);
    }
}

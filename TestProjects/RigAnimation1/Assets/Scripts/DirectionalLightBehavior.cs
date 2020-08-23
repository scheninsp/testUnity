using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightBehavior : MonoBehaviour
{
    void FixedUpdate()
    {
        //transform.Rotate(Vector3.up, 5f, Space.World);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,
            transform.localEulerAngles.y + 1f,
            transform.localEulerAngles.z);
    }
}

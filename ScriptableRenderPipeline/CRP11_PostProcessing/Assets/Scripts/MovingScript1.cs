using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingScript1 : MonoBehaviour
{
    Vector3 centerPosition;
    public float Amplitude = 1;
    public float Frequency = 1;

    private void Awake()
    {
        centerPosition = transform.localPosition;
        Debug.Log("centerPosition : " + centerPosition);
    }

    void FixedUpdate()
    {
        Vector3 position = new Vector3(Amplitude * Mathf.Sin(Mathf.PI * Frequency * Time.time), 0, 0);
        transform.localPosition = centerPosition + position;
    }
}

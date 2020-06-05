using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotation : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }

    public Camera cam;
    void Update()
    {
        Vector3 fwd = cam.transform.forward;
        fwd.Normalize();
        if (Input.GetMouseButton(0))
        {
            Vector3 vaxis = Vector3.Cross(fwd, Vector3.right);
            transform.Rotate(vaxis, -Input.GetAxis("Mouse X"), Space.World);
            Vector3 haxis = Vector3.Cross(fwd, Vector3.up);
            transform.Rotate(haxis, -Input.GetAxis("Mouse Y"), Space.World);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxAcitivity_1 : MonoBehaviour
{
    private float moveSpeed = 1f;

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        //transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.R))
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
    }
}

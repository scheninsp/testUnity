using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Transform player;

    private Vector3 offset;

    void Start()
    {
        offset = player.position - this.transform.position;
    }

    void Update()
    {
        this.transform.position = player.position - offset;
    }
}

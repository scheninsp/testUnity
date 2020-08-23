using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSimple : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, 1f);
    }
}

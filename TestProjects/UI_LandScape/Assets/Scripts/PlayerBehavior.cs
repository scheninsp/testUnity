using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{

    public Transform playerTransform;

    private float speed = 0.05f;
    

    public void playerMove(Vector2 vec)
    {
        Vector3 translateVector = new Vector3(vec.x, 0, vec.y);
        playerTransform.Translate(translateVector * speed);
    }
}

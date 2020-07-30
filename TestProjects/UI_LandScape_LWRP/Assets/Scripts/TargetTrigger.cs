using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Attacker")
        {
            Vector3 normalizedDirection = this.transform.position -
                    other.gameObject.transform.parent.transform.position;
            normalizedDirection.Normalize();

            this.transform.parent.GetComponentInParent<ShapeController>().DestroyShape(normalizedDirection);
        }
    }
}

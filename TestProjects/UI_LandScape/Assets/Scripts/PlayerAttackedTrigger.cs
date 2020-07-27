using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackedTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "TargetAttacker")
        {
            this.transform.parent.GetComponent<PlayerBehavior>().Attacked();
        }
    }

}

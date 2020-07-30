using System;
using System.Collections;
using UnityEngine;

public class AttackerBehavior : MonoBehaviour
{
    public Transform playerTransform;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 rotateAxis = new Vector3(1, -1, 0);
    private float rotateSpeed = 600;

    DateTime timer = new DateTime();

    public GameObject AttackerEffect;

    private bool attackingState = false;

    void Awake()
    {
        originalLocalPosition = GetComponent<Transform>().localPosition;
        originalLocalRotation = GetComponent<Transform>().localRotation;
        gameObject.SetActive(false);
    }

    public void ActiveAttacker()
    {
        if(attackingState == false)
        {
            this.transform.localPosition = originalLocalPosition;
            this.transform.localRotation = originalLocalRotation;
            timer = DateTime.Now;
            gameObject.SetActive(true);
            attackingState = true;
            StartCoroutine(Attack1());
        }
    }

    private void Update()
    {
    }

    IEnumerator Attack1()
    {
        while (attackingState == true)
        {
            TimeSpan dur1 = DateTime.Now.Subtract(timer);
            if (dur1.TotalSeconds > 0.15f)
            {
                attackingState = false;
                gameObject.SetActive(false);
                AttackerEffect.transform.GetChild(0).GetComponent<ParticleSystem>().Clear();

                yield break;
            }

            this.transform.RotateAround(playerTransform.position,
            playerTransform.localToWorldMatrix * rotateAxis, Time.deltaTime * rotateSpeed);
            yield return null;
        }
    }  

}

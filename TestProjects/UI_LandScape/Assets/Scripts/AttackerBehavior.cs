using System;
using UnityEngine;

public class AttackerBehavior : MonoBehaviour
{
    public Transform playerTransform;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 rotateAxis = new Vector3(1, -1, 0);
    private float rotateSpeed = 1000;

    DateTime timer = new DateTime();

    void Awake()
    {
        originalLocalPosition = GetComponent<Transform>().localPosition;
        originalLocalRotation = GetComponent<Transform>().localRotation;
        gameObject.SetActive(false);
    }

    public void ActiveAttacker()
    {
        this.transform.localPosition = originalLocalPosition;
        this.transform.localRotation = originalLocalRotation;
        timer = DateTime.Now;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        TimeSpan dur1 = DateTime.Now.Subtract(timer);

        this.transform.RotateAround(playerTransform.position,
        rotateAxis, Time.deltaTime * rotateSpeed);
        if(dur1.TotalSeconds > 0.15f)
        {
            gameObject.SetActive(false);
        }
    }
}

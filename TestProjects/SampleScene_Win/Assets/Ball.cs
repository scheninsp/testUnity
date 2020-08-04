using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    public float m_movSpeed = 0.2f;
    public float rotateSpeed = 10;

    public Transform targetTransform;
    public Transform playerTransform;

    Vector3 currentFowardPointer;

    private void Start()
    {
        currentFowardPointer = new Vector3(0, 0, 1);
        this.transform.GetChild(1).gameObject.SetActive(false);
    }

    private void Update()
    {
        Control();
    }

    void Control()
    {

        float xm = 0, ym = 0, zm = 0;

        if (Input.GetKey(KeyCode.W))
        {
            // zm += m_movSpeed * Time.deltaTime;
            zm += m_movSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            // zm -= m_movSpeed * Time.deltaTime;
            zm -= m_movSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // xm -= m_movSpeed * Time.deltaTime;
            xm -= m_movSpeed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // xm += m_movSpeed * Time.deltaTime;
            xm += m_movSpeed;
        }
        this.transform.Translate(new Vector3(xm, ym, zm));

        /*
        float rd = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            rd -= rotateSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rd += rotateSpeed * Time.deltaTime;
        }
        Quaternion rotateQuarternion = Quaternion.Euler(0, rd, 0);
        this.transform.rotation = rotateQuarternion * this.transform.rotation;
        Vector3 eulerResult = this.transform.rotation.eulerAngles;
        */

        playerRotateTo(targetTransform.position);

    }

    public Quaternion playerRotateTo(Vector3 lookAtPosition)
    {
        Quaternion rotateQuater = Quaternion.identity;

        lookAtPosition.y = playerTransform.position.y;
        Vector3 tolookAtPosition = lookAtPosition - playerTransform.position;

        float rotateAngle = MyMath.AngleSigned(currentFowardPointer, tolookAtPosition, Vector3.up);

        rotateQuater = Quaternion.AngleAxis(rotateAngle, Vector3.up);
        playerTransform.localRotation = rotateQuater * playerTransform.localRotation;

        currentFowardPointer = rotateQuater * currentFowardPointer;

        return rotateQuater;

    }
}
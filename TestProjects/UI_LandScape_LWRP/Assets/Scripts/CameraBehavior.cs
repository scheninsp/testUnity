using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Transform playerTransform;

    private Vector3 offset;
    private Vector3 InitialOffset;
    private Quaternion initialCameraRotation;

    //state 0 : follow character
    //state 1 : lock target
    //state 2 : manually rotating
    public int state = 0;

    public class rotateSpeed
    {
        public static float rotateManualSpeed = 0.005f;
        public static float rotateToTargetSpeed = 0.5f;
    }

    private float rotateAngleXAxis;
    private float rotateAngleYAxis;

    private Transform targetTransform;

    void Start()
    {
        InitialOffset = playerTransform.position - this.transform.position;
        offset = InitialOffset;
        initialCameraRotation = this.transform.rotation;
    }

    void Update()
    {
        if(state == 1)
        {
            Vector3 offsetWithConstantRotation = playerTransform.rotation * InitialOffset;
            this.transform.position = playerTransform.position - offsetWithConstantRotation;
        }
        else
        {
            this.transform.position = playerTransform.position - offset;
        }


        if (state == 1)
        {
            rotateToTarget();
        }

        if (state == 2)
        {
            //YAxis rotation
            this.transform.RotateAround(playerTransform.position, Vector3.up, rotateAngleYAxis * rotateSpeed.rotateManualSpeed);


            //bound XAxis rotation

            float xAngle = MyMath.TransformAngle(this.transform.rotation.eulerAngles.x - rotateAngleXAxis * rotateSpeed.rotateManualSpeed);

            //this.transform.RotateAround(playerTransform.position, -this.transform.right, rotateAngleXAxis * rotateSpeed.rotateManualSpeed);
            //float xAngle2 = MyMath.TransformAngle(this.transform.rotation.eulerAngles.x);  

            float xLowerLimit = -5f;
            float xUpperLimit = 30f;

            if (xAngle < xLowerLimit)
            {
                float correctedRotateAngle = xLowerLimit - this.transform.rotation.eulerAngles.x;
                this.transform.RotateAround(playerTransform.position, -this.transform.right, correctedRotateAngle);
            }
            else if (xAngle > xUpperLimit)
            {
                float correctedRotateAngle = xUpperLimit - this.transform.rotation.eulerAngles.x;
                this.transform.RotateAround(playerTransform.position, -this.transform.right, correctedRotateAngle);
            }
            else
            {
                this.transform.RotateAround(playerTransform.position, -this.transform.right, rotateAngleXAxis * rotateSpeed.rotateManualSpeed);
            }


            //update offset after rotate around
            offset = playerTransform.position - this.transform.position;

        }
    }

    public void startRotate(Vector2 dir)
    {
        if(state != 1)
        {
            state = 2;

            rotateAngleYAxis = 0f;
            rotateAngleXAxis = 0f;

            if (dir.x * dir.x > dir.y * dir.y)
            {
                if (dir.x < 0)
                {
                    rotateAngleYAxis = -90f;
                }
                else
                {
                    rotateAngleYAxis = 90f;
                }
            }
            else
            {
                if (dir.y < 0)
                {
                    rotateAngleXAxis = -90f;
                }
                else
                {
                    rotateAngleXAxis = 90f;
                }
            }

        }
    }

    public void stopRotate()
    {
        state = 0;
    }

    private void rotateToTarget()
    {
        Vector3 tolookAtPosition = targetTransform.position - this.transform.position;
        tolookAtPosition.y = 0;

        Vector3 currentFowardPointer = new Vector3(0, 0, 1);
        float fowardAxisRotationInY = this.transform.rotation.eulerAngles.y;
        Quaternion rotateFowardPointerInY = Quaternion.Euler(0, fowardAxisRotationInY, 0);
        currentFowardPointer = rotateFowardPointerInY * currentFowardPointer;

        float rotateAngle = MyMath.AngleSigned(currentFowardPointer, tolookAtPosition, Vector3.up);

        Quaternion rotateQuater = Quaternion.AngleAxis(rotateAngle * rotateSpeed.rotateToTargetSpeed, 
                                                            Vector3.up);
        this.transform.localRotation = rotateQuater * this.transform.localRotation;
    }


    public void lockTarget(Transform trans)
    {
        state = 1;
        targetTransform = trans;

        //incase the camera has already manually rotated
        offset = InitialOffset;
        this.transform.rotation = initialCameraRotation;
    }

    public void unlockTarget()
    {
        offset = playerTransform.position - this.transform.position;
        state = 0;
    }
}

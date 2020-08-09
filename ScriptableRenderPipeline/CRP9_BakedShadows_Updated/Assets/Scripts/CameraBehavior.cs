using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{

    public Transform playerTransform;
    public Transform targetTransform;

    Vector3 offset;

    public class rotateSpeed
    {
        public static float rotateManualSpeed = 0.005f;
        public static float rotateToTargetSpeed = 0.02f;
    }

    void Start()
    {
        offset = playerTransform.position - this.transform.position;
    }

    void Update()
    {
        Vector3 offsetWithConstantRotation = playerTransform.rotation * offset;

        Vector3 playerFowardDirection = playerTransform.rotation * Vector3.forward;

        this.transform.position = playerTransform.position - offsetWithConstantRotation;
        Vector3 tmp = this.transform.position - playerTransform.position;
        float tmpmag = tmp.magnitude;

        rotateToTarget();
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
}

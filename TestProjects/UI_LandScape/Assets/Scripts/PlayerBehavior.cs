using System;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    //state 1 : move
    //state 2 : dash
    private int stateFlag = 1;

    public Transform playerTransform;

    public class speedSettings
    {
        public static float moveSpeed = 0.1f;
        public static float dashSpeed = 0.2f;
    }
    float speed = speedSettings.moveSpeed;

    DateTime timer = new DateTime();

    float dashDuration = 0.2f;
    float dashCoolDown = 0.5f;
    float dashCoolDownTimer = -1f;

    Vector3 currentFowardPointer;

    bool lockState = false;
    Shape lockedTarget = null;

    private void Awake()
    {
        currentFowardPointer = new Vector3(0,0,1);
    }

    private void Update()
    {
        //return to state 1
        TimeSpan dur1 = DateTime.Now.Subtract(timer);
        if (stateFlag == 2 && dur1.TotalSeconds > dashDuration)
        {
            stateFlag = 1;
            speed = speedSettings.moveSpeed;
        }

        //Skill cooldown
        if (dashCoolDownTimer > 0)
        {
            dashCoolDownTimer -= Time.deltaTime;
        }

        //Locked State
        if(lockState == true)
        {
            this.playerRotateTo(lockedTarget.transform.position);
        }

    }

    public void playerMove(Vector2 vec)
    {
        Vector3 translateVector = new Vector3(vec.x, 0, vec.y);
        translateVector = playerTransform.worldToLocalMatrix * translateVector;
        playerTransform.Translate(translateVector * speed);
    }

    public void playerDash()
    {
        if(stateFlag != 2 && dashCoolDownTimer<=0)
        {
            dashCoolDownTimer = dashCoolDown;
            stateFlag = 2;
            timer = DateTime.Now;
            this.speed = speedSettings.dashSpeed;
        }

    }

    public Quaternion playerRotateTo(Vector3 lookAtPosition)
    {
        lookAtPosition.y = playerTransform.position.y;
        Vector3 tolookAtPosition = lookAtPosition - playerTransform.position;

        float rotateAngle = MyMath.AngleSigned(currentFowardPointer, tolookAtPosition, Vector3.up);

        Quaternion rotateQuater = Quaternion.AngleAxis(rotateAngle, Vector3.up);
        playerTransform.rotation = rotateQuater * playerTransform.rotation;

        currentFowardPointer = rotateQuater * currentFowardPointer;

        return rotateQuater;
    }

    public void lockTarget(Shape target)
    {
        lockedTarget = target;
        lockState = true;
    }

    public void unlockTarget()
    {
        lockState = false;
        lockedTarget = null;
    }

}

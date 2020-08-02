using System;
using System.Collections;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    //state 1 : move
    //state 2 : dash
    private int stateFlag = 1;

    public class speedSettings
    {
        public static float moveSpeed = 4f;
        public static float dashSpeed = 8f;
    }
    float speed = speedSettings.moveSpeed;

    DateTime timer = new DateTime();

    const float dashDuration = 0.2f;
    const float dashCoolDown = 0.5f;
    float dashCoolDownTimer = -1f;

    [HideInInspector]
    public Vector3 currentFowardPointer;

    bool lockState = false;
    Shape lockedTarget = null;

    //state 0 : normal
    //state 1 : attacked
    [HideInInspector]
    public int state = 0;

    const float shakeXposMax = 0.1f;
    const int repeatTimes = 2;
    const float attackedCantMoveDuration = 0.5f;

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

    public void playerMove(Vector3 vec)
    { //vec : world space moving direction
        if(state != 1)
        {
            Vector3 translateVector = vec;

            if (lockState == false)
            {
                //turn head
                float rotateAngle = MyMath.AngleSigned(currentFowardPointer, translateVector, Vector3.up);

                Quaternion rotateQuater = Quaternion.AngleAxis(rotateAngle, Vector3.up);
                this.transform.localRotation = rotateQuater * this.transform.localRotation;

                currentFowardPointer = rotateQuater * currentFowardPointer;
            }

            //move
            translateVector = this.transform.worldToLocalMatrix * translateVector;
            translateVector.Normalize();
            this.transform.Translate(translateVector * speed * Time.deltaTime);
        }

    }

    public void playerDash()
    {
        if (state != 1)
        {
            if (stateFlag != 2 && dashCoolDownTimer <= 0)
            {
                dashCoolDownTimer = dashCoolDown;
                stateFlag = 2;
                timer = DateTime.Now;
                this.speed = speedSettings.dashSpeed;
            }
        }
    }

    public Quaternion playerRotateTo(Vector3 lookAtPosition)
    {
        Quaternion rotateQuater = Quaternion.identity;

        if (state != 1)
        {
            lookAtPosition.y = this.transform.position.y;
            Vector3 tolookAtPosition = lookAtPosition - this.transform.position;

            float rotateAngle = MyMath.AngleSigned(currentFowardPointer, tolookAtPosition, Vector3.up);

            rotateQuater = Quaternion.AngleAxis(rotateAngle, Vector3.up);
            this.transform.localRotation = rotateQuater * this.transform.localRotation;

            currentFowardPointer = rotateQuater * currentFowardPointer;
        }
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

    public void Attacked()
    {
        if (state == 0)
        {
            StartCoroutine(Attacked1());
        }
    }

    private IEnumerator Attacked1()
    {
        DateTime timer1 = new DateTime();
        DateTime timer2 = new DateTime();
        TimeSpan dur1 = new TimeSpan();

        float singleRoundDuration = attackedCantMoveDuration/ repeatTimes;

        if (state == 0)
        {
            timer1 = DateTime.Now;
            state = 1;
        }

        while(state == 1 && dur1.TotalSeconds < attackedCantMoveDuration)
        {
            timer2 = DateTime.Now;
            dur1 = timer2.Subtract(timer1);

            float currentLerpVal = MyMath.TriangleFunction((float)dur1.TotalSeconds, singleRoundDuration);

            float newx = Mathf.Lerp(-shakeXposMax, shakeXposMax, currentLerpVal);
            Vector3 newLocalPosition = new Vector3(this.transform.localPosition.x + newx, 
                this.transform.localPosition.y, this.transform.localPosition.z);

            this.transform.localPosition = newLocalPosition;

            yield return null;
        }

        state = 0;
        yield break;
    }

}

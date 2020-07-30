using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeController : MonoBehaviour
{
    //make a property
    public int ShapeId
    {
        get
        {
            return shapeId;
        }
        set
        {
            if (shapeId == int.MinValue && value != int.MinValue)
            {
                shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId");
            }
        }
    }
    int shapeId = int.MinValue;


    public int MaterialId
    {
        get;
        private set; //forbidden
    }
    int materialId = int.MinValue;

    //state 0 : both not activated
    //state 1 : shape activated, destructedShape deactivated
    //state 2 : shape deactivated, destructedShape activated
    public int state = 0;

    const int shapeChildIndex = 0;
    const int destructedShapeChildIndex = 1;

    private Transform playerTransform;
    public Vector3 currentFowardPointer;  //in world space

    public class speedSettings
    {
        public static float moveSpeed = 3f;
        public static float dashSpeed = 6f;
        public static float autoLeaveSpeed = 3f;
    }
    float speed = speedSettings.moveSpeed;

    const float rotateSpeed = 0.05f;

    const float minDistanceToPlayer = 5f;

    const float attack1CoolDown = 2f;
    float attackCoolDownTimer = -1f;

    //shapeNearbyState 0 : nothing nearby
    //shapeNearbyState 1 : nearby targets in same direction
    //shapeNearbyState 2 : nearby targets in different direction
    public int shapeNearbyState = 0;

    public Vector3 normalizedLeavingDirection;

    //movingState 0 : leaving nearby object
    public int movingState = 0;

    void Awake()
    {
        this.transform.GetChild(shapeChildIndex).gameObject.SetActive(true);
        this.transform.GetChild(destructedShapeChildIndex).gameObject.SetActive(false);
        state = 1;
        playerTransform = GameObject.Find("Player").transform;
        currentFowardPointer = new Vector3(0, 0, -1);  //initial face direction
    }

    void Update()
    {
        if(this.transform.GetChild(shapeChildIndex).gameObject.activeSelf == false &&
            this.transform.GetChild(destructedShapeChildIndex).gameObject.activeSelf == false)
        {
            state = 0;
        }

        if(state == 1)
        {
            Vector3 vecToPlayer = playerTransform.position - this.transform.position;
            vecToPlayer.y = 0;

            //Actions
            if (shapeNearbyState == 0 && (vecToPlayer.magnitude > minDistanceToPlayer))
            {
                AutoMove();
            }
            else if(shapeNearbyState == 1)
            {
                if(movingState == 0)
                {
                    StartCoroutine(startLeaving());
                }
            }
            else if(shapeNearbyState == 2)
            {
                //no moving
            }


            float angleToPlayer = Vector3.Angle(currentFowardPointer, vecToPlayer);
            if (angleToPlayer > 0.2f)
            {
                AutoRotate();
            }

            if (vecToPlayer.magnitude <= minDistanceToPlayer && angleToPlayer <= 0.2f && attackCoolDownTimer <= 0f)
            {
                Attack1();
                attackCoolDownTimer = attack1CoolDown;
            }

            //Timers
            if(attackCoolDownTimer > 0)
            {
                attackCoolDownTimer -= Time.deltaTime;
            }

        }

    }

    public void DestroyShape(Vector3 normalizedDirection)
    //normalizedDirection: from player to target
    {
        this.transform.GetChild(shapeChildIndex).gameObject.SetActive(false);
        this.transform.GetChild(destructedShapeChildIndex).gameObject.SetActive(true);
        this.transform.GetChild(destructedShapeChildIndex).GetComponent<DestructedShape>().Explode(normalizedDirection);
        state = 2;
    }

    public void SetMaterial(Material material, int materialId)
    {
        this.transform.GetChild(shapeChildIndex).GetComponent<Shape>().SetMaterial(material);
        this.transform.GetChild(destructedShapeChildIndex).GetComponent<DestructedShape>().SetMaterial(material);
    }

    public void Reclaim()
    {
        this.transform.GetChild(shapeChildIndex).GetComponent<Shape>().Reclaim();
        this.transform.GetChild(destructedShapeChildIndex).GetComponent<DestructedShape>().Reclaim();

        this.transform.GetChild(shapeChildIndex).gameObject.SetActive(true);
        this.transform.GetChild(destructedShapeChildIndex).gameObject.SetActive(false);
        state = 1;
        shapeNearbyState = 0;
        movingState = 0;
    }

    public void Attack1()
    {
        if(this.transform.GetChild(shapeChildIndex).gameObject.activeSelf == true &&
            this.transform.GetChild(destructedShapeChildIndex).gameObject.activeSelf == false)
        {
            this.transform.GetChild(shapeChildIndex).gameObject.GetComponent<Shape>().StartAttack1();
        }
    }

    public void AutoMove()
    {

        if (this.transform.GetChild(shapeChildIndex).GetComponent<Shape>().state == 0)
        {
            Vector3 translateVector = playerTransform.position - this.transform.position;
            translateVector.y = 0;
            translateVector.Normalize();

            //move
            translateVector = this.transform.worldToLocalMatrix * translateVector;
            this.transform.Translate(translateVector * speed * Time.deltaTime);
        }

    }

    public void AutoRotate()
    {
        if (this.transform.GetChild(shapeChildIndex).GetComponent<Shape>().state == 0)
        {
            Vector3 translateVector = playerTransform.position - this.transform.position;
            translateVector.y = 0;

            //turn head
            float rotateAngle = MyMath.AngleSigned(currentFowardPointer, translateVector, Vector3.up);

            Quaternion rotateQuater = Quaternion.AngleAxis(rotateAngle * rotateSpeed, Vector3.up);
            this.transform.localRotation = rotateQuater * this.transform.localRotation;

            currentFowardPointer = rotateQuater * currentFowardPointer;

        }
    }

    public void AutoLeaveNearest(Vector3 noramlizedLeavingDir)
    {
        if (this.transform.GetChild(shapeChildIndex).GetComponent<Shape>().state == 0)
        {
            //add a small random direction
            //noramlizedLeavingDir.x += Random.Range(0, 0.01f);
            //noramlizedLeavingDir.z += Random.Range(0, 0.01f);

            //noramlizedLeavingDir.Normalize();

            //leave nearest
            noramlizedLeavingDir = this.transform.worldToLocalMatrix * noramlizedLeavingDir;
            this.transform.Translate(noramlizedLeavingDir * speedSettings.autoLeaveSpeed * Time.deltaTime);
        }
    }

    private IEnumerator startLeaving()
    {
        movingState = 1;
        yield return new WaitForSeconds(0.2f);
        while (shapeNearbyState == 1)
        {
            AutoLeaveNearest(normalizedLeavingDirection);
            yield return null;
        }
        movingState = 0;
        yield break;
    }
}

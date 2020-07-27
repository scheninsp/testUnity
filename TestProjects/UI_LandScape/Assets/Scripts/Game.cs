using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{

    public TargetControllerFactory targetFactory;

    List<ShapeController> shapeControllers;

    private const float headLevelPosition = 0.8f;
    private const float footLevelPositionTarget = 0.3f;

    public Transform playerTransform;
    Transform initialPlayerTransform = null;

    public PlayerBehavior playerBehavior;

    public PassiveLayerBehavior passiveLayerBehavior;

    int playerLockedTargetIndex = -1;

    void Awake()
    {
        shapeControllers = new List<ShapeController>();
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        BeginNewGame();
    }

    private void Update()
    {
        if(shapeControllers.Count == 0 || shapeControllers[playerLockedTargetIndex].state != 1)
        {
            passiveLayerBehavior.removeLockImage();
            playerBehavior.unlockTarget();
        }

        //reclaim state 0 shapeControllers
        for(int i=0; i<shapeControllers.Count; i++)
        {
            if(shapeControllers[i].state == 0)
            {
                targetFactory.Reclaim(shapeControllers[i]);
                shapeControllers.RemoveAt(i);
            }
        }

    }

    void BeginNewGame()
    {
        //if begin from a current state, clear current state
        for (int i = 0; i < shapeControllers.Count; i++)
        {
            targetFactory.Reclaim(shapeControllers[i]);
        }
        shapeControllers.Clear();

        if(initialPlayerTransform == null)
        {
            initialPlayerTransform = playerTransform;
        }
        else
        {
            //reset player to origin
            playerTransform.position = initialPlayerTransform.position;
            playerTransform.rotation = initialPlayerTransform.rotation;
        }
    }

    public void CreateTarget()
    {
        ShapeController o = targetFactory.GetRandom();
        
        //put targets under one layer
        o.gameObject.transform.SetParent(this.transform.GetChild(0).transform);
        o.gameObject.layer = this.transform.GetChild(0).gameObject.layer;
        foreach (Transform tran in o.gameObject.GetComponentsInChildren<Transform>(true))
        {
            tran.gameObject.layer = this.transform.GetChild(0).gameObject.layer;
        }

        Transform t = o.transform;

        t.localScale = Vector3.one * Random.Range(1f, 2f);
        t.localPosition = new Vector3(Random.Range(-10f, 10f), footLevelPositionTarget + t.localScale.y/2,
            Random.Range(1f, 10f));

        shapeControllers.Add(o);

    }

    //destroy last generated ShapeController
    public void DestroyTarget()
    {
        if (shapeControllers.Count > 0)
        {
            int index = shapeControllers.Count - 1;

            if(index == playerLockedTargetIndex)
            {
                passiveLayerBehavior.removeLockImage();
                playerBehavior.unlockTarget();
                playerLockedTargetIndex = -1;
            }

            targetFactory.Reclaim(shapeControllers[index]);
            shapeControllers.RemoveAt(index);
        }
    }

    public void LockTarget()
    {
        if (shapeControllers.Count > 0)
        {
            //calculate distance to all targets, select closest
            float dist_closest = 99999f;
            int index_closest = -1;
            for (int i = 0; i < shapeControllers.Count; i++)
            {
                if(shapeControllers[i].state == 1)  //activated as Shape
                {
                    Vector3 targetPosition = shapeControllers[i].transform.GetChild(0).position;
                    targetPosition = playerTransform.position - targetPosition;
                    float dist = targetPosition.magnitude;
                    if (dist < dist_closest)
                    {
                        dist_closest = dist;
                        index_closest = i;
                    }
                }

            }

            if(index_closest > -1)
            {
                Vector3 targetPositionFinal = shapeControllers[index_closest].GetComponent<Transform>().position;
                Quaternion rotateAngle = playerBehavior.playerRotateTo(targetPositionFinal);
                playerLockedTargetIndex = index_closest;

                Shape targetShape = shapeControllers[index_closest].transform.GetChild(0).gameObject.GetComponent<Shape>();
                playerBehavior.lockTarget(targetShape);

                passiveLayerBehavior.generateLockImage(targetShape);
            }

        }

    }

    public void TargetAttack()
    {
        if (shapeControllers.Count > 0 && shapeControllers[shapeControllers.Count-1].state == 1)
        {
            shapeControllers[shapeControllers.Count - 1].Attack1();
        }

    }
}

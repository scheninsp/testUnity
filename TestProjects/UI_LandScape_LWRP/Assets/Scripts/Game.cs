using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KdTree;

public class Game : MonoBehaviour
{

    public TargetControllerFactory targetFactory;

    List<ShapeController> shapeControllers;
    List<int> activeShapeIndexList = new List<int>();

    private const float headLevelPosition = 0.8f;
    private const float footLevelPositionTarget = 0.3f;

    public Transform playerTransform;
    Transform initialPlayerTransform = null;

    public PlayerBehavior playerBehavior;

    public PassiveLayerBehavior passiveLayerBehavior;

    int playerLockedTargetIndex = -1;

    public Camera mainCamera;

    const float minDistBetweenTargets = 3f;
    const float minDistStopLeaving = 4f;

    public static Game Instance { get; private set; }

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            shapeControllers = new List<ShapeController>();
            Application.targetFrameRate = 60;

            //keep object while switching between scenes
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        BeginNewGame();
    }

    void Update()
    {
        int tmp = 1;  //an entrance for debugger

        if (playerLockedTargetIndex >= 0 && 
            (shapeControllers.Count == 0 || shapeControllers[playerLockedTargetIndex].state != 1))
        {
            unlockTarget();
        }

        //reclaim state 0 shapeControllers
        int i1 = 0;
        int currentLastPos = shapeControllers.Count;
        while ( i1 < currentLastPos )
        {
            if(shapeControllers[i1].state == 0)
            {
                //Reclaim i
                targetFactory.Reclaim(shapeControllers[i1]);
                //set List[i] <- List[last]
                int lastIndex = currentLastPos - 1;

                if (i1 < lastIndex)
                {
                    shapeControllers[i1] = shapeControllers[lastIndex];
                    if (lastIndex == playerLockedTargetIndex)
                    {
                        playerLockedTargetIndex = i1;
                    }
                }
                //remove List[last]
                shapeControllers.RemoveAt(lastIndex);
                currentLastPos--;
            }
            else
            {
                i1++;
            }
        }

        activeShapeIndexList.Clear();
        for (int i = 0; i<shapeControllers.Count; i++)
        {
            if (shapeControllers[i].state == 1)
            {
                activeShapeIndexList.Add(i);
            }
        }

        if (activeShapeIndexList.Count == 1)
        {
            shapeControllers[activeShapeIndexList[0]].shapeNearbyState = 0;
        }

        if (activeShapeIndexList.Count == 2)
        {
            int k0 = activeShapeIndexList[0];
            int k1 = activeShapeIndexList[1];

            float Shape1_x = shapeControllers[k0].transform.position.x;
            float Shape1_z = shapeControllers[k0].transform.position.z;

            float Shape2_x = shapeControllers[k1].transform.position.x;
            float Shape2_z = shapeControllers[k1].transform.position.z;

            Vector2 xzPlaneDistVec = new Vector2(Shape2_x - Shape1_x,
                    Shape2_z - Shape1_z);

            if (xzPlaneDistVec.magnitude < minDistBetweenTargets)
            {

                shapeControllers[k1].shapeNearbyState = 1;
                Vector3 distVec = shapeControllers[k1].transform.position
                    - shapeControllers[k0].transform.position;
                distVec.y = 0;

                shapeControllers[k1].normalizedLeavingDirection = distVec.normalized;
            }
            else
            {
                shapeControllers[k1].shapeNearbyState = 0;
            }
        }


        if (activeShapeIndexList.Count > 2)
        {
            KdTree<float, int> kDTreeShapes = new KdTree<float, int>(2, new KdTree.Math.FloatMath());

            int maxNeighborNumber = 4;
            float maxNeighborRadius = minDistStopLeaving + 1f;

            //add positions to KDTrees
            for (int i = 0; i < activeShapeIndexList.Count; i++)
            {
                kDTreeShapes.Add(new[] { shapeControllers[activeShapeIndexList[i]].transform.position.x,
                shapeControllers[activeShapeIndexList[i]].transform.position.z }, i);
            }

            //search KDTree
            for (int i = 0; i < activeShapeIndexList.Count; i++)
            {
                float thisShape_x = shapeControllers[activeShapeIndexList[i]].transform.position.x;
                float thisShape_z = shapeControllers[activeShapeIndexList[i]].transform.position.z;
           
                var nodes = kDTreeShapes.RadialSearch(new[] {thisShape_x, thisShape_z }, 
                                                        maxNeighborRadius, maxNeighborNumber);

                int nVectors = nodes.Length - 1;

                if(nVectors > 0)
                {
                    Vector2[] xzPlaneDistVec = new Vector2[nVectors];
                    for (int j = 1; j < nodes.Length; j++)
                    {
                        xzPlaneDistVec[j - 1] = new Vector2(nodes[j].Point[0] - thisShape_x,
                            nodes[j].Point[1] - thisShape_z);
                    }

                    bool vectorOpposeDirFlag = false;
                    for (int j = 0; j < nVectors; j++)
                    {
                        for (int k = j + 1; k < nVectors; k++)
                        {
                            if (Vector3.Angle(xzPlaneDistVec[j], xzPlaneDistVec[k]) > 150f)
                            {
                                vectorOpposeDirFlag = true;
                                break;
                            }
                        }
                    }

                    bool minDistFlag = false;
                    for (int j = 0; j < nVectors; j++)
                    {
                        if (xzPlaneDistVec[j].magnitude < minDistBetweenTargets)
                        {
                            minDistFlag = true;
                            break;
                        }
                    }

                    float minDist = 99999f;
                    for (int j = 0; j < nVectors; j++)
                    {
                        if (xzPlaneDistVec[j].magnitude < minDist)
                        {
                            minDist = xzPlaneDistVec[j].magnitude;
                        }
                    }

                    if (minDistFlag == false)
                    {
                        if(shapeControllers[activeShapeIndexList[i]].movingState == 1)
                        {
                            if(minDist > minDistStopLeaving)
                            {
                                shapeControllers[activeShapeIndexList[i]].shapeNearbyState = 0;
                            }
                        }
                        else
                        {
                            shapeControllers[activeShapeIndexList[i]].shapeNearbyState = 0;
                        }
                    }
                    else if (minDistFlag == true && vectorOpposeDirFlag == false)
                    {

                        Vector2 leaveDir = new Vector2(0, 0);
                        for (int j = 0; j < nVectors; j++)
                        {
                            leaveDir += xzPlaneDistVec[j].normalized;
                        }
                        leaveDir = -leaveDir;

                        Vector2 forwardPointerXZ = new Vector2(shapeControllers[activeShapeIndexList[i]].currentFowardPointer.x,
                            shapeControllers[i].currentFowardPointer.z);

                        //if leave direction is same as forward direction, still move
                        if (Vector2.Angle(leaveDir, forwardPointerXZ) < 90f) {
                            shapeControllers[activeShapeIndexList[i]].shapeNearbyState = 0;
                        }
                        else
                        {
                            shapeControllers[activeShapeIndexList[i]].shapeNearbyState = 1;
                            shapeControllers[activeShapeIndexList[i]].normalizedLeavingDirection = new Vector3(leaveDir.x, 0, leaveDir.y);
                        }

                    }
                    else if (minDistFlag == true && vectorOpposeDirFlag == true)
                    {
                        shapeControllers[activeShapeIndexList[i]].shapeNearbyState = 2;
                    }
                }
                else //no nearest neighbor inside radius
                {
                    shapeControllers[activeShapeIndexList[i]].shapeNearbyState = 0;
                }

            } //end search KdTree

            kDTreeShapes.Clear();   //TEST: resources of KDTree need further revision
        }
     
    }//end update

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
        Transform t = o.transform;
        t.localScale = Vector3.one * Random.Range(1f, 2f);

        Vector3 newPosition = new Vector3(Random.Range(-10f, 10f), footLevelPositionTarget + t.localScale.y / 2,
            Random.Range(1f, 10f));

        int maximumTry = 4;
        int tryCount = 0;
        while (tryCount < maximumTry)
        {
            bool foundInside = false;
            for (int i = 0; i < shapeControllers.Count; i++)
            {
                if ((newPosition - shapeControllers[i].transform.position).magnitude < minDistBetweenTargets)
                {
                    foundInside = true;
                }
            }

            if (foundInside == true)
            {
                newPosition = new Vector3(Random.Range(-10f, 10f), footLevelPositionTarget + t.localScale.y / 2,
                    Random.Range(1f, 10f));
                tryCount++;
            }
            else
            {
                break;
            }
        }

        if (tryCount < maximumTry)   //not failed
        {

            //put targets under one layer
            o.gameObject.transform.SetParent(this.transform.GetChild(0).transform);
            o.gameObject.layer = this.transform.GetChild(0).gameObject.layer;
            foreach (Transform tran in o.gameObject.GetComponentsInChildren<Transform>(true))
            {
                tran.gameObject.layer = this.transform.GetChild(0).gameObject.layer;
            }

            t.localPosition = newPosition;

            shapeControllers.Add(o);
        }
        else
        {
            //cannot generate more targets
            targetFactory.Reclaim(o);
        }
    }

    //destroy last generated ShapeController
    public void DestroyTarget()
    {
        if (shapeControllers.Count > 0)
        {
            int index = shapeControllers.Count - 1;

            if(index == playerLockedTargetIndex)
            {
                unlockTarget();
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
                mainCamera.GetComponent<CameraBehavior>().lockTarget(targetShape.transform);
                    
            }

        }

    }

    public void unlockTarget()
    {
        passiveLayerBehavior.removeLockImage();
        playerBehavior.unlockTarget();
        mainCamera.GetComponent<CameraBehavior>().unlockTarget();
        playerLockedTargetIndex = -1;
    }

    public void TargetAttack()
    {
        if (shapeControllers.Count > 0 && shapeControllers[shapeControllers.Count-1].state == 1)
        {
            shapeControllers[shapeControllers.Count - 1].Attack1();
        }

    }
}

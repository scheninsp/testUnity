using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{

    public TargetFactory targetFactory;

    List<Shape> shapes;

    private float headLevelPosition = 0.8f;

    public Transform playerTransform;
    Transform initialPlayerTransform = null;

    public PlayerBehavior playerBehavior;

    public PassiveLayerBehavior passiveLayerBehavior;

    int playerLockedTargetIndex = -1;

    void Awake()
    {
        shapes = new List<Shape>();
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        BeginNewGame();
    }

    private void Update()
    {
        if(shapes.Count == 0)
        {
            passiveLayerBehavior.removeLockImage();
            playerBehavior.unlockTarget();
        }

    }

    void BeginNewGame()
    {
        //if begin from a current state, clear current state
        for (int i = 0; i < shapes.Count; i++)
        {
            targetFactory.Reclaim(shapes[i]);
        }
        shapes.Clear();

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
        Shape o = targetFactory.GetRandom();

        Transform t = o.transform;
        t.localPosition = new Vector3(Random.Range(-20f,20f), headLevelPosition,
            Random.Range(1f, 20f));
        //t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.5f, 2f);
        /*o.SetColor(Random.ColorHSV(hueMin: 0f, hueMax: 1f,
            saturationMin: 0.5f, saturationMax: 1f, valueMin: 0.25f, valueMax: 1f,
            alphaMin: 1f, alphaMax: 1f));*/

        shapes.Add(o);

    }

    //destroy last generated shape
    public void DestroyTarget()
    {
        if (shapes.Count > 0)
        {
            int index = shapes.Count - 1;

            if(index == playerLockedTargetIndex)
            {
                passiveLayerBehavior.removeLockImage();
                playerBehavior.unlockTarget();
                playerLockedTargetIndex = -1;
            }

            targetFactory.Reclaim(shapes[index]);
            shapes.RemoveAt(index);
        }
    }

    public void LockTarget()
    {
        if (shapes.Count > 0)
        {
            //calculate distance to all targets, select closest
            float dist_closest = 99999f;
            int index_closest = -1;
            for (int i = 0; i < shapes.Count; i++)
            {
                Vector3 targetPosition = shapes[i].GetComponent<Transform>().position;
                targetPosition = playerTransform.position - targetPosition;
                float dist = targetPosition.magnitude;
                if (dist < dist_closest)
                {
                    dist_closest = dist;
                    index_closest = i;
                }
            }

            if(index_closest > -1)
            {
                Vector3 targetPositionFinal = shapes[index_closest].GetComponent<Transform>().position;
                Quaternion rotateAngle = playerBehavior.playerRotateTo(targetPositionFinal);
                playerLockedTargetIndex = index_closest;
                playerBehavior.lockTarget(shapes[index_closest]);

                passiveLayerBehavior.generateLockImage(shapes[index_closest]);
            }

        }

    }

}

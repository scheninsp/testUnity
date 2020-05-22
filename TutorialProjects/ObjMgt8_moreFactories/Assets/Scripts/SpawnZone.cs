using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnZone : PersistableObject
{
    public abstract Vector3 SpawnPoint { get; }

    [System.Serializable]
    public struct SpawnConfiguration
    {

        public enum MovementDirection
        {
            Forward,
            Upward,
            Outward,
            Random
        }

        public MovementDirection movementDirection;
        public FloatRange speed;
        public FloatRange angularSpeed;
        public FloatRange scale;
        public ColorRangeHSV color;

        public bool uniformColor;

        public ShapeFactory[] factories;

    }

    [SerializeField]
    SpawnConfiguration spawnConfig;

    public virtual Shape SpawnShape()
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();

        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;

        if (spawnConfig.uniformColor)
        {
            Color setColor = spawnConfig.color.RandomInRange;
            for (int i = 0; i < shape.ColorCount; i++)
            {
                shape.SetColor(setColor, i);
            }
        }
        else
        {
            for(int i=0; i<shape.ColorCount; i++)
            {
                shape.SetColor(spawnConfig.color.RandomInRange, i );
            }
        }


        shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;  
        //50 = 1/time step of fixedUpdate

        Vector3 direction = Vector3.zero;
        switch (spawnConfig.movementDirection) {
            case SpawnConfiguration.MovementDirection.Forward:                
                direction = transform.forward;
                break;

            case SpawnConfiguration.MovementDirection.Upward:               
                direction = transform.up;
                break;

            case SpawnConfiguration.MovementDirection.Outward:               
                //t position = t localposition because t has no father
                direction = (t.position - transform.position).normalized;
                break;

            case SpawnConfiguration.MovementDirection.Random:                
                direction = Random.onUnitSphere;
                break;
        }
        shape.Velocity = direction * spawnConfig.speed.RandomValueInRange;

        return shape;
    }

}

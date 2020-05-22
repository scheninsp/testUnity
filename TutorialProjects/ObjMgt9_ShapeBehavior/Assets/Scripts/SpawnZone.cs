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

        public MovementDirection oscillationDirection;
        public FloatRange oscillationAmplitude;
        public FloatRange oscillationFrequency;
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

        float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
        if (angularSpeed != 0f)
        {
            var rotation = shape.AddBehavior<RotationShapeBehavior>();
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }
        //50 = 1/time step of fixedUpdate

        
        float speed = spawnConfig.speed.RandomValueInRange;
        if(speed != 0f)
        {
            var movement = shape.AddBehavior<MovementShapeBehavior>();
            movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
        }

        SetupOscillation(shape);

        return shape;
    }

    void SetupOscillation(Shape shape)
    {
        float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
        float frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
        if (amplitude == 0f || frequency == 0f)
        {
            return;
        }
        var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
        oscillation.Offset = GetDirectionVector(
            spawnConfig.oscillationDirection, shape.transform
        ) * amplitude;
        oscillation.Frequency = frequency;
    }

    Vector3 GetDirectionVector(SpawnConfiguration.MovementDirection d,
        Transform t)
    {
        Vector3 direction;
        switch (d)
        {
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

            default:
                direction = transform.forward;
                break;
        }
        return direction;
    }

}

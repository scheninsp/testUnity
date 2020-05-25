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

        [System.Serializable]
        public struct SatelliteConfiguration  //property of satellite object
        {
            [FloatRangeSlider(0.1f, 1f)]
            public FloatRange relativeScale;
            public IntRange satelliteAmount;

            public bool uniformLifecycles;
        }

        public SatelliteConfiguration satellite;
        public FloatRange orbitRadius;
        public FloatRange orbitFrequency;

        [System.Serializable]
        public struct LifecycleConfiguration
        {
            [FloatRangeSlider(0f, 2f)]
            public FloatRange growingDuration;

            [FloatRangeSlider(0f, 20f)]
            public FloatRange adultDuration;

            [FloatRangeSlider(0f, 2f)]
            public FloatRange dyingDuration;

            public Vector3 RandomDurations
            {
                get
                {
                    return new Vector3(
                        growingDuration.RandomValueInRange,
                        adultDuration.RandomValueInRange,
                        dyingDuration.RandomValueInRange
                    );
                }
            }
        }

        public LifecycleConfiguration lifecycle;

    }

    [SerializeField]
    SpawnConfiguration spawnConfig;

    [SerializeField, Range(0f, 50f)]
    float spawnSpeed;
    float spawnProgress;

    private void FixedUpdate()
    {
        spawnProgress += Time.deltaTime * spawnSpeed;
        while (spawnProgress >= 1f)
        {
            spawnProgress -= 1f;
            SpawnShapes();
        }
    }

    public virtual void SpawnShapes()
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();

        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;

        SetupColor(shape);

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

        Vector3 lifecycleDurations = spawnConfig.lifecycle.RandomDurations;

        int satelliteCount = spawnConfig.satellite.satelliteAmount.RandomValueInRange;
        for(int i=0; i<satelliteCount; i++)
        {
            if (spawnConfig.satellite.uniformLifecycles)
            {
                CreateSatelliteFor(shape, lifecycleDurations);
            }
            else{
                Vector3 lifecycleDurationsSatellites = spawnConfig.lifecycle.RandomDurations;
                CreateSatelliteFor(shape, lifecycleDurationsSatellites);
            }
        }

        SetupLifeCycle(shape, lifecycleDurations);
    }

    void SetupColor(Shape shape)
    {
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
            for (int i = 0; i < shape.ColorCount; i++)
            {
                shape.SetColor(spawnConfig.color.RandomInRange, i);
            }
        }
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


    void CreateSatelliteFor(Shape focalShape, Vector3 lifeCycleDurations)
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        Transform t = shape.transform;
        t.localRotation = Random.rotation;
        t.localScale = focalShape.transform.localScale *
                        spawnConfig.satellite.relativeScale.RandomValueInRange;

        SetupColor(shape);
        shape.AddBehavior<SatelliteShapeBehavior>().Initialize(
            shape, focalShape, spawnConfig.orbitRadius.RandomValueInRange,
            spawnConfig.orbitFrequency.RandomValueInRange);

        SetupLifeCycle(shape, lifeCycleDurations);

    }

    void SetupLifeCycle(Shape shape, Vector3 durations)
    {
        if (durations.x > 0f)
        {
            //if grow, adult, die all configured
            if (durations.y > 0f || durations.z > 0f)
            {
                shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                    shape, durations.x, durations.y, durations.z
                );
            }
            else //if grow configured
            {
                shape.AddBehavior<GrowingShapeBehavior>().Initialize(
                    shape, durations.x
                );
            }
        }
        else if (durations.y > 0f)
        //if grow <= 0, adult configured
        {
            shape.AddBehavior<LifecycleShapeBehavior>().Initialize(
                shape, durations.x, durations.y, durations.z
            );
        }
        else if (durations.z > 0f)
        //if grow <= 0, adult<=0, die configured
        {
            shape.AddBehavior<DyingShapeBehavior>().Initialize(
                shape, durations.z
            );
        }
        else
        {
            Debug.LogError("Parameters of lifecycle must at least have one positive");
        }
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(spawnProgress);
    }

    public override void Load(GameDataReader reader)
    {
        spawnProgress = reader.ReadFloat();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeBehaviorType
{
    Movement,
    Rotation,
    Oscillation
}

//use extension method to bind an object with its generator( pool )
public static class ShapeBehaviorTypeMethods
{
    public static ShapeBehavior GetInstance(this ShapeBehaviorType type)
    {
        switch (type)
        {
            case ShapeBehaviorType.Movement:
                return ShapeBehaviorPool<MovementShapeBehavior>.Get();
            case ShapeBehaviorType.Rotation:
                return ShapeBehaviorPool<RotationShapeBehavior>.Get();
            case ShapeBehaviorType.Oscillation:
                return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
        }
        UnityEngine.Debug.Log("Forgot to support " + type);
        return null;
    }

}
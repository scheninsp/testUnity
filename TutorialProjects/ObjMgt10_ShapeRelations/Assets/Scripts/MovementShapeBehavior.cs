using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class MovementShapeBehavior : ShapeBehavior
{

    public Vector3 Velocity { get; set; }

    public override ShapeBehaviorType BehaviorType { get {
            return ShapeBehaviorType.Movement;
        } }

    public override bool GameUpdate(Shape shape)
    {
        shape.transform.localPosition += Velocity * Time.deltaTime;
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(Velocity);
    }

    public override void Load(GameDataReader reader)
    {
        Velocity = reader.ReadVector3();
    }

    public override void Recycle()
    {
        //ShapeBehaviorPool is a static class so do not need to instantiate
        ShapeBehaviorPool<MovementShapeBehavior>.Reclaim(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//extend ScriptableObject class to make runtime recompilation does
//not lose all behavior components, without serialize them
public abstract class ShapeBehavior
#if UNITY_EDITOR
    : ScriptableObject
#endif
{

    public abstract ShapeBehaviorType BehaviorType { get; }

#if UNITY_EDITOR
    public bool IsReclaimed { get; set; }

    private void OnEnable()
    {
        //rebuild reclaimed pool after recompilation
        if (IsReclaimed)
        {
            Recycle();
        }
    }
#endif

    public abstract bool GameUpdate(Shape shape);

    public abstract void Save(GameDataWriter writer);

    public abstract void Load(GameDataReader reader);

    public abstract void Recycle();

    public virtual void ResolveShapeInstances() { }


}

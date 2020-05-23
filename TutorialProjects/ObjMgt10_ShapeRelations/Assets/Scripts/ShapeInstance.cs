using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this is a pointer class to shape
[System.Serializable]
public struct ShapeInstance
{
    public Shape Shape { get; set; }

    int instanceIdOrSaveId;

    public ShapeInstance(Shape shape)
    {
        Shape = shape;
        instanceIdOrSaveId = shape.InstanceId;
    }

    public ShapeInstance(int saveIndex)
    {
        //Shape = Game_2.Instance.GetShape(saveIndex); //fetch instance in shapes List
        //this might not working when focalShape hasn't been loaded
        Shape = null;
        instanceIdOrSaveId =saveIndex;
    }

    public void Resolve()
    {
        if(instanceIdOrSaveId >= 0)
        {
            Shape = Game_2.Instance.GetShape(instanceIdOrSaveId);
            instanceIdOrSaveId = Shape.InstanceId;
        }
    }

    public bool isValid
    {
        get
        {
            if(Shape != null)
            {
                return instanceIdOrSaveId == Shape.InstanceId;
            }
            else
            {
                return false;
            }
        }
    }

    //casting operator
    //explicit/implicit cast
    public static implicit operator ShapeInstance (Shape shape)
    {
        return new ShapeInstance(shape);
    }
}

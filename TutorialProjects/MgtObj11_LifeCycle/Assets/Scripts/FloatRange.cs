using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FloatRange
{
    public float min, max;
    public float RandomValueInRange {
        get
        {
            return Random.Range(min, max);
        }
    }
}
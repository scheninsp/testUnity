using UnityEngine;

public class FloatRangeSliderAttribute : PropertyAttribute
{
    public float Min { get; set; }
    public float Max { get; set; }

    //constructor
    public FloatRangeSliderAttribute(float min, float max)
    {
        if (max < min)
        {
            max = min;
        }
        Min = min;
        Max = max;
    }
}

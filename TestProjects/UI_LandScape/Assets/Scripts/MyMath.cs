using UnityEngine;

public class MyMath
{ 
    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
    {
        return Mathf.Atan2(
            Vector3.Dot(n, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    public static float TriangleFunction(float t, float Tcycle)
    {//triangle funciton with Tcycle

        float t_rem = t - Mathf.Floor(t / Tcycle) * Tcycle;
        float currentLerpVal = t_rem / Tcycle;

        if (currentLerpVal <= 0.5f)
        {
            currentLerpVal = 2 * currentLerpVal;
        }
        else
        {
            currentLerpVal = - 2 * currentLerpVal + 2;
        }

        return currentLerpVal;
    }

    public static float TransformAngle(float deg)
    {   //change angle , match (0-180) to(0-180), (180-360) to (-180,-0)
        //0 = -180; 180 = 360, be careful don't use these values as threshold
        float angle = deg - 180;

        if (angle > 0)
        {
            return angle - 180;
        }
        else
        {
            return angle + 180;
        }
    }
}

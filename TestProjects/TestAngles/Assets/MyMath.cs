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
}

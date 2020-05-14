using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Clock : MonoBehaviour
{
    public Transform hoursTransform;
    public Transform minuteTransform;
    public Transform secondTransform;
    public bool continuous;

    const float degreesPerHour = 30f;
    const float degreesPerMinute = 6f;
    const float degreesPerSecond = 6f;


    void Awake()
    {
        hoursTransform.localRotation =
           Quaternion.Euler(0f, DateTime.Now.Hour * degreesPerHour, 0f); //rotation angles on x,y,z axis
        minuteTransform.localRotation =
           Quaternion.Euler(0f, DateTime.Now.Minute * degreesPerMinute, 0f);
        secondTransform.localRotation =
           Quaternion.Euler(0f, DateTime.Now.Second * degreesPerSecond, 0f);
    }

    // Update is called once per frame
    void UpdateDiscrete()
    {
        hoursTransform.localRotation =
            Quaternion.Euler(0f, DateTime.Now.Hour * degreesPerHour, 0f);
        minuteTransform.localRotation =
           Quaternion.Euler(0f, DateTime.Now.Minute * degreesPerMinute, 0f);
        secondTransform.localRotation =
           Quaternion.Euler(0f, DateTime.Now.Second * degreesPerSecond, 0f);

    }

    void UpdateContinuous()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        hoursTransform.localRotation =
        Quaternion.Euler(0f, (float)time.TotalHours * degreesPerHour, 0f);
        minuteTransform.localRotation =
           Quaternion.Euler(0f, (float)time.TotalMinutes * degreesPerMinute, 0f);
        secondTransform.localRotation =
           Quaternion.Euler(0f, (float)time.TotalSeconds * degreesPerSecond, 0f);

    }

    void Update()
    {
        if (continuous) { UpdateContinuous(); }
        else { UpdateDiscrete(); }
    }
}

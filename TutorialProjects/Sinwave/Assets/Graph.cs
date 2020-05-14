using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform pointPrefab;
    [Range(10, 100)]
    public int resolution = 10;
    Transform[] points; 

    // Start is called before the first frame update
    void Awake()
    {
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        Vector3 position;
        position.z = 0f;
        position.y = 0f;
        points = new Transform[resolution];
        for (int i=0; i<resolution; i++)
        {   
            Transform point = Instantiate(pointPrefab);
            position.x = (i + 0.5f) * step - 1f;
            //position.y = position.x * position.x; //move to update
            point.localPosition = position;
            point.localScale = scale;
            point.SetParent(transform);
            points[i] = point;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < resolution; i++){
            Transform point = points[i];
            Vector3 position = point.localPosition;
            position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
            point.localPosition = position;
        }
    }
}

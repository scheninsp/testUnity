using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{

    public float springForce = 20f;
    public float damping = 5f;
    float uniformScale = 1f;

    Mesh deformingMesh;
    Vector3[] originalVertices, displaceVertices;
    Vector3[] vertexVelocities;

    void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displaceVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displaceVertices[i] = originalVertices[i];
        }

        vertexVelocities = new Vector3[originalVertices.Length];
    }

    //point: the hit point with collider
    //used in MeshDeformerInput
    public void AddDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point); //correct when object is transformed
        //Debug.DrawLine(Camera.main.transform.position, point);
        for(int i=0; i<displaceVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = displaceVertices[i] - point;
        pointToVertex *= uniformScale;  //force is uniform to scaling object
        float attenuatedForce = force / (1 + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }

    private void Update()
    {
        for(int i=0; i < displaceVertices.Length; i++)
        {
            UpdateVertex(i);
        }
        deformingMesh.vertices = displaceVertices;
        deformingMesh.RecalculateNormals();
    }

    void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displaceVertices[i] - originalVertices[i];
        displacement *= uniformScale;  //force is uniform to scaling object
        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;
        vertexVelocities[i] = velocity;
        displaceVertices[i] += velocity * (Time.deltaTime / uniformScale);
    }
}

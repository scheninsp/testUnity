using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinNormalsRecal : MonoBehaviour
{
    public Transform center_of_normals ;
    public float fix_weight;

    void Start()
    {
        var mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
        //mesh.normals = SmoothNormals(mesh.normals);
        mesh.normals = SmoothNormalsByVertAndNorm(mesh.vertices, mesh.normals);
    }

    Vector3[] SmoothNormals(Vector3[] normals_in)
    {
        Vector3[] normals_out = new Vector3[normals_in.Length];
        int smooth_range = 2;
        for(int i=smooth_range; i<normals_in.Length-smooth_range; i++)
        {
            normals_out[i] = new Vector3(0, 0, 0);
            for (int j=0; j<smooth_range; j++)
            {
                normals_out[i] += normals_in[i+j];
            }
            normals_out[i] /= smooth_range;
        }
        return normals_out;
    }

    Vector3[] SmoothNormalsByVert(Vector3[] vertices_in)
    {
        Vector3[] normals_out = new Vector3[vertices_in.Length];

        Vector3 center_of_vertices = this.transform.worldToLocalMatrix * center_of_normals.position;

        for (int i = 0; i < vertices_in.Length; i++)
        {
            normals_out[i] = new Vector3(0, 0, 0);
            normals_out[i] = (vertices_in[i] - center_of_vertices).normalized;
        }

        return normals_out;
    }

    Vector3[] SmoothNormalsByVertAndNorm(Vector3[] vertices_in, Vector3[] normals_in)
    {
        Vector3[] normals_out = new Vector3[vertices_in.Length];

        Vector3 center_of_vertices = this.transform.worldToLocalMatrix * center_of_normals.position;

        for (int i = 0; i < vertices_in.Length; i++)
        {
            normals_out[i] = normals_in[i];
            normals_out[i] += fix_weight * (vertices_in[i] - center_of_vertices).normalized;
            normals_out[i].Normalize();
        }

        return normals_out;
    }
}

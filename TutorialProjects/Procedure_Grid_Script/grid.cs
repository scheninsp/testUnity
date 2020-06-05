using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MeshFilter), typeof(MeshRenderer))]
public class grid : MonoBehaviour
{
    public int xSize, ySize;

    void Awake()
    {
        StartCoroutine(Generate());
    }

    private Vector3[] vertices;
    private Mesh mesh;

    private IEnumerator Generate()
    {
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "Procedural Grid";

        vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, y);
            }
        }
        mesh.vertices = vertices;

        int[] triangles = new int[xSize * ySize * 6]; //6 vertices for 1 square
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
                //mesh.RecalculateNormals();  //Do not use this
            }
        }
        mesh.triangles = triangles;

        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                uv[i] = new Vector2((float)x / xSize, (float)y/ySize);
            }
        }
        mesh.uv = uv;

        yield return wait;

    }

    private void OnDrawGizmos()
    {
        if (vertices == null) { return; }

        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}

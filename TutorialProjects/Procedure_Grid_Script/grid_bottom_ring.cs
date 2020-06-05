using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MeshFilter), typeof(MeshRenderer))]
public class grid : MonoBehaviour
{
    public int xSize, ySize, zSize;

    private Vector3[] vertices;
    private Mesh mesh;

    void Awake()
    {
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        WaitForSeconds wait = new WaitForSeconds(0.05f);

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";

        //------------ Generate Vertices ----------------
        //xSize+1 vertices are needed for each edge
        int numVertices = 2 * ((xSize - 1) * (ySize - 1) + (ySize - 1) * (zSize - 1) + (xSize - 1) * (zSize - 1));
        numVertices += ((xSize - 1) + (ySize - 1) + (zSize - 1)) * 4;
        numVertices += 8;

        vertices = new Vector3[numVertices];

        int v = 0;
        //bottom surface
        for (int z = 0; z < zSize + 1; z++)
        {
            for (int x = 0; x < xSize + 1; x++)
            {
                vertices[v++] = new Vector3(x, 0, z);
                yield return wait;
            }
        }

        //middle rings , counter clockwise
        for (int y = 1; y < ySize; y++)
        {
            for (int x = 0; x < xSize + 1; x++)
            {
                vertices[v++] = new Vector3(x, y, 0);
                yield return wait;
            }
            for (int z = 1; z < zSize; z++)
            {
                vertices[v++] = new Vector3(xSize, y, z);
                yield return wait;
            }
            for (int x = xSize; x >= 0; x--)
            {
                vertices[v++] = new Vector3(x, y, zSize);
                yield return wait;
            }
            for (int z = zSize -1; z >0; z--) //x has already draw (x=0, z=0)
            {
                vertices[v++] = new Vector3(0, y, z);
                yield return wait;
            }

        }

        //top surface
        for (int z = 0; z < zSize + 1; z++)
        {
            for (int x = 0; x < xSize + 1; x++)
            {
                vertices[v++] = new Vector3(x, ySize, z);
            }
        }
        mesh.vertices = vertices;

        //------------ END Generate Vertices ----------------

        //-------------- Generate Triangles ----------------
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2 * 6;
        int[] triangles = new int[quads];
        int nv_complete = (xSize + 1) * (zSize + 1);
        int nv_ring = 2*((xSize + 1) + (zSize - 1));

        //bottom face + bottom ring
        int t = 0;
        
        for (int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                int v00 = x + z * (xSize + 1);
                int v10 = v00 + xSize + 1;
                int v01 = v00 + 1;
                int v11 = v10 + 1;
                setQuad(triangles,t,v00, v10, v01, v11);
                t += 6;
            }
        }

        //bottom ring front
        for (int x = 0; x < xSize; x++)
        {
            int shift1 = nv_complete;
            setQuad(triangles, t, x, x + 1, x + shift1, x + shift1 + 1);
            t += 6;
            mesh.triangles = triangles;
            yield return wait;
        }

        //bottom ring back
        for (int x = (xSize + 1) * zSize; x < (xSize + 1) * (zSize + 1) -1 ; x++)
        {
            int v00 = x;
            int v10 = x + (xSize + 1) + zSize + ((xSize + 1) * (zSize + 1) - 1 - x) * 2;
            int v01 = x + 1;
            int v11 = v10 - 1;
            setQuad(triangles, t, v00, v10, v01, v11);
            t += 6;
            mesh.triangles = triangles;
            yield return wait;
        }

        //bottom ring left
        int ov00 = 0;   
        int ov10 = nv_complete;  //first z is shared by x so cannot included into for loop
        int ov01 = xSize + 1;
        int ov11 = nv_complete + nv_ring - 1;   
        setQuad(triangles, t, ov00, ov10, ov01, ov11);
        t += 6;
        mesh.triangles = triangles;

        for (int z = 1; z < zSize; z++)
        {
            int v00 = z * (xSize+1);
            int v10 = nv_complete + nv_ring - z;
            int v01 = (z+1) * (xSize + 1);
            int v11 = v10 - 1;
            setQuad(triangles, t, v00, v10, v01, v11);
            t += 6;
            mesh.triangles = triangles;
            yield return wait;
        }

        //bottom ring right
        for (int z = 0; z < zSize; z++)
        {
            int v00 = z * (xSize + 1) + xSize;
            int v10 = (z + 1) * (xSize + 1) + xSize;
            int v01 = nv_complete + xSize + z;
            int v11 = v01 + 1;
            setQuad(triangles, t, v00, v10, v01, v11);
            t += 6;
            mesh.triangles = triangles;
            yield return wait;
        }


        //middle rings

        //top face + top ring
        yield return wait;

    }

    private static void setQuad(int[] triangles, int i, int v00, int v10, int v01, int v11){
        //leftbottom 00 , lefttop 01 righttop 11 rightbottom 10
        triangles[i] = v00;
        triangles[i + 1] = v01;
        triangles[i + 2] = v10;
        triangles[i + 3] = v10;
        triangles[i + 4] = v01;
        triangles[i + 5] = v11;
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

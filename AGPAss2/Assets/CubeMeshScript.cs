using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CubeMeshScript : MonoBehaviour {

    public Mesh mesh;
    public Material mat;
    public int resolution;
    public float height, radius;
    public bool showVertices;
    public bool showMesh;
    public bool showCircleVertices;
    public bool showSideVertices;
    public bool showVertexNumbers;

    Vector3[] circleVertexArray;
    Vector3[] sideVertexArray;
    int[] circleTriangles;
    int[] sideTriangles;
    public Vector2[] uvs;
    public Vector3[] totalVertices;
    public int[] totalTriangles;

    void Start()
    {
        showVertices = true;
        showMesh = true;
    }

    public void RebuildMesh()
    {
        showMesh = true;
        mesh = new Mesh();

        resolution = Mathf.Clamp(resolution, 1, 400);

        circleVertexArray = new Vector3[2 * (1 + resolution)];//these are the vertices used for the circle faces
        sideVertexArray = new Vector3[2 * resolution];//these are the vertices used for the face wrapping the cylinder 
        circleTriangles = new int[resolution * 6];//fix number overflow (initialization of int can be a negative number, as is now)
        sideTriangles = new int[resolution * 6];
        uvs = new Vector2[circleVertexArray.Length + sideVertexArray.Length];

        circleVertexArray[2 * (1 + resolution) - 1] = new Vector3(0, .5f * height, 0);
        circleVertexArray[2 * (1 + resolution) - 2] = new Vector3(0, -.5f * height, 0);

        for (int i = 0; i < resolution; i++)
        {
            float cornerAngle = (float)i / resolution * 2 * Mathf.PI;
            circleVertexArray[i] = new Vector3(Mathf.Cos(cornerAngle) * radius, .5f * height, Mathf.Sin(cornerAngle) * radius);
            sideVertexArray[i] = circleVertexArray[i];
            circleVertexArray[i + resolution] = new Vector3(Mathf.Cos(cornerAngle) * radius, -.5f * height, Mathf.Sin(cornerAngle) * radius);
            sideVertexArray[i + resolution] = circleVertexArray[i + resolution];
        }

        for (int triIndex = 0, vertIndex = 0; triIndex < circleTriangles.Length && vertIndex < circleVertexArray.Length; triIndex += 6, vertIndex++)
        {
            if (triIndex == (resolution - 1) * 6)//if we're at the last vertex of the circle
            {
                //build a triangle on the top circle from the last vertex on that circle
                circleTriangles[triIndex] = vertIndex;
                circleTriangles[triIndex + 1] = 2 * (1 + resolution) - 1;
                circleTriangles[triIndex + 2] = 0;

                //build a triangle on the bottom circle from the last vertex on that circle
                circleTriangles[triIndex + 3] = vertIndex + resolution;
                circleTriangles[triIndex + 4] = resolution;
                circleTriangles[triIndex + 5] = 2 * (1 + resolution) - 2;
            }
            else
            {
                //build a triangle on the top circle
                circleTriangles[triIndex] = vertIndex;
                circleTriangles[triIndex + 1] = 2 * (1 + resolution) - 1;
                circleTriangles[triIndex + 2] = vertIndex + 1;

                //build a triangle on the bottom circle
                circleTriangles[triIndex + 3] = vertIndex + resolution;
                circleTriangles[triIndex + 4] = vertIndex + resolution + 1;
                circleTriangles[triIndex + 5] = 2 * (1 + resolution) - 2;
            }
        }

        for (int triIndex = 0, vertIndex = 0; triIndex < sideTriangles.Length && vertIndex < sideVertexArray.Length; triIndex += 6, vertIndex++)
        {
            if (triIndex == (resolution - 1) * 6)//if we're at the last vertex of the circle
            {
                //build quad on the side from the last vertices of the circles
                sideTriangles[triIndex] = vertIndex;
                sideTriangles[triIndex + 1] = 0;
                sideTriangles[triIndex + 2] = resolution;
                sideTriangles[triIndex + 3] = vertIndex;
                sideTriangles[triIndex + 4] = resolution;
                sideTriangles[triIndex + 5] = vertIndex + resolution;
            }
            else
            {
                //build quad on the side
                sideTriangles[triIndex] = vertIndex;
                sideTriangles[triIndex + 1] = vertIndex + 1;
                sideTriangles[triIndex + 2] = vertIndex + resolution + 1;
                sideTriangles[triIndex + 3] = vertIndex;
                sideTriangles[triIndex + 4] = vertIndex + resolution + 1;
                sideTriangles[triIndex + 5] = vertIndex + resolution;
            }
        }

        totalVertices = new Vector3[circleVertexArray.Length + sideVertexArray.Length];
        totalTriangles = new int[circleTriangles.Length + sideTriangles.Length];
        for (int i = 0; i < circleVertexArray.Length; i++)
        {
            totalVertices[i] = circleVertexArray[i];
        }
        for (int i = 0; i < sideVertexArray.Length; i++)
        {
            totalVertices[i + circleVertexArray.Length] = sideVertexArray[i];
        }

        for (int i = 0; i < circleTriangles.Length; i++)
        {
            totalTriangles[i] = circleTriangles[i];
        }
        for (int i = 0; i < sideTriangles.Length; i++)
        {
            totalTriangles[i + circleTriangles.Length] = sideTriangles[i];
        }

        mesh.vertices = totalVertices;
        mesh.triangles = totalTriangles;

        mesh.RecalculateNormals();

        float uvPerResolution = 1 / resolution;

        //map uvs
        for (int i = 0; i < circleVertexArray.Length; i++)
        {
            uvs[i] = new Vector2(circleVertexArray[i].x, circleVertexArray[i].z).normalized;
        }
        for (int i = 0; i < sideVertexArray.Length; i += 2)
        {
            uvs[i + circleVertexArray.Length] = new Vector2(0, uvPerResolution * i);
            uvs[i + circleVertexArray.Length + 1] = new Vector2(1, uvPerResolution * i);
        }

        mesh.uv = uvs;

        GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<Renderer>().material = mat;
    }

    public void Update()
    {
        if (showMesh)
        {
            RebuildMesh();
        }
        else
        {
            ClearMesh();
        }
    }

    public void ClearMesh()
    {
        showMesh = false;
        mesh = new Mesh
        {
            vertices = new Vector3[0],
            triangles = new int[0]
        };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDrawGizmos()//the gizmo only displays the vertices of the local mesh variable and not 'GetComponent<MeshFilter>().mesh'
    {
        Gizmos.color = Color.yellow;
        if (showVertices)
        {
            if (showVertexNumbers)
            {
                for (int i = 0; i < totalVertices.Length; i++)
                {
                    Handles.Label(totalVertices[i], i.ToString());
                }
            }
            if (showCircleVertices)
            {
                for (int i = 0; i < circleVertexArray.Length; i++)
                {
                    Handles.Label(circleVertexArray[i], uvs[i].ToString());
                    Gizmos.DrawSphere(circleVertexArray[i], .1f);
                }
            }
            if (showSideVertices)
            {
                for (int i = 0; i < sideVertexArray.Length; i++)
                {
                    Handles.Label(sideVertexArray[i], uvs[i + circleVertexArray.Length].ToString());
                    Gizmos.DrawSphere(sideVertexArray[i], .1f);
                }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace {

    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    ShapeGenerator shapeGenerator;

    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                vertices[i] = shapeGenerator.CalculatePointOnPlanet(PointOnCubeToPointOnSphere(pointOnUnitCube));

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    static Vector3 PointOnCubeToPointOnSphere(Vector3 p) 
    {
        float x2 = p.x * p.x;
        float y2 = p.y * p.y;
        float z2 = p.z * p.z;
        double x1 = p.x * Math.Sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
        double y1 = p.y * Math.Sqrt(1 - (z2 + x2) / 2 + (z2 * x2) / 3);
        double z1 = p.z * Math.Sqrt(1 - (x2 + y2) / 2 + (x2 * y2) / 3);
        float x = Convert.ToSingle(x1);
        float y = Convert.ToSingle(y1);
        float z = Convert.ToSingle(z1);
        return new Vector3(x, y, z);
    }
}
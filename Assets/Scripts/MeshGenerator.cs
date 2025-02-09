using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public float viewDistance;
    public float fovAngle;
    public int segments; 

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = CreateFOVMesh();
    }

    Mesh CreateFOVMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // The center of the cone is at the origin
        vertices.Add(Vector3.zero);

        float halfFOV = fovAngle * 0.5f;
        float angleIncrement = fovAngle / segments;

        // Create vertices along the arc
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfFOV + i * angleIncrement;
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 vertex = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * viewDistance;
            vertices.Add(vertex);
        }

        // Create triangles from the center to each segment edge
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
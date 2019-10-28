using System;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using System.Linq;

public class DelaunayMesh : MonoBehaviour
{
    public Transform chunkPrefab = null;
    private List<float> elevations;
    private TriangleNet.Mesh mesh;
    private readonly List<Transform> chunks = new List<Transform>();
    private bool debug = false;

    public void Generate(IEnumerable<Vector3> locations, Transform groundPlaneTransform, Action meshGenerationCompleted)
    {
        var polygon = new Polygon();
        elevations = new List<float>();

        foreach (var location in locations)
        {
            polygon.Add(new Vertex(location.x, location.z));
        }

        var options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = false };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);

        var globalLocalPositions = locations.ToArray();
        for (var i = 0; i < globalLocalPositions.Length; i++)
        {
            var globalLocalPosition = globalLocalPositions[i];

            var waterHeight = globalLocalPosition.y;

            if(waterHeight != -9999)
            {
                elevations.Add((float)waterHeight + groundPlaneTransform.position.y);
            }
            else
            {
                elevations.Add(groundPlaneTransform.position.y);
            }
        }

        ClearMesh();
        var trianglesInChunk = 5000;
        MakeMesh(trianglesInChunk);
        meshGenerationCompleted();
    }

    private void MakeMesh(int trianglesInChunk)
    {
        var triangleEnumerator = mesh.Triangles.GetEnumerator();

        var numberOfChunks = 1;
        for (var chunkStart = 0; chunkStart < mesh.Triangles.Count; chunkStart += trianglesInChunk)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var chunkEnd = chunkStart + trianglesInChunk;

            for (var i = chunkStart; i < chunkEnd; i++)
            {
                if (!triangleEnumerator.MoveNext())
                {
                    // If we hit the last triangle before we hit the end of the chunk, stop
                    break;
                }

                var triangle = triangleEnumerator.Current;
                var v0 = GetPoint3D(triangle.vertices[2].id);
                var v1 = GetPoint3D(triangle.vertices[1].id);
                var v2 = GetPoint3D(triangle.vertices[0].id);

                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                var normal = Vector3.Cross(v1 - v0, v2 - v0);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
            }

            var chunkMesh = new Mesh
            {
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray(),
                normals = normals.ToArray()
            };

            // Instantiate the GameObject which will display this chunk
            var chunk = Instantiate(chunkPrefab, transform.position, transform.rotation);
            chunk.GetComponent<MeshFilter>().mesh = chunkMesh;
            chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
            chunk.transform.parent = transform;

            chunks.Add(chunk);
        }
    }

    private void ClearMesh()
    {
        if (chunks != null)
        {
            //Debug.Log("Clearing the mesh");
            foreach (var chunk in chunks)
                Destroy(chunk.gameObject);
        }
        chunks.Clear();
    }

    private Vector3 GetPoint3D(int index)
    {
        var vertex = mesh.vertices[index];
        var elevation = elevations[index];
        return new Vector3((float)vertex.x, elevation, (float)vertex.y);
    }

    public void OnDrawGizmos()
    {
        if (!debug) return;

        if (mesh == null)
        {
            // Probably in the editor
            return;
        }

        Gizmos.color = Color.red;
        foreach (var edge in mesh.Edges)
        {
            var v0 = mesh.vertices[edge.P0];
            var v1 = mesh.vertices[edge.P1];
            var p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
            var p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
            Gizmos.DrawLine(p0, p1);
        }
    }
}
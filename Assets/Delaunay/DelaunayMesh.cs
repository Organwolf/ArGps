using ARLocation;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;

public class DelaunayMesh : MonoBehaviour {

    // Prefab which is generated for each chunk of the mesh.
    public Transform chunkPrefab = null;

    // Elevations at each point in the mesh
    private List<float> elevations;

    // The delaunay mesh
    private TriangleNet.Mesh mesh = null;

    private List<Transform> chunks = new List<Transform>();
    private List<Location> csvWaterLocations;

    // Event/Action experiment
    public void OnStringActionInvoked(string msg)
    {
        Debug.Log(msg);
    }

    public void SetPositionsToHandleLocations(List<Location> locationWithWaterHeight)
    {
        csvWaterLocations = locationWithWaterHeight;
    }

    public virtual void Generate(List<Vector3> locations)
    {

        Polygon polygon = new Polygon();
        elevations = new List<float>();

        // Create the polygon for the triangulation
        foreach (Vector3 loc in locations)
        {
            polygon.Add(new Vertex(loc.x, loc.z));
        }

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = false };
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);

        // Adjusted elevation <-- this has to be changed to be relative to the actual ground
        for (int i = 0; i < locations.Count; i++)
        {
            elevations.Add(((float)csvWaterLocations[i].Altitude / 10f) - 2.5f);
        }

        if (chunks != null)
        {
            foreach (Transform chunk in chunks)
            Destroy(chunk.gameObject);
        }
        chunks.Clear();

        MakeMesh();
    }
    
    public void MakeMesh() {
        IEnumerator<Triangle> triangleEnumerator = mesh.Triangles.GetEnumerator();

        for (int chunkStart = 0; chunkStart < mesh.Triangles.Count; chunkStart++) {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            if (!triangleEnumerator.MoveNext())
            {
                break;
            }

            Triangle triangle = triangleEnumerator.Current;

            // For the triangles to be right-side up, they need
            // to be wound in the opposite direction
            Vector3 v0 = GetPoint3D(triangle.vertices[2].id);
            Vector3 v1 = GetPoint3D(triangle.vertices[1].id);
            Vector3 v2 = GetPoint3D(triangle.vertices[0].id);

            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(0.0f, 0.0f));

            Mesh chunkMesh = new Mesh();
            chunkMesh.vertices = vertices.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            chunkMesh.normals = normals.ToArray();

            Transform chunk = Instantiate<Transform>(chunkPrefab, transform.position, transform.rotation);
            chunk.GetComponent<MeshFilter>().mesh = chunkMesh;
            chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
            chunk.transform.parent = transform;
            chunks.Add(chunk);
        }
    }

    /* Returns a point's local coordinates. */
    public Vector3 GetPoint3D(int index) {
        Vertex vertex = mesh.vertices[index];
        float elevation = elevations[index];
        return new Vector3((float)vertex.x, elevation, (float)vertex.y);
    }
    
    public void OnDrawGizmos() {
        if (mesh == null) {
            // Probably in the editor
            return;
        }

        Gizmos.color = Color.red;
        foreach (Edge edge in mesh.Edges) {
            Vertex v0 = mesh.vertices[edge.P0];
            Vertex v1 = mesh.vertices[edge.P1];
            Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
            Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
            Gizmos.DrawLine(p0, p1);
        }
    }
}
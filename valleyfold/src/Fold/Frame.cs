using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.FrameModifications;

namespace valleyfold.Fold;

public enum Assignment
{
    B, // Border
    M, // Mountain Fold
    V, // Valley Fold
    F, // Unfolded ( M/V than open)
    U, // Unspecified Fold
    C, // Cut Fold
    J // Join (Flat triangulated polygon edge)
}

public class Frame
{
    private readonly List<Edge> _edges = new();
    private readonly List<Face> _faces = new();

    public IEnumerable<Face> Faces => _faces;
    public IEnumerable<Edge> Edges => _edges;
    public List<Vertex> Vertices { get; } = new();

    public void InitializePaper()
    {
        Vertices.AddRange(new[]
        {
            new Vertex { Coord = new Vector3(0, 0, 0) },
            new Vertex { Coord = new Vector3(1, 0, 0) },
            new Vertex { Coord = new Vector3(1, 0, 1) },
            new Vertex { Coord = new Vector3(0, 0, 1) }
        });

        _edges.AddRange(new[]
        {
            new Edge
            {
                Vertices = new Id[] { 0, 1 },
                Assignment = Assignment.B,
                FoldAngle = 0
            },
            new Edge
            {
                Vertices = new Id[] { 1, 2 },
                Assignment = Assignment.B,
                FoldAngle = 0
            },
            new Edge
            {
                Vertices = new Id[] { 2, 3 },
                Assignment = Assignment.B,
                FoldAngle = 0
            },
            new Edge
            {
                Vertices = new Id[] { 3, 0 },
                Assignment = Assignment.B,
                FoldAngle = 0
            }
        });

        _faces.AddRange(new[]
        {
            new Face { Vertices = new List<Id> { 0, 1, 2, 3 } }
        });
    }

    public void SplitFace(Face face, Face faceL, Face faceR)
    {
        _faces.Remove(face);
        _faces.Add(faceL);
        _faces.Add(faceR);
    }

    public void AddEdge(Edge edge)
    {
        if (_edges.Contains(edge)) return;
        _edges.Add(edge);
    }

    public List<Edge> NearestEdgesTo(Vector3 point)
    {
        return EdgeQueries.NearestEdgesToPoint(this, point);
    }

    public Id NearestVertexIdTo(Vector3 point, float threshold = float.MaxValue)
    {
        return VertexQueries.NearestVertexIdTo(this, point, threshold);
    }

    public (Vector3, Edge) NearestPointOnEdgeTo(Vector3 point, List<Edge> edges)
    {
        return EdgeQueries.NearestPointOnEdgeToPoint(this, point, edges);
    }

    public Vector3 NearestPointOnEdgeTo(Vector3 point, Edge edge)
    {
        return EdgeQueries.NearestPointOnEdgeTo(this, point, edge);
    }

    public Id AddVertexOnEdge(Vector3 point, Edge edge)
    {
        return EdgeCommands.AddVertexToEdge(this, point, edge);
    }

    public void UnmarkEdges()
    {
        for (var i = 0; i < _edges.Count; i++) _edges[i].IsSelected = false;
    }

    public void UnmarkVertices()
    {
        for (var i = 0; i < Vertices.Count; i++) Vertices[i].IsSelected = false;
    }
}
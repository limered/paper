using System.Collections.Generic;
using Godot;

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
    public List<Face> Faces { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Vertex> Vertices { get; } = new();

    public void InitializePaper()
    {
        Vertices.AddRange(new[]
        {
            new Vertex { Coord = new Vector2(0, 0) },
            new Vertex { Coord = new Vector2(1, 0) },
            new Vertex { Coord = new Vector2(1, 1) },
            new Vertex { Coord = new Vector2(0, 1) }
        });

        Edges.AddRange(new[]
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

        Faces.AddRange(new[]
        {
            new Face { Vertices = new List<Id> { 0, 1, 2, 3 } }
        });
    }

    public void SplitFace(Face face, Face faceL, Face faceR)
    {
        Faces.Remove(face);
        Faces.Add(faceL);
        Faces.Add(faceR);
    }

    public void AddEdge(Edge edge)
    {
        if (Edges.Contains(edge)) return;
        Edges.Add(edge);
    }

    public void UnmarkEdges()
    {
        for (var i = 0; i < Edges.Count; i++) Edges[i].IsSelected = false;
    }

    public void UnmarkVertices()
    {
        for (var i = 0; i < Vertices.Count; i++) Vertices[i].IsSelected = false;
    }
}
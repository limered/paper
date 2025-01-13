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
    private readonly List<Edge> _edges = new();
    private readonly List<Face> _faces = new();

    public IEnumerable<Face> Faces => _faces;
    public List<Vertex> Vertices { get; } = new();

    public void InitializePaper()
    {
        Vertices.AddRange(new[]
        {
            new Vertex { Coord = new Vector3(0, 0, 0), Edges = new List<Id> { 0, 3 } },
            new Vertex { Coord = new Vector3(1, 0, 0), Edges = new List<Id> { 1, 0 } },
            new Vertex { Coord = new Vector3(1, 0, 1), Edges = new List<Id> { 1, 2 } },
            new Vertex { Coord = new Vector3(0, 0, 1), Edges = new List<Id> { 3, 2 } }
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
            new Face { Vertices = new Id[] { 0, 1, 2, 3 }, Edges = new List<Id> { 0, 1, 2, 3 } }
        });
    }

    public void AddCrease(Id start, Id end, Assignment assignment)
    {
    }
}
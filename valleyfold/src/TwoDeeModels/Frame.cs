using System.Collections.Generic;
using Godot;

namespace valleyfold.TwoDeeModels;

public class Frame
{
    public List<Face> Faces { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Vertex> Vertices { get; } = new();

    private int _nextFaceId;

    public Id AddVertex(Vector2 coord)
    {
        var vertex = new Vertex { Coord = coord, Id = Vertices.Count };
        Vertices.Add(vertex);
        return vertex.Id;
    }

    /// <summary>
    /// Assigns the next monotonic <see cref="Face.Id"/> and appends.
    /// All production face creation should funnel through here so face IDs are
    /// unique within a <see cref="Frame"/>'s lifetime.
    /// </summary>
    public Id AddFace(Face face)
    {
        face.Id = _nextFaceId++;
        Faces.Add(face);
        return face.Id;
    }

    public void InitializePaper()
    {
        Vertices.AddRange(new[]
        {
            new Vertex { Coord = new Vector2(0, 0), Id = 0 },
            new Vertex { Coord = new Vector2(1, 0), Id = 1 },
            new Vertex { Coord = new Vector2(1, 1), Id = 2 },
            new Vertex { Coord = new Vector2(0, 1), Id = 3 }
        });

        Edges.AddRange(new[]
        {
            new Edge
            {
                Vertices = new Id[] { 0, 1 },
                Assignment = Assignment.B,
                FoldAngle = 0,
                Id = 0,
            },
            new Edge
            {
                Vertices = new Id[] { 1, 2 },
                Assignment = Assignment.B,
                FoldAngle = 0,
                Id = 1,
            },
            new Edge
            {
                Vertices = new Id[] { 2, 3 },
                Assignment = Assignment.B,
                FoldAngle = 0,
                Id = 2,
            },
            new Edge
            {
                Vertices = new Id[] { 3, 0 },
                Assignment = Assignment.B,
                FoldAngle = 0,
                Id = 3,
            }
        });

        AddFace(new Face { Vertices = new List<Id> { 0, 1, 2, 3 } });
    }

    public void SplitFace(Face face, Face faceL, Face faceR)
    {
        Faces.Remove(face);
        AddFace(faceL);
        AddFace(faceR);
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

    public void AddEdgeAfterSplit(Edge edge, Edge secondEdge)
    {
        Statics.ChangeMemory.AddEdgeToExistingChange(edge.Id, secondEdge.Id);
        AddEdge(secondEdge);
    }
}
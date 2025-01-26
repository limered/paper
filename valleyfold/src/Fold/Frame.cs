using System.Collections.Generic;
using System.Linq;
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
        var nearestEdges = new List<Edge>();
        for (var i = 0; i < _edges.Count; i++)
        {
            var edge = _edges[i];
            var start = Vertices[edge.Vertices[0]].Coord;
            var end = Vertices[edge.Vertices[1]].Coord;
            var forward = end - start;
            var backward = start - end;
            if (forward.Dot(point - start) <= 0) continue;
            if (backward.Dot(point - end) <= 0) continue;

            nearestEdges.Add(edge);
        }

        return nearestEdges;
    }

    public Id NearestVertexIdTo(Vector3 point)
    {
        var nearest = 0;
        var minDist = point.DistanceSquaredTo(Vertices[nearest].Coord);
        for (var i = 1; i < Vertices.Count; i++)
        {
            var dist = point.DistanceSquaredTo(Vertices[i].Coord);
            if (dist < minDist)
            {
                nearest = i;
                minDist = dist;
            }
        }

        return nearest;
    }

    public (Vector3, Edge) NearestPointOnEdgeTo(Vector3 point, List<Edge> edges)
    {

        var nearestDistance = float.MaxValue;
        (Vector3, Edge) result = (Vector3.Zero, null);
        for (var i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            var edgePoint = NearestPointOnEdgeTo(point, edge);
            var dist = edgePoint.DistanceSquaredTo(point);
            if (dist >= nearestDistance) continue;
            
            nearestDistance = dist;
            result.Item1 = edgePoint;
            result.Item2 = edge;
        }
        
        return result;
    }

    public Vector3 NearestPointOnEdgeTo(Vector3 point, Edge edge)
    {
        var start = Vertices[edge.Vertices[0]].Coord;
        var end = Vertices[edge.Vertices[1]].Coord;
        var startToPoint = point - start;
        var startToEnd = end - start;
        return startToPoint.Project(startToEnd) + start;
    }

    public Id AddVertexOnEdge(Vector3 point, Edge edge)
    {
        var id = Vertices.Count;
        Vertices.Add(new Vertex
        {
            Coord = point
        });

        var edgeAdjacentFaces = EdgeAdjacentFaces(edge);
        foreach (var edgeAdjacentFace in edgeAdjacentFaces)
        {
            var startVertexId = edgeAdjacentFace.Vertices.IndexOf(edge.Vertices[0]);
            var endVertexId = edgeAdjacentFace.Vertices.IndexOf(edge.Vertices[1]);
            edgeAdjacentFace.Vertices
                .Insert(startVertexId < endVertexId ? endVertexId : startVertexId, id);
        }

        var secondEdge = new Edge
        {
            Vertices = new Id[] { id, edge.Vertices[1] },
            Assignment = edge.Assignment,
            FoldAngle = edge.FoldAngle
        };
        edge.Vertices[1] = id;
        AddEdge(secondEdge);

        return id;
    }

    public Face[] EdgeAdjacentFaces(Edge edge)
    {
        return _faces.Where(face =>
                face.Vertices.Contains(edge.Vertices[0]) && face.Vertices.Contains(edge.Vertices[1]))
            .ToArray();
    }
}
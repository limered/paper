using System.Collections.Generic;
using Godot;
using valleyfold.Fold;

namespace valleyfold.Folding;

public class EdgeStrip
{
    public readonly List<Edge> Edges = new();
    public readonly List<Edge> EndEdges = new();
    public readonly List<Vector2> EndPoints = new();
    public readonly List<Vector2> Points = new();
    public readonly List<Edge> StartEdges = new();
    public readonly List<Vector2> StartPoints = new();
    public static EdgeStrip Empty => new();
    public int Length => StartPoints.Count;

    public void AddSingle(Vector2 point, Edge edge)
    {
        Points.Add(point);
        Edges.Add(edge);
    }

    public void PrependSingle(Vector2 point, Edge edge)
    {
        Points.Insert(0, point);
        Edges.Insert(0, edge);
    }

    public void Add((Vector2 pointA, Edge edgeA, Vector2 pointB, Edge edgeB) points)
    {
        StartPoints.Add(points.pointA);
        EndPoints.Add(points.pointB);
        StartEdges.Add(points.edgeA);
        EndEdges.Add(points.edgeB);
    }

    public void Prepend((Vector2 pointA, Edge edgeA, Vector2 pointB, Edge edgeB) points)
    {
        StartPoints.Insert(0, points.pointA);
        EndPoints.Insert(0, points.pointB);
        StartEdges.Insert(0, points.edgeA);
        EndEdges.Insert(0, points.edgeB);
    }

    public void Sort()
    {
        if (Length <= 1) return; // sorted because its only one element

        var start = StartEdges[0];
        if (start == EndEdges[1] || start == StartEdges[1])
        {
            (StartPoints[0], EndPoints[0]) = (EndPoints[0], StartPoints[0]);
            (StartEdges[1], EndEdges[1]) = (EndEdges[1], StartEdges[1]);
        }

        for (var i = 1; i < Length; i++)
        {
            var end = EndEdges[i];
            if (end == EndEdges[i - 1])
            {
                (StartPoints[i], EndPoints[i]) = (EndPoints[i], StartPoints[i]);
                (StartEdges[i], EndEdges[i]) = (EndEdges[i], StartEdges[i]);
            }
        }
    }
}
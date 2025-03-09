using System.Collections.Generic;
using Godot;
using valleyfold.Fold;

namespace valleyfold.Folding;

public class EdgeStrip
{
    public List<Edge> EndEdges = new();
    public List<Vector2> EndPoints = new();
    public List<Edge> StartEdges = new();
    public List<Vector2> StartPoints = new();
    public static EdgeStrip Empty  => new();
    public int Length => StartPoints.Count;

    public void Add((Vector2 pointA, Edge edgeA, Vector2 pointB, Edge edgeB) points)
    {
        StartPoints.Add(points.pointA);
        EndPoints.Add(points.pointB);
        StartEdges.Add(points.edgeA);
        EndEdges.Add(points.edgeB);
    }
}
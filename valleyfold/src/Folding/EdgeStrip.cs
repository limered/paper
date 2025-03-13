using System.Collections.Generic;
using Godot;
using valleyfold.Fold;

namespace valleyfold.Folding;

public class EdgeStrip
{
    public readonly List<Edge> Edges = new();
    public readonly List<Vector2> Points = new();
    public static EdgeStrip Empty => new();
    public int Length => Points.Count;

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
}
using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Fold;

namespace valleyfold.FrameModifications;

public static class EdgeQueries
{
    public static List<Edge> NearestEdgesToPoint(Frame frame, Vector3 point)
    {
        var nearestEdges = new List<Edge>();
        for (var i = 0; i < frame.Edges.Count(); i++)
        {
            var edge = frame.Edges.ElementAt(i);
            var start = frame.Vertices[edge.Vertices[0]].Coord;
            var end = frame.Vertices[edge.Vertices[1]].Coord;
            var forward = end - start;
            var backward = start - end;
            if (forward.Dot(point - start) <= 0) continue;
            if (backward.Dot(point - end) <= 0) continue;

            nearestEdges.Add(edge);
        }

        return nearestEdges;
    }

    public static (Vector3, Edge) NearestPointOnEdgeToPoint(Frame frame, Vector3 point, List<Edge> edges)
    {
        var nearestDistance = float.MaxValue;
        (Vector3, Edge) result = (Vector3.Zero, null);
        for (var i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            var edgePoint = NearestPointOnEdgeTo(frame, point, edge);
            var dist = edgePoint.DistanceSquaredTo(point);
            if (dist >= nearestDistance) continue;

            nearestDistance = dist;
            result.Item1 = edgePoint;
            result.Item2 = edge;
        }

        return result;
    }

    public static Vector3 NearestPointOnEdgeTo(Frame frame, Vector3 point, Edge edge)
    {
        var start = frame.Vertices[edge.Vertices[0]].Coord;
        var end = frame.Vertices[edge.Vertices[1]].Coord;
        var startToPoint = point - start;
        var startToEnd = end - start;
        return startToPoint.Project(startToEnd) + start;
    }
}
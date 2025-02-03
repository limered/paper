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
        for (var i = 0; i < frame.Edges.Count; i++)
        {
            var edge = frame.Edges[i];
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

    public static List<Edge> EdgesCrossingEdge(Frame frame, Edge edge)
    {
        var crossedEdges = new List<Edge>();
        foreach (var frameEdge in frame.Edges)
        {
            if (frameEdge == edge) continue;
            if (edge.Vertices[0] == frameEdge.Vertices[0] || edge.Vertices[0] == frameEdge.Vertices[1] ||
                edge.Vertices[1] == frameEdge.Vertices[0] || edge.Vertices[1] == frameEdge.Vertices[1]) continue;

            if (EdgesIntersect(
                    frame.Vertices[edge.Vertices[0]].Coord,
                    frame.Vertices[edge.Vertices[1]].Coord,
                    frame.Vertices[frameEdge.Vertices[0]].Coord,
                    frame.Vertices[frameEdge.Vertices[1]].Coord))
                crossedEdges.Add(frameEdge);
        }

        return crossedEdges;
    }

    private static bool EdgesIntersect(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
    {
        float x1 = start1.X, y1 = start1.Z;
        float x2 = end1.X, y2 = end1.Z;
        float x3 = start2.X, y3 = start2.Z;
        float x4 = end2.X, y4 = end2.Z;

        var denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

        if (denominator == 0)
            return false; // Lines are parallel

        var ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denominator;
        var ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denominator;

        return ua is >= 0 and <= 1 && ub is >= 0 and <= 1;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.TwoDeeModels;

namespace valleyfold.FrameModifications;

public static class EdgeQueries
{
    public static Face[] Faces(this Edge edge)
    {
        var frame = Statics.Frame;
        var collectedFaces = new List<Face>();
        for (var i = 0; i < frame.Faces.Count; i++)
        {
            var face = frame.Faces[i];
            if (face.Vertices.Contains(edge.Vertices[0]) && face.Vertices.Contains(edge.Vertices[1]))
                collectedFaces.Add(face);
        }

        return collectedFaces.ToArray();
    }

    public static List<Edge> NearestEdgesToPoint(Frame frame, Vector2 point)
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

    public static (Vector2, Edge) NearestPointOnEdgeToPoint(Frame frame, Vector2 point, List<Edge> edges)
    {
        var nearestDistance = float.MaxValue;
        (Vector2, Edge) result = (Vector2.Zero, null);
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

    public static Vector2 NearestPointOnEdgeTo(Frame frame, Vector2 point, Edge edge)
    {
        var start = frame.Vertices[edge.Vertices[0]].Coord;
        var end = frame.Vertices[edge.Vertices[1]].Coord;
        var startToPoint = point - start;
        var startToEnd = end - start;
        return startToPoint.Project(startToEnd) + start;
    }


    public static List<Edge> EdgesCrossedByEdge(Frame frame, Id startVertex, Id endVertex)
    {
        var crossedEdges = new List<Edge>();
        foreach (var frameEdge in frame.Edges)
        {
            if ((frameEdge.Vertices[0] == startVertex && frameEdge.Vertices[1] == endVertex) ||
                (frameEdge.Vertices[0] == endVertex && frameEdge.Vertices[1] == startVertex)) continue;

            if (startVertex == frameEdge.Vertices[0] || startVertex == frameEdge.Vertices[1] ||
                endVertex == frameEdge.Vertices[0] || endVertex == frameEdge.Vertices[1]) continue;

            if (EdgesIntersect(
                    frame.Vertices[startVertex].Coord,
                    frame.Vertices[endVertex].Coord,
                    frame.Vertices[frameEdge.Vertices[0]].Coord,
                    frame.Vertices[frameEdge.Vertices[1]].Coord))
                crossedEdges.Add(frameEdge);
        }

        return crossedEdges;
    }

    public static bool EdgesIntersect(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
    {
        float x1 = start1.X, y1 = start1.Y;
        float x2 = end1.X, y2 = end1.Y;
        float x3 = start2.X, y3 = start2.Y;
        float x4 = end2.X, y4 = end2.Y;

        var denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

        if (Math.Abs(denominator) < 1e-10)
            return false; // Lines are parallel

        var ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denominator;
        var ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denominator;

        return ua is >= 0 and <= 1 && ub is >= 0 and <= 1;
    }

    public static Vector2 EdgeToEdgeIntersectionPoint(Frame frame, Edge a, Edge b)
    {
        var (x1, y1) = frame.Vertices[a.Vertices[0]].Coord;
        var (x2, y2) = frame.Vertices[a.Vertices[1]].Coord;
        var (x3, y3) = frame.Vertices[b.Vertices[0]].Coord;
        var (x4, y4) = frame.Vertices[b.Vertices[1]].Coord;

        var denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

        if (Math.Abs(denominator) < 1e-10)
            throw new ArgumentException("Lines cant be parallel");

        var ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denominator;
        var ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denominator;

        if (ua is < 0 or > 1 || ub is < 0 or > 1)
            throw new ArgumentException(
                $"No Intersection Point, CheckInput Edges beforehand using {nameof(EdgesIntersect)}");

        var x0 = x1 + ua * (x2 - x1);
        var y0 = y1 + ua * (y2 - y1);
        return new Vector2(x0, y0);
    }

    public static Vector2 EdgeToEdgeIntersectionPoint(Vertex startA, Vertex endA, Vertex startB, Vertex endB)
    {
        var (x1, y1) = startA.Coord;
        var (x2, y2) = endA.Coord;
        var (x3, y3) = startB.Coord;
        var (x4, y4) = endB.Coord;

        var denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

        if (Math.Abs(denominator) < 1e-10)
            throw new ArgumentException("Lines cant be parallel");

        var ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denominator;
        var ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denominator;

        if (ua is < 0 or > 1 || ub is < 0 or > 1)
            throw new ArgumentException(
                $"No Intersection Point, CheckInput Edges beforehand using {nameof(EdgesIntersect)}");

        var x0 = x1 + ua * (x2 - x1);
        var y0 = y1 + ua * (y2 - y1);
        return new Vector2(x0, y0);
    }

    public static Edge EdgeContainingVertices(Frame frame, Id vertexIdA, Id vertexIdB)
    {
        return frame.Edges.FirstOrDefault(edge =>
            edge.Vertices.Contains(vertexIdA) && edge.Vertices.Contains(vertexIdB));
    }

    public static (bool, Vector2, Vector2) FoldBoardersVertices(Frame frame, Vector2 center, Vector2 perpendicular)
    {
        // collect all cut edges
        var fullEdgeStart = center - perpendicular * 5f;
        var fullEdgeEnd = center + perpendicular * 5f;

        var intersectedEdges = new List<(Vector2, int)>();
        for (var i = 0; i < frame.Edges.Count; i++)
        {
            var edge = frame.Edges[i];
            if (!EdgesIntersect(fullEdgeStart, fullEdgeEnd, frame.Vertices[edge.Vertices[0]].Coord,
                    frame.Vertices[edge.Vertices[1]].Coord)) continue;

            var point = EdgeToEdgeIntersectionPoint(
                new Vertex { Coord = fullEdgeStart },
                new Vertex { Coord = fullEdgeEnd },
                frame.Vertices[edge.Vertices[0]],
                frame.Vertices[edge.Vertices[1]]);
            intersectedEdges.Add((point, i));
        }

        if (intersectedEdges.Count <= 1)
            return (false, fullEdgeStart, fullEdgeEnd);

        // choose the two border edges
        var projection = center.DirectionTo(fullEdgeEnd);
        var sortedIntersectedEdges = intersectedEdges
            .OrderBy(e => (new Vector2(e.Item1.X, e.Item1.Y) - center).Dot(projection))
            .ToList();
        return (true, sortedIntersectedEdges.First().Item1, sortedIntersectedEdges.Last().Item1);
    }

    public static (Vector2 pointA, Edge edgeA, Vector2 pointB, Edge edgeB)? CrossedEdgesOnPolygon(
        Frame frame, Vector2 center, Vector2 perpendicular, Face polyWithCenter)
    {
        var fullEdgeStart = center - perpendicular * 5f;
        var fullEdgeEnd = center + perpendicular * 5f;

        var points = new List<Vector2>(2);
        var edges = new List<Edge>(2);
        var polyEdges = polyWithCenter.Edges();
        for (var i = 0; i < polyEdges.Length; i++)
        {
            var edge = polyEdges[i];
            if (EdgesIntersect(
                    frame.Vertices[edge.Vertices[0]].Coord,
                    frame.Vertices[edge.Vertices[1]].Coord,
                    fullEdgeStart,
                    fullEdgeEnd))
            {
                var point = EdgeToEdgeIntersectionPoint(
                    new Vertex { Coord = fullEdgeStart },
                    new Vertex { Coord = fullEdgeEnd },
                    frame.Vertices[edge.Vertices[0]],
                    frame.Vertices[edge.Vertices[1]]);
                points.Add(point);
                edges.Add(edge);
            }
        }
        return points.Any() ? 
            (points[0], edges[0], points[1], edges[1]) : 
            default((Vector2 pointA, Edge edgeA, Vector2 pointB, Edge edgeB)?);
    }
}
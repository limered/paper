using System.Collections.Generic;
using Godot;
using valleyfold.Fold;

namespace valleyfold.FrameModifications;

public static class VertexQueries
{
    public static Id NearestVertexIdTo(Frame frame, Vector2 point, float threshold = float.MaxValue)
    {
        var nearest = -1;
        var minDist = float.MaxValue;
        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            var dist = point.DistanceSquaredTo(frame.Vertices[i].Coord);
            if (dist > threshold * threshold || dist >= minDist) continue;
            nearest = i;
            minDist = dist;
        }

        return nearest;
    }

    public static List<Id> VerticesCrossedByEdge(Frame frame, Id vertexIdA, Id vertexIdB)
    {
        var crossed = new List<Id>();
        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            if (i == vertexIdA || i == vertexIdB) continue;

            var potentiallyCrossed = frame.Vertices[i];
            var isCrossed = IsPointOnSegment(potentiallyCrossed.Coord, frame.Vertices[vertexIdA].Coord,
                frame.Vertices[vertexIdB].Coord);
            if (isCrossed) crossed.Add(i);
        }

        return crossed;
    }


    private static bool IsPointOnSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var crossProduct = (point.Y - segmentStart.Y) * (segmentEnd.X - segmentStart.X) -
                           (point.X - segmentStart.X) * (segmentEnd.Y - segmentStart.Y);

        if (Mathf.Abs(crossProduct) > Mathf.Epsilon)
            return false;

        var minX = Mathf.Min(segmentStart.X, segmentEnd.X);
        var maxX = Mathf.Max(segmentStart.X, segmentEnd.X);
        var minY = Mathf.Min(segmentStart.Y, segmentEnd.Y);
        var maxY = Mathf.Max(segmentStart.Y, segmentEnd.Y);

        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
    }
}
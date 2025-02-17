using System.Collections.Generic;
using Godot;
using valleyfold.Fold;

namespace valleyfold.FrameModifications;

public static class VertexQueries
{
    public static Id NearestVertexIdTo(Frame frame, Vector3 point, float threshold = float.MaxValue)
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


    private static bool IsPointOnSegment(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
    {
        var crossProduct = (point.Z - segmentStart.Z) * (segmentEnd.X - segmentStart.X) -
                           (point.X - segmentStart.X) * (segmentEnd.Z - segmentStart.Z);

        if (Mathf.Abs(crossProduct) > Mathf.Epsilon)
            return false;

        var minX = Mathf.Min(segmentStart.X, segmentEnd.X);
        var maxX = Mathf.Max(segmentStart.X, segmentEnd.X);
        var minY = Mathf.Min(segmentStart.Z, segmentEnd.Z);
        var maxY = Mathf.Max(segmentStart.Z, segmentEnd.Z);

        return point.X >= minX && point.X <= maxX && point.Z >= minY && point.Z <= maxY;
    }
}
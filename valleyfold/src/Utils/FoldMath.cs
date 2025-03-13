using System;
using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Utils;

public static class FoldMath
{
    public static (Vector2 point, Edge edge)? CalculateEdgeOtherEdgeCrossing(
        Frame frame, Vector2 lineA, Vector2 lineB, Face face, Edge edgeToSkip)
    {
        var faceEdges = face.Edges();
        for (var i = 0; i < faceEdges.Length; i++)
        {
            var edge = faceEdges[i];
            if (edge == edgeToSkip) continue;
            var point = LineSegmentCrossing(
                lineA, lineB,
                frame.Vertices[edge.Vertices[0]].Coord,
                frame.Vertices[edge.Vertices[1]].Coord);

            if (point is null) continue;
            return (point.Value, edge);
        }

        return default;
    }

    public static Vector2? LineSegmentCrossing(
        Vector2 lineA, Vector2 lineB, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var lineDirection = lineB - lineA;
        var segmentDirection = segmentEnd - segmentStart;

        var denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;

        if (Math.Abs(denominator) < float.Epsilon)
            return null;

        var startDiff = new Vector2(segmentStart.X - lineA.X, segmentStart.Y - lineA.Y);
        var t = (startDiff.X * segmentDirection.Y - startDiff.Y * segmentDirection.X) / denominator;
        var s = (startDiff.X * lineDirection.Y - startDiff.Y * lineDirection.X) / denominator;

        if (s is >= 0 and <= 1)
            return new Vector2(
                lineA.X + t * lineDirection.X,
                lineA.Y + t * lineDirection.Y);
        return null;
    }
}
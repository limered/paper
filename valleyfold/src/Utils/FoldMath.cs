using System;
using Godot;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;

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
    
    public static bool AreOnSameSideOfLine(Vector2 lineA, Vector2 lineB, Vector2 pointC, Vector2 pointD)
    {
        var crossC = (pointC.X - lineA.X) * (lineB.Y - lineA.Y) 
                     - (pointC.Y - lineA.Y) * (lineB.X - lineA.X);

        var crossD = (pointD.X - lineA.X) * (lineB.Y - lineA.Y) 
                     - (pointD.Y - lineA.Y) * (lineB.X - lineA.X);

        return (crossC * crossD) >= 0;
    }

    /// <summary>
    /// Rotates <paramref name="vertex"/> around the axis defined by the directed
    /// edge from <paramref name="edgeA"/> to <paramref name="edgeB"/> by
    /// <paramref name="angle"/> radians. The right-hand rule applies relative to
    /// the edge direction.
    /// </summary>
    public static Vector3 RotatedAroundEdge(Vector3 edgeA, Vector3 edgeB, Vector3 vertex, float angle)
    {
        var foldAxis = (edgeB - edgeA).Normalized();
        var relative = vertex - edgeA;
        var rotated = relative.Rotated(foldAxis, angle);
        return edgeA + rotated;
    }
}
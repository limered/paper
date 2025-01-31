
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
}
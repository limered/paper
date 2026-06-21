using Godot;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;

namespace valleyfold.Templates;

/// <summary>
/// Maps a paper-local 2D point to a current 3D position by reading
/// <see cref="Frame3D"/>. Used by the ghost renderer to position a crease
/// line without mutating the graph (unlike
/// <see cref="PaperResolver.ResolveOrCreateVertexAt"/>).
///
/// Strategy: if the point matches an existing vertex, return its 3D coord.
/// Otherwise find a 2D edge that contains the point, compute the
/// parametric t along that edge, and lerp the corresponding 3D endpoints.
/// Falls back to <c>null</c> if neither matches.
/// </summary>
public static class PaperPoint3D
{
    public static Vector3? Resolve(Frame frame, Frame3D frame3d, Vector2 point,
        float tolerance = PaperResolver.DefaultTolerance)
    {
        if (frame == null || frame3d == null) return null;

        var vertexId = PaperResolver.ResolveVertexAt(frame, point, tolerance);
        if (vertexId != -1 && vertexId < frame3d.Vertices.Count)
            return frame3d.Vertices[vertexId].Coord;

        for (var i = 0; i < frame.Edges.Count; i++)
        {
            var edge = frame.Edges[i];
            var aId = edge.Vertices[0];
            var bId = edge.Vertices[1];
            var a2 = frame.Vertices[aId].Coord;
            var b2 = frame.Vertices[bId].Coord;
            var ab = b2 - a2;
            var lenSq = ab.LengthSquared();
            if (lenSq <= 0f) continue;
            var t = (point - a2).Dot(ab) / lenSq;
            if (t < 0f || t > 1f) continue;
            var proj = a2 + ab * t;
            if (proj.DistanceSquaredTo(point) > tolerance * tolerance) continue;
            if (aId >= frame3d.Vertices.Count || bId >= frame3d.Vertices.Count) continue;
            return frame3d.Vertices[aId].Coord.Lerp(frame3d.Vertices[bId].Coord, t);
        }
        return null;
    }
}

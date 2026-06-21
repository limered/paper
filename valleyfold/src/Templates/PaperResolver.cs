using Godot;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Templates;

/// <summary>
/// Maps paper-local 2D coordinates to live <see cref="Frame"/> features.
/// Templates author fold steps against the unfolded paper's coordinate
/// space; this resolver pins those abstract positions to whichever
/// concrete <see cref="Vertex"/>/<see cref="Edge"/> currently occupies
/// the spot after prior folds have rewritten the graph.
///
/// Frame.Vertices.Coord always stores ORIGINAL paper-local coords —
/// folds add new vertices but never move existing ones — so the lookup
/// is on the 2D model directly, not Frame3D.
/// </summary>
public static class PaperResolver
{
    public const float DefaultTolerance = 1e-3f;

    /// <summary>
    /// Nearest vertex within <paramref name="tolerance"/> (Euclidean) of
    /// <paramref name="point"/> in paper-local coords, or -1 if none.
    /// </summary>
    public static Id ResolveVertexAt(Frame frame, Vector2 point, float tolerance = DefaultTolerance)
        => VertexQueries.NearestVertexIdTo(frame, point, tolerance);

    /// <summary>
    /// As <see cref="ResolveVertexAt"/>, but if no vertex matches, finds
    /// an edge whose 2D segment passes through <paramref name="point"/>
    /// and splits it via <see cref="EdgeCommands.AddVertexToEdge"/>,
    /// returning the new vertex id. Returns -1 if neither a vertex nor a
    /// covering edge is found.
    /// </summary>
    public static Id ResolveOrCreateVertexAt(Frame frame, Vector2 point, float tolerance = DefaultTolerance)
    {
        var existing = ResolveVertexAt(frame, point, tolerance);
        if (existing != -1) return existing;

        for (var i = 0; i < frame.Edges.Count; i++)
        {
            var edge = frame.Edges[i];
            var a = frame.Vertices[edge.Vertices[0]].Coord;
            var b = frame.Vertices[edge.Vertices[1]].Coord;
            if (!IsPointOnSegment(point, a, b, tolerance)) continue;
            return EdgeCommands.AddVertexToEdge(frame, point, edge);
        }

        return -1;
    }

    private static bool IsPointOnSegment(Vector2 p, Vector2 a, Vector2 b, float tolerance)
    {
        var ab = b - a;
        var lenSq = ab.LengthSquared();
        if (lenSq <= 0f) return false;
        var t = (p - a).Dot(ab) / lenSq;
        if (t < 0f || t > 1f) return false;
        var proj = a + ab * t;
        return proj.DistanceSquaredTo(p) <= tolerance * tolerance;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.FrameModifications;

public static class FaceQueries
{
    public static Face[] FacesAdjacentToEdge(Frame frame, Edge edge)
    {
        return frame.Faces
            .Where(face => face.Vertices.Contains(edge.Vertices[0]) && face.Vertices.Contains(edge.Vertices[1]))
            .ToArray();
    }

    /// <summary>
    /// BFS over the 2D face graph of <paramref name="frame"/> starting from
    /// <paramref name="anchor"/>. An edge between two faces is traversable iff
    /// its <see cref="Edge.Id"/> is NOT in <paramref name="blockingEdgeIds"/>.
    /// Returns every face reachable from the anchor under that rule
    /// (the anchor itself is always included).
    /// </summary>
    /// <remarks>
    /// Pure over <see cref="Frame"/>: no <c>Statics</c> reads, no Godot runtime
    /// dependencies. See ADR-0001 for the role this plays in selecting the
    /// participating face set during multi-layer fold replay.
    /// </remarks>
    public static Face[] FacesReachableFrom(Frame frame, Face anchor, ISet<Id> blockingEdgeIds)
    {
        var visited = new HashSet<Face> { anchor };
        var queue = new Queue<Face>();
        queue.Enqueue(anchor);

        while (queue.Count > 0)
        {
            var face = queue.Dequeue();
            for (var i = 0; i < frame.Edges.Count; i++)
            {
                var edge = frame.Edges[i];
                if (!face.Vertices.Contains(edge.Vertices[0]) ||
                    !face.Vertices.Contains(edge.Vertices[1])) continue;
                if (blockingEdgeIds.Contains(edge.Id)) continue;

                var adjacent = FacesAdjacentToEdge(frame, edge);
                for (var a = 0; a < adjacent.Length; a++)
                {
                    if (visited.Add(adjacent[a]))
                        queue.Enqueue(adjacent[a]);
                }
            }
        }

        return visited.ToArray();
    }

    /// <summary>
    /// BFS over the 2D face graph anchored at <paramref name="anchor"/>. An
    /// edge is treated as blocking (cannot be traversed) iff its 2D segment
    /// — taken from the XZ projection of <paramref name="vertexPositions3D"/>
    /// at the edge's endpoint vertices — is on opposite sides of, or coincident
    /// with, the line through <paramref name="lineA2d"/> and <paramref name="lineB2d"/>.
    /// </summary>
    /// <remarks>
    /// Pre-split blocking criterion for ADR-0002 §"Face-based picking contract".
    /// The line is treated as infinite (passing through the two given points);
    /// this matches the way <see cref="Utils.FoldMath.LineSegmentCrossing"/>
    /// uses its first two arguments. One-endpoint-on-line is NOT blocking —
    /// the edge then lies entirely on one side, just touching the line.
    /// </remarks>
    public static Face[] FacesReachableFromBlockedByLine(
        Frame frame,
        IReadOnlyList<Vector3> vertexPositions3D,
        Face anchor,
        Vector2 lineA2d,
        Vector2 lineB2d)
    {
        var visited = new HashSet<Face> { anchor };
        var queue = new Queue<Face>();
        queue.Enqueue(anchor);

        while (queue.Count > 0)
        {
            var face = queue.Dequeue();
            for (var i = 0; i < frame.Edges.Count; i++)
            {
                var edge = frame.Edges[i];
                if (!face.Vertices.Contains(edge.Vertices[0]) ||
                    !face.Vertices.Contains(edge.Vertices[1])) continue;

                var p0 = vertexPositions3D[edge.Vertices[0]].Vector2XZ();
                var p1 = vertexPositions3D[edge.Vertices[1]].Vector2XZ();
                if (IsEdgeBlockedByLine(p0, p1, lineA2d, lineB2d)) continue;

                var adjacent = FacesAdjacentToEdge(frame, edge);
                for (var a = 0; a < adjacent.Length; a++)
                {
                    if (visited.Add(adjacent[a]))
                        queue.Enqueue(adjacent[a]);
                }
            }
        }

        return visited.ToArray();
    }

    /// <summary>
    /// Pre-split blocking predicate: an edge segment is "blocked" by the
    /// (infinite) line through <paramref name="lineA"/>—<paramref name="lineB"/>
    /// iff the segment endpoints lie on strictly opposite sides of the line
    /// (line cuts the segment interior), OR both endpoints lie on the line
    /// (segment is collinear with the line, i.e. it IS a crease). One-endpoint-
    /// touch is not blocking — the edge lies on one side, only touching.
    /// </summary>
    public static bool IsEdgeBlockedByLine(Vector2 segStart, Vector2 segEnd, Vector2 lineA, Vector2 lineB)
    {
        const float onLineEpsilon = 1e-6f;
        var sStart = SignedSideOfLine(segStart, lineA, lineB);
        var sEnd = SignedSideOfLine(segEnd, lineA, lineB);

        var startOn = Math.Abs(sStart) < onLineEpsilon;
        var endOn = Math.Abs(sEnd) < onLineEpsilon;

        if (startOn && endOn) return true;   // collinear
        if (startOn || endOn) return false;  // touches at one endpoint only
        return sStart * sEnd < 0f;           // strictly opposite sides
    }

    private static float SignedSideOfLine(Vector2 p, Vector2 a, Vector2 b)
    {
        return (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X);
    }

    /// <summary>
    /// True iff the XY projections of <paramref name="a"/> and <paramref name="b"/>
    /// (XZ in our world space — see <see cref="VectorExtensions.Vector2XZ"/>)
    /// have a positive-area intersection. Edge-touching, vertex-touching, and
    /// disjoint configurations return false. Full containment returns true.
    /// </summary>
    /// <param name="frame">Unused; reserved for graph-aware variants. Kept for
    /// signature stability per the ADR-0002 issue spec.</param>
    /// <param name="a">First face.</param>
    /// <param name="b">Second face.</param>
    /// <param name="vertexPositions3D">3D vertex coordinates indexed by
    /// <see cref="Vertex.Id"/>; the face polygons are read from
    /// <c>face.Vertices</c> against this list.</param>
    /// <remarks>
    /// Hand-rolled Sutherland-Hodgman clipping rather than
    /// <c>Godot.Geometry2D.IntersectPolygons</c> — the latter P/Invokes into the
    /// Godot native runtime, which is not initialised in the xUnit test host
    /// (the editor / scene tree have to be up). Sutherland-Hodgman assumes a
    /// convex clipper, which is safe for M0: any chord through a convex
    /// polygon produces two convex sub-polygons, so faces stay convex
    /// throughout valley-only fold history.
    ///
    /// Area tolerance for "positive area" is <c>1e-6</c> in the XZ unit
    /// square; this filters the zero-area degenerate polygons that
    /// Sutherland-Hodgman produces for edge- and vertex-touching inputs.
    /// </remarks>
    public static bool FacesOverlap(
        Frame frame,
        Face a,
        Face b,
        IReadOnlyList<Vector3> vertexPositions3D)
    {
        _ = frame;

        var polyA = new Vector2[a.Vertices.Count];
        for (var i = 0; i < a.Vertices.Count; i++)
            polyA[i] = vertexPositions3D[a.Vertices[i]].Vector2XZ();

        var polyB = new Vector2[b.Vertices.Count];
        for (var i = 0; i < b.Vertices.Count; i++)
            polyB[i] = vertexPositions3D[b.Vertices[i]].Vector2XZ();

        var clipped = SutherlandHodgmanClip(polyA, polyB);
        if (clipped.Length < 3) return false;

        const float areaEpsilon = 1e-6f;
        return Polygon2dArea(clipped) > areaEpsilon;
    }

    /// <summary>
    /// Sutherland-Hodgman polygon clipping. <paramref name="clipper"/> must be
    /// convex and wound consistently (CCW under standard math coords). The
    /// subject may be any simple polygon; output is the intersection
    /// (subject ∩ clipper-interior).
    /// </summary>
    private static Vector2[] SutherlandHodgmanClip(Vector2[] subject, Vector2[] clipper)
    {
        if (subject.Length < 3 || clipper.Length < 3) return System.Array.Empty<Vector2>();

        // Detect clipper winding and flip the inside test accordingly so the
        // method works regardless of caller convention. Signed area > 0 → CCW.
        var clipperCcw = SignedPolygonArea(clipper) > 0f;

        var output = new List<Vector2>(subject);
        var clipCount = clipper.Length;
        for (var c = 0; c < clipCount; c++)
        {
            if (output.Count == 0) break;
            var input = output.ToArray();
            output.Clear();
            var edgeA = clipper[c];
            var edgeB = clipper[(c + 1) % clipCount];
            var s = input[input.Length - 1];
            var sInside = IsInside(s, edgeA, edgeB, clipperCcw);
            for (var j = 0; j < input.Length; j++)
            {
                var e = input[j];
                var eInside = IsInside(e, edgeA, edgeB, clipperCcw);
                if (eInside)
                {
                    if (!sInside)
                    {
                        var hit = Utils.FoldMath.LineSegmentCrossing(edgeA, edgeB, s, e);
                        if (hit.HasValue) output.Add(hit.Value);
                    }
                    output.Add(e);
                }
                else if (sInside)
                {
                    var hit = Utils.FoldMath.LineSegmentCrossing(edgeA, edgeB, s, e);
                    if (hit.HasValue) output.Add(hit.Value);
                }
                s = e;
                sInside = eInside;
            }
        }
        return output.ToArray();
    }

    private static bool IsInside(Vector2 p, Vector2 edgeA, Vector2 edgeB, bool clipperCcw)
    {
        // Points ON the clipper edge (within a tiny epsilon) count as inside;
        // this keeps Sutherland-Hodgman well-behaved when subject and clipper
        // share an edge or a vertex.
        const float onLineEpsilon = 1e-6f;
        var side = SignedSideOfLine(p, edgeA, edgeB);
        if (System.Math.Abs(side) < onLineEpsilon) return true;
        return clipperCcw ? side < 0f : side > 0f;
    }

    private static float SignedPolygonArea(Vector2[] polygon)
    {
        if (polygon.Length < 3) return 0f;
        var area = 0f;
        var j = polygon.Length - 1;
        for (var i = 0; i < polygon.Length; i++)
        {
            area += (polygon[j].X + polygon[i].X) * (polygon[j].Y - polygon[i].Y);
            j = i;
        }
        // Standard math y-up: shoelace as written gives -2*A for CCW; flip sign.
        return -area * 0.5f;
    }

    private static float Polygon2dArea(Vector2[] polygon)
    {
        if (polygon == null || polygon.Length < 3) return 0f;
        var area = 0f;
        var j = polygon.Length - 1;
        for (var i = 0; i < polygon.Length; i++)
        {
            area += (polygon[j].X + polygon[i].X) * (polygon[j].Y - polygon[i].Y);
            j = i;
        }
        return Math.Abs(area) * 0.5f;
    }

    public static Face FacesContainingVertexIds(Frame frame, List<Id> vertexIds)
    {
        return frame.Faces
            .FirstOrDefault(face => vertexIds.All(face.Vertices.Contains));
    }

    public static Edge[] Edges(this Face face)
    {
        // TODO: sort edges for result
        
        var frame = Statics.Frame;
        var collectedEdges = new List<Edge>();
        for (var i = 0; i < frame.Edges.Count; i++)
        {
            var edge = frame.Edges[i];
            if (face.Vertices.Contains(edge.Vertices[0]) && face.Vertices.Contains(edge.Vertices[1]))
                collectedEdges.Add(edge);
        }

        return collectedEdges.ToArray();
    }

    /// <summary>
    /// Frame-explicit overload of <see cref="Edges(Face)"/>. Prefer in code
    /// that may run without <see cref="Statics.Frame"/> being set (e.g. tests).
    /// </summary>
    public static Edge[] EdgesOf(Frame frame, Face face)
    {
        var collectedEdges = new List<Edge>();
        for (var i = 0; i < frame.Edges.Count; i++)
        {
            var edge = frame.Edges[i];
            if (face.Vertices.Contains(edge.Vertices[0]) && face.Vertices.Contains(edge.Vertices[1]))
                collectedEdges.Add(edge);
        }

        return collectedEdges.ToArray();
    }

    public static Face FaceContainingPoint(Frame frame, Vector2 point)
    {
        for (var i = 0; i < frame.Faces.Count; i++)
        {
            var face = frame.Faces[i];
            if (IsPointInPolygon(frame, point, face.Vertices))
                return face;
        }

        return null;
    }

    public static bool IsPointInPolygon(Frame frame, Vector2 point, List<Id> polygon)
    {
        var isInside = false;
        var j = polygon.Count - 1;

        for (var i = 0; i < polygon.Count; i++)
        {
            var a = frame.Vertices[polygon[i]].Coord;
            var b = frame.Vertices[polygon[j]].Coord;
            if (a.Y > point.Y != b.Y > point.Y && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                isInside = !isInside;
            j = i;
        }

        return isInside;
    }
}
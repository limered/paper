using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.TwoDeeModels;

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
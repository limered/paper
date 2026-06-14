using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Folding;

public class FoldInteractionApplier
{
    /// <summary>
    /// Legacy vertex-drag entry point. Iterates ALL edges of the frame for the
    /// crossing test — produces phantom creases on under-layers when the paper
    /// has already been folded (see ADR-0002, bug 1). Kept functional for the
    /// existing input layer until issue 05-ghost-crease-click migrates it.
    /// </summary>
    [Obsolete("Use ApplyValleyFold(Face, Edge) instead — see ADR-0002. Removal tracked by issue 05-ghost-crease-click.")]
    public static void ApplyVertexValleyFold(Id startVertex, Vector3 endPoint)
        => ApplyVertexFold(startVertex, endPoint, ChangeType.ValleyFold);

    /// <summary>
    /// Legacy vertex-drag entry point for mountain folds. Same caveats as
    /// <see cref="ApplyVertexValleyFold"/> — see ADR-0002 and issue
    /// 05-ghost-crease-click.
    /// </summary>
    [Obsolete("Use ApplyMountainFold(Face, Edge) instead — see ADR-0002. Removal tracked by issue 05-ghost-crease-click.")]
    public static void ApplyVertexMountainFold(Id startVertex, Vector3 endPoint)
        => ApplyVertexFold(startVertex, endPoint, ChangeType.MountainFold);

    private static void ApplyVertexFold(Id startVertex, Vector3 endPoint, ChangeType changeType)
    {
        if (Statics.Frame == null) return;
        if (Statics.FoldAnimator.IsAnimating) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;

        var centerPoint = frame3d.Vertices[startVertex].Coord.Lerp(endPoint, 0.5f);
        var direction = (endPoint - frame3d.Vertices[startVertex].Coord).Normalized();
        var perpendicular = new Vector3(-direction.Z, 0, direction.X);

        var lineA = centerPoint + perpendicular * 100;
        var lineB = centerPoint - perpendicular * 100;

        var lineA2d = lineA.Vector2XZ();
        var lineB2d = lineB.Vector2XZ();

        var addedVertices = new List<Id>();
        var edgeCount = frame.Edges.Count;
        for (var e = 0; e < edgeCount; e++)
        {
            var edge = frame.Edges[e];
            var edgeStart = frame3d.Vertices[edge.Vertices[0]].Coord;
            var edgeEnd = frame3d.Vertices[edge.Vertices[1]].Coord;

            var crossing = FoldMath.LineSegmentCrossing(
                lineA2d,
                lineB2d,
                edgeStart.Vector2XZ(),
                edgeEnd.Vector2XZ());
            if (!crossing.HasValue) continue;

            // calculate t value for point on edge
            var pointDirection = crossing.Value.Vector3XZ() - edgeStart;
            var lineDirection = edgeEnd - edgeStart;
            var t = pointDirection.Dot(lineDirection) / lineDirection.LengthSquared();

            // add new vertices to edge
            var pointOn2dEdge = frame.Vertices[edge.Vertices[0]].Coord.Lerp(frame.Vertices[edge.Vertices[1]].Coord, t);
            var addedVertexId = EdgeCommands.AddVertexToEdge(frame, pointOn2dEdge, edge);
            addedVertices.Add(addedVertexId);
        }

        if (addedVertices.Count == 0) return;

        var addedEdges = new List<Id>();
        for (var v = 0; v < addedVertices.Count; v++)
        {
            var vertexId = addedVertices[v];
            for (var cv = 0; cv < addedVertices.Count; cv++)
            {
                if (v == cv) continue;
                var otherVertexId = addedVertices[cv];
                var faces = frame.Faces.Where(f => f.Vertices.Contains(vertexId) && f.Vertices.Contains(otherVertexId));
                if (!faces.Any()) continue;
                if (frame.Edges.Any(e => e.Vertices.Contains(vertexId) && e.Vertices.Contains(otherVertexId)))
                    continue; // already existing edge

                // ToDo: use different assignments depending on face up direction
                var creaseAssignment = changeType == ChangeType.MountainFold ? Assignment.M : Assignment.V;
                var addedEdge = new VertexToVertexFold(vertexId, otherVertexId, creaseAssignment).Apply(frame);
                addedEdges.Add(addedEdge);
            }
        }

        var changeRecord = new ChangeRecord
        {
            ChangeType = changeType,
            PickedVertex = startVertex,
            FoldLineA = lineA,
            FoldLineB = lineB,
            AddedVertices = addedVertices,
            AddedEdges = addedEdges,
            TargetAngle = (float)Math.PI
        };
        Statics.ChangeMemory.AddChange(changeRecord);

        Statics.FoldAnimator.Start(changeRecord);
    }

    /// <summary>
    /// Face-based valley-fold entry point per ADR-0002. The participating face
    /// set is determined by face-graph BFS anchored at <paramref name="pickedFace"/>
    /// using the ghost-crease's infinite 2D line as the blocking criterion;
    /// edge splits are then restricted to that participating set, avoiding the
    /// phantom-crease bug that the legacy <see cref="ApplyVertexValleyFold"/>
    /// produces on under-layers.
    /// </summary>
    /// <param name="pickedFace">Face the player picked — must already exist in
    /// <c>Statics.Frame.Faces</c>. Recorded as
    /// <see cref="ChangeRecord.PickedFace"/>.</param>
    /// <param name="ghostCrease">Existing edge whose endpoint vertices define
    /// the 3D fold axis (via <c>Frame3D.Vertices</c>) and the 2D crease line
    /// (its XZ projection).</param>
    public static void ApplyValleyFold(Face pickedFace, Edge ghostCrease)
        => ApplyFaceFold(pickedFace, ghostCrease, ChangeType.ValleyFold);

    /// <summary>
    /// Mountain-fold counterpart to <see cref="ApplyValleyFold"/>. Uses the
    /// same face-graph BFS and crease-split path; only the recorded
    /// <see cref="ChangeRecord.ChangeType"/> differs. The renderer negates
    /// the rotation angle internally (see <see cref="valleyfold.Render.ThreeDee.FoldRotation"/>).
    /// </summary>
    public static void ApplyMountainFold(Face pickedFace, Edge ghostCrease)
        => ApplyFaceFold(pickedFace, ghostCrease, ChangeType.MountainFold);

    private static void ApplyFaceFold(Face pickedFace, Edge ghostCrease, ChangeType changeType)
    {
        if (Statics.Frame == null) return;
        if (Statics.FoldAnimator.IsAnimating) return;
        if (pickedFace == null || ghostCrease == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;

        var change = BuildFoldChange(frame, frame3d, pickedFace, ghostCrease, changeType);
        if (change == null) return;

        Statics.ChangeMemory.AddChange(change);
        Statics.FoldAnimator.Start(change);
    }

    /// <summary>
    /// Pure-over-<see cref="Frame"/>/<see cref="Frame3D"/> core of
    /// <see cref="ApplyValleyFold"/>. Mutates the provided <paramref name="frame"/>
    /// (splits crossed edges, adds crease edges) but does not touch
    /// <c>Statics</c> directly except for the unavoidable <c>ChangeMemory</c>
    /// callback inside <see cref="Frame.AddEdgeAfterSplit"/>. Returns the
    /// constructed <see cref="ChangeRecord"/>, or <c>null</c> if no edges
    /// needed splitting (degenerate fold).
    /// </summary>
    public static ChangeRecord BuildValleyFoldChange(
        Frame frame,
        Frame3D frame3d,
        Face pickedFace,
        Edge ghostCrease)
        => BuildFoldChange(frame, frame3d, pickedFace, ghostCrease, ChangeType.ValleyFold);

    /// <summary>
    /// Mountain-fold counterpart to <see cref="BuildValleyFoldChange"/>.
    /// Geometrically identical (same crease, same participating-face BFS,
    /// same edge splits, same added crease edges, same <see cref="ChangeRecord.TargetAngle"/>);
    /// only <see cref="ChangeRecord.ChangeType"/> differs. The renderer
    /// (<see cref="valleyfold.Render.ThreeDee.PaperRenderer"/>) negates the
    /// rotation angle internally for <see cref="ChangeType.MountainFold"/>,
    /// so a mountain fold and a valley fold of the same crease land their
    /// moved vertices as mirror images across the page plane.
    /// </summary>
    public static ChangeRecord BuildMountainFoldChange(
        Frame frame,
        Frame3D frame3d,
        Face pickedFace,
        Edge ghostCrease)
        => BuildFoldChange(frame, frame3d, pickedFace, ghostCrease, ChangeType.MountainFold);

    private static ChangeRecord BuildFoldChange(
        Frame frame,
        Frame3D frame3d,
        Face pickedFace,
        Edge ghostCrease,
        ChangeType changeType)
    {
        var lineA = frame3d.Vertices[ghostCrease.Vertices[0]].Coord;
        var lineB = frame3d.Vertices[ghostCrease.Vertices[1]].Coord;
        var lineA2d = lineA.Vector2XZ();
        var lineB2d = lineB.Vector2XZ();

        var positions = new List<Vector3>(frame3d.Vertices.Count);
        for (var i = 0; i < frame3d.Vertices.Count; i++)
            positions.Add(frame3d.Vertices[i].Coord);

        // Pre-split BFS: every face reachable from picked face without
        // traversing a face-adjacency edge whose 2D segment is crossed by
        // (or collinear with) the ghost-crease line.
        var participatingFaces = FaceQueries.FacesReachableFromBlockedByLine(
            frame, positions, pickedFace, lineA2d, lineB2d);

        // Restrict the edge-crossing/splitting loop to edges of the participating
        // faces. Snapshot the edge IDs up front so new edges added during the
        // loop (from edge splits) are NOT themselves rescanned.
        var participatingEdgeIds = new HashSet<Id>();
        for (var f = 0; f < participatingFaces.Length; f++)
        {
            var face = participatingFaces[f];
            var faceEdges = FaceQueries.EdgesOf(frame, face);
            for (var e = 0; e < faceEdges.Length; e++)
                participatingEdgeIds.Add(faceEdges[e].Id);
        }

        var addedVertices = new List<Id>();
        var edgeIdsToScan = participatingEdgeIds.ToList();
        for (var e = 0; e < edgeIdsToScan.Count; e++)
        {
            var edge = frame.Edges[edgeIdsToScan[e]];
            var edgeStart = frame3d.Vertices[edge.Vertices[0]].Coord;
            var edgeEnd = frame3d.Vertices[edge.Vertices[1]].Coord;

            var crossing = FoldMath.LineSegmentCrossing(
                lineA2d, lineB2d,
                edgeStart.Vector2XZ(), edgeEnd.Vector2XZ());
            if (!crossing.HasValue) continue;

            // Skip vertex-only touches — the line passes through an edge
            // endpoint without splitting the segment interior. Mirrors the
            // "one-endpoint-on-line is NOT blocking" rule in
            // FaceQueries.IsEdgeBlockedByLine.
            var startXz = edgeStart.Vector2XZ();
            var endXz = edgeEnd.Vector2XZ();
            const float endpointEpsilonSq = 1e-12f;
            if (startXz.DistanceSquaredTo(crossing.Value) < endpointEpsilonSq) continue;
            if (endXz.DistanceSquaredTo(crossing.Value) < endpointEpsilonSq) continue;

            var pointDirection = crossing.Value.Vector3XZ() - edgeStart;
            var lineDirection = edgeEnd - edgeStart;
            var lenSq = lineDirection.LengthSquared();
            if (lenSq <= 0f) continue;
            var t = pointDirection.Dot(lineDirection) / lenSq;

            var pointOn2dEdge = frame.Vertices[edge.Vertices[0]].Coord
                .Lerp(frame.Vertices[edge.Vertices[1]].Coord, t);
            var addedVertexId = EdgeCommands.AddVertexToEdge(frame, pointOn2dEdge, edge);
            addedVertices.Add(addedVertexId);
        }

        if (addedVertices.Count == 0) return null;

        var addedEdges = new List<Id>();
        for (var v = 0; v < addedVertices.Count; v++)
        {
            var vertexId = addedVertices[v];
            for (var cv = 0; cv < addedVertices.Count; cv++)
            {
                if (v == cv) continue;
                var otherVertexId = addedVertices[cv];
                var faces = frame.Faces.Where(f =>
                    f.Vertices.Contains(vertexId) && f.Vertices.Contains(otherVertexId));
                if (!faces.Any()) continue;
                if (frame.Edges.Any(e =>
                        e.Vertices.Contains(vertexId) && e.Vertices.Contains(otherVertexId)))
                    continue;
                var creaseAssignment = changeType == ChangeType.MountainFold ? Assignment.M : Assignment.V;
                var addedEdge = new VertexToVertexFold(vertexId, otherVertexId, creaseAssignment).Apply(frame);
                addedEdges.Add(addedEdge);
            }
        }

        // PickedVertex is retained for back-compat with the existing renderer
        // fallback path (per ADR-0002 §"Face-based picking contract"). Pick a
        // vertex ON the ghost crease so the fallback BFS — if it has to run —
        // anchors on the crease line and ADR-0001's anchor-face-independence
        // invariant holds.
        var pickedVertex = ghostCrease.Vertices[0];

        return new ChangeRecord
        {
            ChangeType = changeType,
            PickedFace = pickedFace.Id,
            PickedVertex = pickedVertex,
            FoldLineA = lineA,
            FoldLineB = lineB,
            AddedVertices = addedVertices,
            AddedEdges = addedEdges,
            TargetAngle = (float)Math.PI
        };
    }
}
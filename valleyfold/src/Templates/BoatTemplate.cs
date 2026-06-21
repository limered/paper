using System;
using System.Collections.Generic;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Folding;
using valleyfold.FrameModifications;
using valleyfold.Render.ThreeDee;
using valleyfold.TwoDeeModels;

namespace valleyfold.Templates;

/// <summary>
/// Hardcoded fold sequence for a sailboat-ish silhouette. The traditional
/// origami boat needs petal/reverse folds the M0 engine can't represent,
/// so this is a stand-in: a hull strip mountain-folded behind + two sail
/// triangles valley-folded inward at the top to form a peaked silhouette.
/// Deferred per issue 04: the data-driven template format is MVP work;
/// the shape is in C# now to unblock manual playtest of the M0 engine.
///
/// Sequence (paper-local, unit-square (0,0)-(1,1)):
///   1. Mountain fold at y=0.25 — bottom strip folds behind (hull base).
///   2. Valley fold top-left corner: crease (0, 0.5)\u2013(0.5, 1), forms left sail.
///   3. Valley fold top-right corner: crease (0.5, 1)\u2013(1, 0.5), forms right sail.
///
/// Manual visual check only — no unit test on the sequence itself.
/// </summary>
public static class BoatTemplate
{
    public readonly record struct FoldStep(
        Vector2 LineA,
        Vector2 LineB,
        Vector2 MovingFacePoint,
        ChangeType Direction);

    public static readonly FoldStep[] Steps =
    {
        new(new Vector2(0f, 0.25f), new Vector2(1f, 0.25f),
            new Vector2(0.5f, 0.1f), ChangeType.MountainFold),

        new(new Vector2(0f, 0.5f), new Vector2(0.5f, 1f),
            new Vector2(0.1f, 0.9f), ChangeType.ValleyFold),

        new(new Vector2(0.5f, 1f), new Vector2(1f, 0.5f),
            new Vector2(0.9f, 0.9f), ChangeType.ValleyFold),
    };

    public static bool IsValidStartingFrame(Frame frame)
        => frame != null
           && frame.Vertices.Count == 4
           && frame.Faces.Count == 1
           && frame.Edges.Count == 4;

    /// <summary>
    /// Resolve crease endpoints, split the host face to create the crease
    /// edge, then start the fold animation directly.
    ///
    /// We bypass <see cref="FoldInteractionApplier.ApplyValleyFold"/>
    /// here on purpose: that path expects the ghost crease to cut through
    /// existing edge interiors, which is true for click-drag but false
    /// for a scripted template that names crease endpoints up-front (the
    /// endpoints become vertices first, so by the time BuildFoldChange
    /// runs the line only touches edge endpoints and the splits loop
    /// finds nothing to add → returns null → no animation).
    /// </summary>
    public static bool ApplyStep(FoldStep step)
    {
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        if (frame == null) return false;

        var aId = PaperResolver.ResolveOrCreateVertexAt(frame, step.LineA);
        var bId = PaperResolver.ResolveOrCreateVertexAt(frame, step.LineB);
        if (aId == -1 || bId == -1) return false;

        // Create the crease edge by splitting the face that contains the
        // line. VertexToVertexFold no-ops if the two vertices already share
        // an edge — we handle that by looking the existing edge up.
        var assignment = step.Direction == ChangeType.MountainFold ? Assignment.M : Assignment.V;
        var creaseEdgeId = new VertexToVertexFold(aId, bId, assignment).Apply(frame);
        if (creaseEdgeId == -1)
        {
            var existing = EdgeQueries.EdgeContainingVertices(frame, aId, bId);
            if (existing == null) return false;
            creaseEdgeId = existing.Id;
        }

        // After splitting, the original face is gone; find whichever new
        // face now contains the picked point — that's the moving face.
        var movingFace = FaceQueries.FaceContainingPoint(frame, step.MovingFacePoint);
        if (movingFace == null) return false;

        // Sync Frame3D so the new vertices (and their 3D coords, which
        // reflect all prior folds in the change log) are addressable.
        PaperRenderer.RebuildFrame3D();
        if (aId >= frame3d.Vertices.Count || bId >= frame3d.Vertices.Count) return false;

        var change = new ChangeRecord
        {
            ChangeType = step.Direction,
            PickedFace = movingFace.Id,
            PickedVertex = aId,
            FoldLineA = frame3d.Vertices[aId].Coord,
            FoldLineB = frame3d.Vertices[bId].Coord,
            AddedVertices = new List<Id>(),
            AddedEdges = new List<Id> { creaseEdgeId },
            TargetAngle = (float)Math.PI,
        };
        Statics.ChangeMemory.AddChange(change);
        Statics.FoldAnimator.Start(change);
        return true;
    }
}

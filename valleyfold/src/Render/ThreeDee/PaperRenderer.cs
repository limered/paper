using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.Render.Edges;
using valleyfold.Render.Faces;
using valleyfold.Render.Vertices;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Render.ThreeDee;

public partial class PaperRenderer : Node3D
{
    [Export] public EdgeRendering EdgeRendering;
    [Export] public FaceRendering FaceRendering;
    [Export] public VertexRendering VertexRendering;

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;

        Statics.FoldAnimator.Tick(delta);

        RebuildFrame3D();

        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;

        if (frame3d.Vertices is not null &&
            frame3d.Vertices.Any() &&
            frame3d.Vertices.Count == frame.Vertices.Count)
        {
            var vertexPoints = frame3d.Vertices.Select(v => v.Coord).ToList();
            FaceRendering.RenderFaces(frame.Faces, vertexPoints, frame3d);
            EdgeRendering.Render(frame.Edges, vertexPoints, frame3d);
            VertexRendering.Render(frame3d.Vertices, frame3d);
        }
    }

    /// <summary>
    /// Rebuilds <see cref="Frame3D"/> by replaying all completed folds at their
    /// full <see cref="ChangeRecord.TargetAngle"/>, plus the in-flight fold (if
    /// any) at <see cref="FoldAnimator.EasedProgress"/> times its target angle.
    /// Replay model — see GDD decision Q12 (kept for M-1, replaced at M0).
    /// Per ADR-0002, also resets the per-face layer map at the start and bumps
    /// layers per replayed fold (via <see cref="LayerUpdater.ApplyLayerUpdate"/>).
    ///
    /// Public so pre-renderer <c>_Process</c> consumers of <see cref="Frame3D"/>
    /// (e.g. <c>ThreeDeePaperSelector</c>) can force a sync before reading,
    /// removing the node-order race that prompted the band-aid clamp in
    /// <see cref="Frame3D.ImportMetadataFromFrame"/>. See
    /// <c>.scratch/refactor-frame3d-sync/issues/01-frame3d-auto-sync.md</c>.
    /// </summary>
    public static void RebuildFrame3D()
    {
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        var animator = Statics.FoldAnimator;
        var inFlight = animator.InFlightChange;

        frame3d.ImportFromFrame(frame);
        frame3d.ResetLayers(frame);

        var changes = Statics.ChangeMemory.Changes;
        foreach (var change in changes)
        {
            var isInFlight = ReferenceEquals(change, inFlight);
            // Skip unfolded changes — except the in-flight one, which is
            // mid-animation (unfold ramping down, or refold ramping up
            // before its Unfolded flag flips at completion).
            if ((change.Unfolded && !isInFlight) ||
                change.ChangeType is not (ChangeType.ValleyFold or ChangeType.MountainFold)) continue;

            var baseAngle = isInFlight
                ? animator.EasedProgress * change.TargetAngle
                : change.TargetAngle;
            var angle = FoldRotation.SignedAngle(change, baseAngle);

            ApplyFoldRotation(change, angle);
        }
    }

    private static void ApplyFoldRotation(ChangeRecord change, float angle)
    {
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        var start = change.FoldLineA;
        var end = change.FoldLineB;

        // Anchor: per ADR-0002 prefer the picked face when set; fall back to
        // ADR-0001's "any face containing PickedVertex" rule for legacy
        // records (PickedFace == -1) and for the case where the picked face
        // has since been split out of existence.
        var anchor = FoldAnchorResolver.Resolve(frame, change);
        if (anchor is null) return;

        // The participating face set is everything reachable from the anchor
        // without crossing one of this fold's crease edges. A vertex rotates
        // iff it belongs to at least one participating face.
        var blockingEdges = new HashSet<Id>(change.AddedEdges);
        var participatingFaces = FaceQueries.FacesReachableFrom(frame, anchor, blockingEdges);

        var rotatingVertices = new HashSet<Id>();
        for (var f = 0; f < participatingFaces.Length; f++)
        {
            var face = participatingFaces[f];
            for (var i = 0; i < face.Vertices.Count; i++)
                rotatingVertices.Add(face.Vertices[i]);
        }

        foreach (var vertexId in rotatingVertices)
        {
            var vertex = frame3d.Vertices[vertexId].Coord;
            frame3d.Vertices[vertexId].Coord = FoldMath.RotatedAroundEdge(start, end, vertex, angle);
        }

        // Layer-update rule (ADR-0002 §"Layer-update rule on fold", extended
        // for mountain folds per ADR-0004). Runs post-rotation against the
        // freshly-rotated frame3d.Vertices.
        LayerUpdater.ApplyLayerUpdate(frame, frame3d, participatingFaces, change.ChangeType);
    }
}

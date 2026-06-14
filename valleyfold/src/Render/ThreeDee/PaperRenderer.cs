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
            FaceRendering.RenderFaces(frame.Faces, vertexPoints);
            EdgeRendering.Render(frame.Edges, vertexPoints);
            VertexRendering.Render(frame3d.Vertices);
        }
    }

    /// <summary>
    /// Rebuilds <see cref="Frame3D"/> by replaying all completed folds at their
    /// full <see cref="ChangeRecord.TargetAngle"/>, plus the in-flight fold (if
    /// any) at <see cref="FoldAnimator.EasedProgress"/> times its target angle.
    /// Replay model — see GDD decision Q12 (kept for M-1, replaced at M0).
    /// </summary>
    private static void RebuildFrame3D()
    {
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        var animator = Statics.FoldAnimator;
        var inFlight = animator.InFlightChange;

        frame3d.ImportFromFrame(frame);

        var changes = Statics.ChangeMemory.Changes;
        foreach (var change in changes)
        {
            if (change.Unfolded ||
                change.ChangeType != ChangeType.ValleyFold) continue;

            var angle = ReferenceEquals(change, inFlight)
                ? animator.EasedProgress * change.TargetAngle
                : change.TargetAngle;

            ApplyFoldRotation(change, angle);
        }
    }

    private static void ApplyFoldRotation(ChangeRecord change, float angle)
    {
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        var start = change.FoldLineA;
        var end = change.FoldLineB;

        // Anchor: any face containing the picked vertex. Per ADR-0001, all
        // faces incident to the picked vertex lie on the same side of the new
        // crease, so the choice of anchor among them does not affect the
        // reachable set.
        var anchor = frame.Faces.FirstOrDefault(f => f.Vertices.Contains(change.PickedVertex));
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
    }
}

using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;

namespace valleyfold.ThreeDeeModels;

/// <summary>
/// Implements the layer-update rule for replayed valley and mountain folds.
/// Used by <c>PaperRenderer.EnsureFresh</c> once per replayed
/// <c>ChangeRecord</c>, after the rotation has been applied to
/// <see cref="Frame3D.Vertices"/>.
///
/// Valley rule per ADR-0003 (which supersedes ADR-0002 §"Layer-update rule
/// on fold"): for each face <c>f</c> in the moving set, the new layer is
/// <c>maxStationary + 1 + (maxMoved - layers[f])</c>. This inverts the
/// relative order within the moving stack — physically correct because a
/// 180° rotation around a horizontal axis flips up/down — and places the
/// whole moved stack above the highest overlapping stationary face.
///
/// Mountain rule (mirror, per issue 10-mountain-fold-layer-ordering): the
/// moved stack lands BELOW the lowest overlapping stationary face. The
/// internal inversion still applies (still a 180° rotation):
/// <c>newLayer(f) = minStationary - 1 - (layers[f] - minMoved)</c>.
/// Negative layer indices are valid — the renderer's per-layer Z-nudge
/// handles any integer.
///
/// Pure over <see cref="Frame"/>/<see cref="Frame3D"/>: no <c>Statics</c>
/// reads, no scene-tree dependencies — testable from xUnit.
/// </summary>
public static class LayerUpdater
{
    /// <summary>
    /// Applies the layer-update rule for the supplied fold direction. If
    /// no stationary face overlaps any moved face (under the already-
    /// rotated <see cref="Frame3D.Vertices"/>) the call is a no-op.
    /// </summary>
    public static void ApplyLayerUpdate(
        Frame frame,
        Frame3D frame3d,
        IEnumerable<Face> movingSet,
        ChangeType foldType = ChangeType.ValleyFold)
    {
        var moving = movingSet as IList<Face> ?? movingSet.ToList();
        if (moving.Count == 0) return;

        var positions = new List<Vector3>(frame3d.Vertices.Count);
        for (var i = 0; i < frame3d.Vertices.Count; i++)
            positions.Add(frame3d.Vertices[i].Coord);

        var movingHash = new HashSet<Face>(moving);

        var overlapping = new HashSet<Face>();
        for (var i = 0; i < frame.Faces.Count; i++)
        {
            var stationary = frame.Faces[i];
            if (movingHash.Contains(stationary)) continue;
            for (var m = 0; m < moving.Count; m++)
            {
                if (FaceQueries.FacesOverlap(frame, stationary, moving[m], positions))
                {
                    overlapping.Add(stationary);
                    break;
                }
            }
        }

        if (overlapping.Count == 0) return;

        if (foldType == ChangeType.MountainFold)
        {
            var minStationary = overlapping.Min(f => frame3d.LayerOf(f));
            var minMoved = moving.Min(f => frame3d.LayerOf(f));
            foreach (var face in moving)
            {
                var oldLayer = frame3d.LayerOf(face);
                var newLayer = minStationary - 1 - (oldLayer - minMoved);
                frame3d.SetLayer(face, newLayer);
            }
            return;
        }

        var maxStationary = overlapping.Max(f => frame3d.LayerOf(f));
        var maxMoved = moving.Max(f => frame3d.LayerOf(f));

        // ADR-0003: invert relative order within the moving stack while
        // placing it above the highest overlapping stationary face. The
        // face that was on top of M ends up at the bottom of the new stack
        // position (closest to the stationary paper) — what 180° does.
        foreach (var face in moving)
        {
            var oldLayer = frame3d.LayerOf(face);
            var newLayer = maxStationary + 1 + (maxMoved - oldLayer);
            frame3d.SetLayer(face, newLayer);
        }
    }
}

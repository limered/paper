using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;

namespace valleyfold.ThreeDeeModels;

/// <summary>
/// Implements the layer-update rule for replayed valley folds. Used by
/// <c>PaperRenderer.RebuildFrame3D</c> once per replayed <c>ChangeRecord</c>,
/// after the rotation has been applied to <see cref="Frame3D.Vertices"/>.
///
/// Rule per ADR-0002 §"Layer-update rule on fold":
///   bump = max(layer of any stationary face that overlaps any moved face) + 1
///          - min(layer of any moved face)
/// Then every moved face's layer is shifted by <c>bump</c>. Preserves the
/// relative order within the moving stack.
///
/// Pure over <see cref="Frame"/>/<see cref="Frame3D"/>: no <c>Statics</c>
/// reads, no scene-tree dependencies — testable from xUnit.
/// </summary>
public static class LayerUpdater
{
    /// <summary>
    /// Applies the ADR-0002 layer-update rule. If no stationary face overlaps
    /// any moved face (under the already-rotated <see cref="Frame3D.Vertices"/>)
    /// the call is a no-op.
    /// </summary>
    public static void ApplyLayerUpdate(
        Frame frame,
        Frame3D frame3d,
        IEnumerable<Face> movingSet)
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

        var maxStationary = overlapping.Max(f => frame3d.LayerOf(f));
        var minMoved = moving.Min(f => frame3d.LayerOf(f));
        var bump = maxStationary + 1 - minMoved;

        frame3d.BumpLayers(moving, bump);
    }
}

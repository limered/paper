using System.Linq;
using valleyfold.ChangeTracking;
using valleyfold.TwoDeeModels;

namespace valleyfold.Render.ThreeDee;

/// <summary>
/// Resolves which face the BFS in <c>PaperRenderer.ApplyFoldRotation</c>
/// should anchor at when replaying a <see cref="ChangeRecord"/>.
///
/// Per ADR-0002, the canonical anchor is the picked face (looked up by
/// <see cref="Face.Id"/>). When the picked face has since been split out of
/// existence — or for legacy records (<c>PickedFace == -1</c>) — we fall back
/// to ADR-0001's "any face containing <see cref="ChangeRecord.PickedVertex"/>"
/// rule, which is correct under that ADR's anchor-face-independence invariant
/// when the picked vertex lies on the crease.
///
/// Pure over <see cref="Frame"/>: no <c>Statics</c> reads, no Godot runtime
/// — testable from xUnit.
/// </summary>
public static class FoldAnchorResolver
{
    public static Face Resolve(Frame frame, ChangeRecord change)
    {
        if (change.PickedFace >= 0)
        {
            var byId = frame.Faces.FirstOrDefault(f => f.Id == change.PickedFace);
            if (byId is not null) return byId;
        }
        return frame.Faces.FirstOrDefault(f => f.Vertices.Contains(change.PickedVertex));
    }
}

using valleyfold.ChangeTracking;

namespace valleyfold.Render.ThreeDee;

/// <summary>
/// Maps a <see cref="ChangeRecord"/> + a positive base angle (as produced
/// by the animator from <see cref="ChangeRecord.TargetAngle"/>) onto the
/// signed rotation angle the geometry should use.
///
/// <para>
/// Valley folds rotate "up" (positive angle) around the crease;
/// mountain folds rotate "down" (negative angle) around the same crease.
/// Per issue 03 the renderer is the sole owner of the sign — fold-building
/// code keeps <see cref="ChangeRecord.TargetAngle"/> positive regardless
/// of direction so that the animator's eased-progress maths is unaffected.
/// </para>
/// </summary>
public static class FoldRotation
{
    public static float SignedAngle(ChangeRecord change, float baseAngle)
        => change.ChangeType == ChangeType.MountainFold ? -baseAngle : baseAngle;
}

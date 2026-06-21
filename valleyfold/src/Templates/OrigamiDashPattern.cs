using System.Collections.Generic;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.TwoDeeModels;

namespace valleyfold.Templates;

/// <summary>
/// Splits a single 3D line segment into the alternating ink/gap dashes of
/// the Yoshizawa–Randlett origami diagram convention:
///   • Valley fold — long dashes.
///   • Mountain fold — dash-dot-dot.
///   • Unfold / unspecified — solid line.
///
/// Output is a list of (start, end) world segments ready to be fed to
/// <see cref="Render.Edges.EdgeLine.AddPositions"/>. Pure geometry.
/// </summary>
public static class OrigamiDashPattern
{
    public const float DefaultUnitLength = 0.04f;

    // Pattern entries: positive = ink length, negative = gap length, both
    // in multiples of the unit length.
    private static readonly float[] ValleyPattern = { 1.0f, -0.5f };
    private static readonly float[] MountainPattern = { 1.5f, -0.4f, 0.2f, -0.4f, 0.2f, -0.4f };

    public static List<(Vector3 Start, Vector3 End)> Build(
        Vector3 a, Vector3 b, ChangeType kind, float unitLength = DefaultUnitLength)
    {
        var result = new List<(Vector3, Vector3)>();
        var pattern = PatternFor(kind);
        var total = a.DistanceTo(b);
        if (total <= 0f || unitLength <= 0f) return result;

        // No pattern (solid): emit the whole segment.
        if (pattern == null)
        {
            result.Add((a, b));
            return result;
        }

        var dir = (b - a) / total;
        var cursor = 0f;
        var i = 0;
        while (cursor < total)
        {
            var entry = pattern[i % pattern.Length] * unitLength;
            var span = Mathf.Min(Mathf.Abs(entry), total - cursor);
            if (entry > 0f)
            {
                result.Add((a + dir * cursor, a + dir * (cursor + span)));
            }
            cursor += span;
            i++;
        }
        return result;
    }

    private static float[] PatternFor(ChangeType kind) => kind switch
    {
        ChangeType.ValleyFold => ValleyPattern,
        ChangeType.MountainFold => MountainPattern,
        _ => null,
    };

    /// <summary>
    /// Build dashes for an edge by its <see cref="Assignment"/>:
    /// V → valley, M → mountain, everything else → solid (paper border
    /// and undecorated creases).
    /// </summary>
    public static List<(Vector3 Start, Vector3 End)> BuildForAssignment(
        Vector3 a, Vector3 b, Assignment assignment, float unitLength = DefaultUnitLength)
    {
        return assignment switch
        {
            Assignment.V => Build(a, b, ChangeType.ValleyFold, unitLength),
            Assignment.M => Build(a, b, ChangeType.MountainFold, unitLength),
            _ => Build(a, b, ChangeType.Unfold, unitLength),
        };
    }
}

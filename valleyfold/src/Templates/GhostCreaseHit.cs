using Godot;

namespace valleyfold.Templates;

/// <summary>
/// Click-vs-ghost-crease proximity test. The ghost is a 3D line segment
/// drawn on top of the paper; a click registers if the picked 3D point
/// (from the paper's collision Area3D) is within <paramref name="tolerance"/>
/// of the segment, measured as 3D point-to-segment distance with the
/// closest point clamped to the segment endpoints.
/// </summary>
public static class GhostCreaseHit
{
    public static bool IsNear(Vector3 point, Vector3 a, Vector3 b, float tolerance)
    {
        var ab = b - a;
        var lenSq = ab.LengthSquared();
        Vector3 closest;
        if (lenSq <= 0f)
        {
            closest = a;
        }
        else
        {
            var t = Mathf.Clamp((point - a).Dot(ab) / lenSq, 0f, 1f);
            closest = a + ab * t;
        }
        return closest.DistanceSquaredTo(point) <= tolerance * tolerance;
    }
}

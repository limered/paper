using Godot;
using valleyfold.Templates;
using Xunit;

namespace Testing.Templates;

public class GhostCreaseHitTests
{
    private static readonly Vector3 A = new(0, 0, 0);
    private static readonly Vector3 B = new(1, 0, 0);
    private const float Tol = 0.1f;

    [Fact]
    public void ExactlyOnEndpointHits()
    {
        Assert.True(GhostCreaseHit.IsNear(A, A, B, Tol));
        Assert.True(GhostCreaseHit.IsNear(B, A, B, Tol));
    }

    [Fact]
    public void ExactlyOnMidpointHits()
    {
        Assert.True(GhostCreaseHit.IsNear(new Vector3(0.5f, 0, 0), A, B, Tol));
    }

    [Fact]
    public void PerpendicularOffsetWithinToleranceHits()
    {
        Assert.True(GhostCreaseHit.IsNear(new Vector3(0.5f, 0.05f, 0), A, B, Tol));
    }

    [Fact]
    public void PerpendicularOffsetBeyondToleranceMisses()
    {
        Assert.False(GhostCreaseHit.IsNear(new Vector3(0.5f, 0.5f, 0), A, B, Tol));
    }

    [Fact]
    public void OffsetInOtherAxisAlsoCounts()
    {
        // Distance measured in 3D — y or z offset both contribute.
        Assert.False(GhostCreaseHit.IsNear(new Vector3(0.5f, 0, 0.5f), A, B, Tol));
    }

    [Fact]
    public void PointPastEndpointIsMeasuredFromEndpointNotInfiniteLine()
    {
        // (2, 0, 0) lies on the infinite line through A-B but is distance 1
        // from the nearest endpoint B — must miss for any tolerance < 1.
        Assert.False(GhostCreaseHit.IsNear(new Vector3(2, 0, 0), A, B, Tol));
    }

    [Fact]
    public void PointBeforeStartIsMeasuredFromStart()
    {
        Assert.False(GhostCreaseHit.IsNear(new Vector3(-2, 0, 0), A, B, Tol));
    }

    [Fact]
    public void ZeroLengthSegmentTreatedAsPoint()
    {
        // Degenerate but a real possibility if the resolver collapses; the
        // closest distance should fall back to "distance to A".
        Assert.True(GhostCreaseHit.IsNear(A, A, A, Tol));
        Assert.False(GhostCreaseHit.IsNear(new Vector3(1, 0, 0), A, A, Tol));
    }
}

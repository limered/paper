using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Templates;
using valleyfold.TwoDeeModels;
using Xunit;

namespace Testing.Templates;

public class OrigamiDashPatternTests
{
    private static readonly Vector3 A = new(0, 0, 0);
    private static readonly Vector3 B = new(1, 0, 0);

    [Fact]
    public void ValleyEmitsAtLeastTwoDashes()
    {
        var dashes = OrigamiDashPattern.Build(A, B, ChangeType.ValleyFold, 0.1f);
        Assert.True(dashes.Count >= 2);
    }

    [Fact]
    public void MountainProducesMoreSegmentsThanValleyAtSameLength()
    {
        // Dash-dot-dot has three ink entries per cycle vs valley's one,
        // so a fixed-length segment must contain more sub-segments.
        var valley = OrigamiDashPattern.Build(A, B, ChangeType.ValleyFold, 0.1f);
        var mountain = OrigamiDashPattern.Build(A, B, ChangeType.MountainFold, 0.1f);
        Assert.True(mountain.Count > valley.Count);
    }

    [Fact]
    public void UnspecifiedFoldIsSolid()
    {
        // Anything that isn't M or V (e.g. F / unfold preview) renders as
        // a single full-length segment.
        var dashes = OrigamiDashPattern.Build(A, B, ChangeType.Unfold, 0.1f);
        Assert.Single(dashes);
        Assert.Equal(A, dashes[0].Start);
        Assert.Equal(B, dashes[0].End);
    }

    [Fact]
    public void AllSubSegmentsLieOnTheLine()
    {
        var dashes = OrigamiDashPattern.Build(A, B, ChangeType.MountainFold, 0.1f);
        foreach (var (s, e) in dashes)
        {
            // y and z stay zero; start must come before end along x.
            Assert.Equal(0f, s.Y);
            Assert.Equal(0f, e.Y);
            Assert.True(s.X >= 0f && e.X <= 1f);
            Assert.True(s.X <= e.X);
        }
    }

    [Fact]
    public void LastDashClampsAtEndpoint()
    {
        // Final emitted dash must not extend past B.
        var dashes = OrigamiDashPattern.Build(A, B, ChangeType.ValleyFold, 0.13f);
        var last = dashes[^1];
        Assert.True(last.End.X <= 1f + 1e-5f);
    }

    [Fact]
    public void DegenerateSegmentProducesNothing()
    {
        var dashes = OrigamiDashPattern.Build(A, A, ChangeType.ValleyFold);
        Assert.Empty(dashes);
    }

    [Fact]
    public void BorderAssignmentRendersAsSolidLine()
    {
        var dashes = OrigamiDashPattern.BuildForAssignment(A, B, Assignment.B, 0.1f);
        Assert.Single(dashes);
        Assert.Equal(A, dashes[0].Start);
        Assert.Equal(B, dashes[0].End);
    }

    [Fact]
    public void ValleyAssignmentMatchesValleyChangeTypeDashes()
    {
        var byAssignment = OrigamiDashPattern.BuildForAssignment(A, B, Assignment.V, 0.1f);
        var byChange = OrigamiDashPattern.Build(A, B, ChangeType.ValleyFold, 0.1f);
        Assert.Equal(byChange.Count, byAssignment.Count);
    }

    [Fact]
    public void MountainAssignmentMatchesMountainChangeTypeDashes()
    {
        var byAssignment = OrigamiDashPattern.BuildForAssignment(A, B, Assignment.M, 0.1f);
        var byChange = OrigamiDashPattern.Build(A, B, ChangeType.MountainFold, 0.1f);
        Assert.Equal(byChange.Count, byAssignment.Count);
    }

    [Fact]
    public void UnspecifiedAssignmentsRenderSolid()
    {
        // F (unfolded) and U (unspecified) have no current mountain/valley
        // semantics — render solid rather than guessing a pattern.
        Assert.Single(OrigamiDashPattern.BuildForAssignment(A, B, Assignment.F, 0.1f));
        Assert.Single(OrigamiDashPattern.BuildForAssignment(A, B, Assignment.U, 0.1f));
    }
}

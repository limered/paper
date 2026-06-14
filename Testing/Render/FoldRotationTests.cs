using System;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Render.ThreeDee;
using valleyfold.TwoDeeModels;

namespace Testing.Render;

public class FoldRotationTests
{
    // Pure helper: converts a positive base angle (as computed by the
    // animator/renderer from ChangeRecord.TargetAngle) into the signed
    // rotation angle the geometry should actually use. Valley folds pass
    // through unchanged; mountain folds negate. Unfold isn't a folding
    // change and is never passed in.

    [Fact]
    public void ValleyFoldPreservesPositiveAngle()
    {
        var change = new ChangeRecord
        {
            ChangeType = ChangeType.ValleyFold,
            TargetAngle = (float)Math.PI
        };

        Assert.Equal((float)Math.PI, FoldRotation.SignedAngle(change, (float)Math.PI));
    }

    [Fact]
    public void MountainFoldNegatesAngle()
    {
        var change = new ChangeRecord
        {
            ChangeType = ChangeType.MountainFold,
            TargetAngle = (float)Math.PI
        };

        Assert.Equal(-(float)Math.PI, FoldRotation.SignedAngle(change, (float)Math.PI));
    }
}

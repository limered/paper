using Godot;

namespace Testing.Utils;

public static class AssertUtils
{
    public static void InRangeVector2(Vector2 actual, Vector2 expectedMin, Vector2 expectedMax)
    {
        Assert.InRange(actual.X, expectedMin.X, expectedMax.X);
        Assert.InRange(actual.Y, expectedMin.Y, expectedMax.Y);
    }
    
    public static void EquivalentWithEpsilon(float actual, float expected, float epsilon = 0.0001f)
    {
        Assert.InRange(actual, expected - epsilon, expected + epsilon);
    }
    
    public static void EquivalentWithEpsilonVector2(Vector2 actual, Vector2 expected, float epsilon = 0.0001f)
    {
        EquivalentWithEpsilon(actual.X, expected.X, epsilon);
        EquivalentWithEpsilon(actual.Y, expected.Y, epsilon);
    }
}
using System;
using Godot;
using Testing.Utils;
using valleyfold.Utils;

namespace Testing.Utils;

public class RotatedAroundEdgeTests
{
    // The paper lies in the XZ plane (Y = 0). Folds rotate vertices around an
    // edge that also lies in the XZ plane. With angle = π, a vertex on one
    // side of the edge ends up at its mirror position on the other side
    // (still at Y = 0). With angle = π/2 it ends up directly above the edge
    // at Y equal to its perpendicular distance to the edge.

    public class AroundXAxisEdge : RotatedAroundEdgeTests
    {
        // Edge from (0,0,0) to (1,0,0) — runs along +X in the XZ plane.
        // A vertex at (0,0,1) is perpendicular distance 1 in +Z.
        private static readonly Vector3 EdgeA = new(0, 0, 0);
        private static readonly Vector3 EdgeB = new(1, 0, 0);
        private static readonly Vector3 Vertex = new(0, 0, 1);

        [Fact]
        public void AngleZero_LeavesVertexUnchanged()
        {
            var result = FoldMath.RotatedAroundEdge(EdgeA, EdgeB, Vertex, 0f);
            AssertUtils.EquivalentWithEpsilonVector3(result, Vertex);
        }

        [Fact]
        public void AngleHalfPi_LiftsVertexAboveTheEdge()
        {
            var result = FoldMath.RotatedAroundEdge(EdgeA, EdgeB, Vertex, (float)(Math.PI / 2));
            // Rotated 90° around +X using right-hand rule: (0,0,1) -> (0,-1,0).
            AssertUtils.EquivalentWithEpsilonVector3(result, new Vector3(0, -1, 0));
        }

        [Fact]
        public void AnglePi_ReflectsVertexAcrossTheEdge()
        {
            var result = FoldMath.RotatedAroundEdge(EdgeA, EdgeB, Vertex, (float)Math.PI);
            // Rotated 180° around +X: (0,0,1) -> (0,0,-1).
            AssertUtils.EquivalentWithEpsilonVector3(result, new Vector3(0, 0, -1));
        }
    }

    public class MatchesPreviousSnapBehaviour : RotatedAroundEdgeTests
    {
        // The previous PaperRenderer.ReflectedAroundLine called
        // Vector3.Rotated(axis, π). RotatedAroundEdge at angle π must produce
        // identical results so existing visual behaviour is preserved.
        [Fact]
        public void OnDiagonalEdgeOfUnitSquare()
        {
            // Diagonal of the unit square in the XZ plane, from (0,0,0) to (1,0,1).
            var edgeA = new Vector3(0, 0, 0);
            var edgeB = new Vector3(1, 0, 1);
            var vertex = new Vector3(1, 0, 0); // corner on one side of the diagonal

            var result = FoldMath.RotatedAroundEdge(edgeA, edgeB, vertex, (float)Math.PI);

            // Reflecting (1,0,0) across the line y=x in the XZ plane gives (0,0,1).
            AssertUtils.EquivalentWithEpsilonVector3(result, new Vector3(0, 0, 1));
        }
    }
}

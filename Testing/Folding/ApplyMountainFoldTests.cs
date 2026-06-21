using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Folding;
using valleyfold.Render.ThreeDee;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace Testing.Folding;

public class ApplyMountainFoldTests
{
    // Mirror of ApplyValleyFoldTests for the mountain entry point. Uses the
    // same two-rectangle fixture so equivalence with the valley path can be
    // asserted directly.

    public class BuildMountainFoldChange : ApplyMountainFoldTests
    {
        private readonly Frame _frame;
        private readonly Frame3D _frame3d;
        private readonly Face _faceA;
        private readonly Edge _ghostCrease;

        public BuildMountainFoldChange()
        {
            _frame = new Frame();

            _frame.Vertices.AddRange(new[]
            {
                new Vertex { Coord = new Vector2(0, 0), Id = 0 },
                new Vertex { Coord = new Vector2(1, 0), Id = 1 },
                new Vertex { Coord = new Vector2(1, 2), Id = 2 },
                new Vertex { Coord = new Vector2(0, 2), Id = 3 },
                new Vertex { Coord = new Vector2(1.5f, 0.5f), Id = 4 },
                new Vertex { Coord = new Vector2(2, 0.5f),    Id = 5 },
                new Vertex { Coord = new Vector2(2, 1.5f),    Id = 6 },
                new Vertex { Coord = new Vector2(1.5f, 1.5f), Id = 7 },
                new Vertex { Coord = new Vector2(-1, 1), Id = 8 },
                new Vertex { Coord = new Vector2(3, 1),  Id = 9 }
            });

            _frame.Edges.AddRange(new[]
            {
                new Edge { Vertices = new Id[] { 0, 1 }, Id = 0, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 1, 2 }, Id = 1, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 2, 3 }, Id = 2, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 3, 0 }, Id = 3, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 4, 5 }, Id = 4, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 5, 6 }, Id = 5, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 6, 7 }, Id = 6, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 7, 4 }, Id = 7, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 8, 9 }, Id = 8, Assignment = Assignment.B }
            });

            _faceA = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2, 3 } };
            var faceB = new Face { Id = 1, Vertices = new List<Id> { 4, 5, 6, 7 } };
            _frame.Faces.AddRange(new[] { _faceA, faceB });

            _ghostCrease = _frame.Edges[8];

            _frame3d = new Frame3D();
            for (var i = 0; i < _frame.Vertices.Count; i++)
            {
                var v2d = _frame.Vertices[i].Coord;
                _frame3d.Vertices.Add(new Vertex3D
                {
                    Coord = new Vector3(v2d.X, 0f, v2d.Y)
                });
            }
        }

        [Fact]
        public void RecordsMountainChangeType()
        {
            var change = FoldInteractionApplier.BuildMountainFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            Assert.Equal(ChangeType.MountainFold, change.ChangeType);
        }

        [Fact]
        public void SplitsSameEdgesAsValleyFold()
        {
            // Mountain folds reuse the valley plumbing — same crease,
            // same participating-face BFS, same edge splits. Asserting the
            // concrete split positions (matching ApplyValleyFoldTests
            // .SplitsCreatedOnlyOnFaceABorderEdges) keeps the contract
            // pinned without coupling to the implementation.
            var change = FoldInteractionApplier.BuildMountainFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            var newPositions = change.AddedVertices
                .Select(id => _frame.Vertices[id].Coord)
                .OrderBy(c => c.X).ThenBy(c => c.Y)
                .ToArray();

            Assert.Equal(2, newPositions.Length);
            Assert.Equal(new Vector2(0f, 1f), newPositions[0]);
            Assert.Equal(new Vector2(1f, 1f), newPositions[1]);
            Assert.Single(change.AddedEdges);
        }

        [Fact]
        public void MirrorsValleyFoldAcrossPagePlane()
        {
            // Pipeline-level mirror property: applying the renderer's
            // signed angle (mountain → -π, valley → +π) to the same
            // moving vertex around the same crease must produce two
            // points whose X and Z coords are equal and whose Y coords
            // are opposite. This is the spec AC #5 ("mirror images
            // across the page plane").

            var mountain = FoldInteractionApplier.BuildMountainFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            // Pick any vertex on the moving side of face A — vertex 2 at
            // (1, 0, 2) in 3D, well above the ghost-crease line z=1.
            var movingVertex = _frame3d.Vertices[2].Coord;

            // Use a mid-fold angle (π/2). At the flat end-state of π
            // a point that started on the page plane lands back on it
            // (Y ≈ 0) for both directions — degenerate for an
            // above/below assertion. Mid-animation the renderer passes
            // EasedProgress * TargetAngle ∈ (0, π) into the helper, so
            // π/2 is a realistic invocation.
            const float baseAngle = (float)(System.Math.PI / 2);
            var valleyAngle = FoldRotation.SignedAngle(
                new ChangeRecord { ChangeType = ChangeType.ValleyFold }, baseAngle);
            var mountainAngle = FoldRotation.SignedAngle(mountain, baseAngle);

            var valleyRotated = FoldMath.RotatedAroundEdge(
                mountain.FoldLineA, mountain.FoldLineB, movingVertex, valleyAngle);
            var mountainRotated = FoldMath.RotatedAroundEdge(
                mountain.FoldLineA, mountain.FoldLineB, movingVertex, mountainAngle);

            const float tol = 1e-5f;
            Assert.InRange(mountainRotated.X - valleyRotated.X, -tol, tol);
            Assert.InRange(mountainRotated.Z - valleyRotated.Z, -tol, tol);
            Assert.InRange(mountainRotated.Y + valleyRotated.Y, -tol, tol);
            // The existing FoldMath convention (see RotatedAroundEdgeTests
            // .AroundXAxisEdge.AngleHalfPi_LiftsVertexAboveTheEdge) is that
            // a positive angle around an X-aligned axis sends a point at
            // +Z to -Y — i.e. the moving side dips BELOW the page mid-
            // rotation for a valley fold. Mountain, with the negated
            // angle, lifts the moving side ABOVE the page. This is AC #4.
            Assert.True(valleyRotated.Y < -tol, $"valley Y was {valleyRotated.Y}");
            Assert.True(mountainRotated.Y > tol, $"mountain Y was {mountainRotated.Y}");
        }
    }
}

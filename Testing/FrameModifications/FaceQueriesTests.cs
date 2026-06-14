using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Folding;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;

namespace Testing.FrameModifications;

public class FaceQueriesTests
{
    public class SingleDiagonalCrease : FaceQueriesTests
    {
        // Unfolded square (InitializePaper) split by a single diagonal crease
        // from vertex 0 (0,0) to vertex 2 (1,1). After the fold:
        //   Faces[0] = [2, 3, 0]   (top-left triangle)
        //   Faces[1] = [0, 1, 2]   (bottom-right triangle)
        // The diagonal crease has Edge.Id == 4 (returned by Apply).
        private readonly Frame _frame;
        private readonly Id _diagonalEdgeId;

        public SingleDiagonalCrease()
        {
            _frame = new Frame();
            _frame.InitializePaper();
            _diagonalEdgeId = new VertexToVertexFold(0, 2).Apply(_frame);
        }

        [Fact]
        public void BfsBlockedByCrease_FromBottomRightTriangle_ReturnsOnlyThatFace()
        {
            var bottomRight = _frame.Faces.First(f =>
                f.Vertices.Contains((Id)0) &&
                f.Vertices.Contains((Id)1) &&
                f.Vertices.Contains((Id)2));
            var blocking = new HashSet<Id> { _diagonalEdgeId };

            var reachable = FaceQueries.FacesReachableFrom(_frame, bottomRight, blocking);

            Assert.Single(reachable);
            Assert.Same(bottomRight, reachable[0]);
        }

        [Fact]
        public void BfsBlockedByCrease_FromTopLeftTriangle_ReturnsOnlyThatFace()
        {
            var topLeft = _frame.Faces.First(f =>
                f.Vertices.Contains((Id)2) &&
                f.Vertices.Contains((Id)3) &&
                f.Vertices.Contains((Id)0));
            var blocking = new HashSet<Id> { _diagonalEdgeId };

            var reachable = FaceQueries.FacesReachableFrom(_frame, topLeft, blocking);

            Assert.Single(reachable);
            Assert.Same(topLeft, reachable[0]);
        }
    }

    public class TwoPerpendicularCreases : FaceQueriesTests
    {
        // Manually constructed unit square split into four quadrants by a
        // horizontal and a vertical crease meeting at the centre. Each crease
        // is two edge-segments (split by the centre vertex), mirroring how
        // ChangeMemory.AddEdgeToExistingChange would record a crease that
        // gets subdivided by a later fold.
        //
        //   3 ------ 6 ------ 2
        //   |        |        |
        //   |   F3   |   F2   |
        //   |        |        |
        //   7 ------ 8 ------ 5
        //   |        |        |
        //   |   F0   |   F1   |
        //   |        |        |
        //   0 ------ 4 ------ 1
        //
        // Edges 8, 9 = vertical crease (lower, upper).
        // Edges 10, 11 = horizontal crease (left, right).
        protected readonly Frame _frame;
        protected readonly Id _verticalCreaseLowerId = 8;
        protected readonly Id _verticalCreaseUpperId = 9;
        protected readonly Id _horizontalCreaseLeftId = 10;
        protected readonly Id _horizontalCreaseRightId = 11;
        protected readonly Face _bottomLeft;
        protected readonly Face _bottomRight;
        protected readonly Face _topRight;
        protected readonly Face _topLeft;

        protected TwoPerpendicularCreases()
        {
            _frame = new Frame();

            _frame.Vertices.AddRange(new[]
            {
                new Vertex { Coord = new Vector2(0, 0), Id = 0 },
                new Vertex { Coord = new Vector2(1, 0), Id = 1 },
                new Vertex { Coord = new Vector2(1, 1), Id = 2 },
                new Vertex { Coord = new Vector2(0, 1), Id = 3 },
                new Vertex { Coord = new Vector2(0.5f, 0), Id = 4 },
                new Vertex { Coord = new Vector2(1, 0.5f), Id = 5 },
                new Vertex { Coord = new Vector2(0.5f, 1), Id = 6 },
                new Vertex { Coord = new Vector2(0, 0.5f), Id = 7 },
                new Vertex { Coord = new Vector2(0.5f, 0.5f), Id = 8 }
            });

            _frame.Edges.AddRange(new[]
            {
                new Edge { Vertices = new Id[] { 0, 4 }, Id = 0, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 4, 1 }, Id = 1, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 1, 5 }, Id = 2, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 5, 2 }, Id = 3, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 2, 6 }, Id = 4, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 6, 3 }, Id = 5, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 3, 7 }, Id = 6, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 7, 0 }, Id = 7, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 4, 8 }, Id = 8, Assignment = Assignment.V },
                new Edge { Vertices = new Id[] { 8, 6 }, Id = 9, Assignment = Assignment.V },
                new Edge { Vertices = new Id[] { 7, 8 }, Id = 10, Assignment = Assignment.V },
                new Edge { Vertices = new Id[] { 8, 5 }, Id = 11, Assignment = Assignment.V }
            });

            _bottomLeft = new Face { Id = 0, Vertices = new List<Id> { 0, 4, 8, 7 } };
            _bottomRight = new Face { Id = 1, Vertices = new List<Id> { 4, 1, 5, 8 } };
            _topRight = new Face { Id = 2, Vertices = new List<Id> { 8, 5, 2, 6 } };
            _topLeft = new Face { Id = 3, Vertices = new List<Id> { 7, 8, 6, 3 } };

            _frame.Faces.AddRange(new[] { _bottomLeft, _bottomRight, _topRight, _topLeft });
        }

        public class BfsBlockedByOneCrease : TwoPerpendicularCreases
        {
            [Fact]
            public void FromBottomLeft_ReturnsBothBottomQuadrants()
            {
                // Block the horizontal crease only. BFS from the bottom-left
                // quadrant must be able to cross the vertical crease (which is
                // not blocking) into the bottom-right quadrant, but must stop
                // at the horizontal crease before reaching either top quadrant.
                var blocking = new HashSet<Id> { _horizontalCreaseLeftId, _horizontalCreaseRightId };

                var reachable = FaceQueries.FacesReachableFrom(_frame, _bottomLeft, blocking);

                Assert.Equal(2, reachable.Length);
                Assert.Contains(_bottomLeft, reachable);
                Assert.Contains(_bottomRight, reachable);
                Assert.DoesNotContain(_topLeft, reachable);
                Assert.DoesNotContain(_topRight, reachable);
            }

            [Fact]
            public void BfsCrossesTheNonBlockingCrease()
            {
                // Same setup as above, restated to make the "BFS does cross the
                // other, non-blocking crease" invariant explicit. _bottomRight
                // is reachable from _bottomLeft only by crossing the vertical
                // crease (edges 8, 9).
                var blocking = new HashSet<Id> { _horizontalCreaseLeftId, _horizontalCreaseRightId };

                var reachable = FaceQueries.FacesReachableFrom(_frame, _bottomLeft, blocking);

                Assert.Contains(_bottomRight, reachable);
            }
        }

        public class AnchorFaceIndependence : TwoPerpendicularCreases
        {
            [Fact]
            public void AnyFaceContainingPickedVertex_YieldsSameReachableSet()
            {
                // Picked vertex 4 (bottom edge midpoint) is contained by the
                // bottom-left and bottom-right quadrants. Both lie on the same
                // side of the (blocked) horizontal crease, so anchoring at
                // either must yield the same reachable set.
                var pickedVertex = (Id)4;
                var anchors = _frame.Faces
                    .Where(f => f.Vertices.Contains(pickedVertex))
                    .ToList();
                var blocking = new HashSet<Id> { _horizontalCreaseLeftId, _horizontalCreaseRightId };

                Assert.Equal(2, anchors.Count); // sanity: precondition for the test

                var fromFirst = new HashSet<Face>(
                    FaceQueries.FacesReachableFrom(_frame, anchors[0], blocking));
                var fromSecond = new HashSet<Face>(
                    FaceQueries.FacesReachableFrom(_frame, anchors[1], blocking));

                Assert.True(fromFirst.SetEquals(fromSecond));
                Assert.Equal(2, fromFirst.Count);
            }
        }
    }

    public class FacesOverlap : FaceQueriesTests
    {
        // ADR-0002 requires positive-area intersection — edge- and vertex-
        // touches must NOT count as overlap. These tests pin that contract.

        private static Frame EmptyFrame() => new();

        private static IReadOnlyList<Vector3> Positions(params (float x, float z)[] xs)
        {
            var list = new List<Vector3>(xs.Length);
            foreach (var (x, z) in xs)
                list.Add(new Vector3(x, 0f, z));
            return list;
        }

        [Fact]
        public void TwoOverlappingSquares_ReturnsTrue()
        {
            // Square (0,0)..(1,1) and square (0.5,0.5)..(1.5,1.5) share a
            // 0.5x0.5 area in their interiors.
            var positions = Positions(
                (0, 0), (1, 0), (1, 1), (0, 1),
                (0.5f, 0.5f), (1.5f, 0.5f), (1.5f, 1.5f), (0.5f, 1.5f));
            var a = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2, 3 } };
            var b = new Face { Id = 1, Vertices = new List<Id> { 4, 5, 6, 7 } };

            Assert.True(FaceQueries.FacesOverlap(EmptyFrame(), a, b, positions));
        }

        [Fact]
        public void TouchingAlongAnEdge_ReturnsFalse()
        {
            // Squares (0,0)..(1,1) and (1,0)..(2,1) share the edge x=1 but
            // no positive-area interior.
            var positions = Positions(
                (0, 0), (1, 0), (1, 1), (0, 1),
                (1, 0), (2, 0), (2, 1), (1, 1));
            var a = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2, 3 } };
            var b = new Face { Id = 1, Vertices = new List<Id> { 4, 5, 6, 7 } };

            Assert.False(FaceQueries.FacesOverlap(EmptyFrame(), a, b, positions));
        }

        [Fact]
        public void TouchingAtASingleVertex_ReturnsFalse()
        {
            // Squares (0,0)..(1,1) and (1,1)..(2,2) share only the corner
            // (1,1).
            var positions = Positions(
                (0, 0), (1, 0), (1, 1), (0, 1),
                (1, 1), (2, 1), (2, 2), (1, 2));
            var a = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2, 3 } };
            var b = new Face { Id = 1, Vertices = new List<Id> { 4, 5, 6, 7 } };

            Assert.False(FaceQueries.FacesOverlap(EmptyFrame(), a, b, positions));
        }

        [Fact]
        public void DisjointSquares_ReturnsFalse()
        {
            // Squares (0,0)..(1,1) and (2,2)..(3,3) — fully separated.
            var positions = Positions(
                (0, 0), (1, 0), (1, 1), (0, 1),
                (2, 2), (3, 2), (3, 3), (2, 3));
            var a = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2, 3 } };
            var b = new Face { Id = 1, Vertices = new List<Id> { 4, 5, 6, 7 } };

            Assert.False(FaceQueries.FacesOverlap(EmptyFrame(), a, b, positions));
        }

        [Fact]
        public void FullContainment_ReturnsTrue()
        {
            // Square (0.25,0.25)..(0.75,0.75) is fully inside (0,0)..(1,1).
            var positions = Positions(
                (0, 0), (1, 0), (1, 1), (0, 1),
                (0.25f, 0.25f), (0.75f, 0.25f), (0.75f, 0.75f), (0.25f, 0.75f));
            var outer = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2, 3 } };
            var inner = new Face { Id = 1, Vertices = new List<Id> { 4, 5, 6, 7 } };

            Assert.True(FaceQueries.FacesOverlap(EmptyFrame(), outer, inner, positions));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Folding;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;

namespace Testing.Folding;

public class ApplyValleyFoldTests
{
    // Integration coverage for FoldInteractionApplier.BuildValleyFoldChange —
    // the pure core that ApplyValleyFold wraps. We exercise it directly so
    // the tests don't need the Godot scene tree, the animator, or the
    // EventBus path.

    public class BuildValleyFoldChange : ApplyValleyFoldTests
    {
        // Fixture: TWO face-graph-disconnected rectangles (no shared
        // vertices or edges), with a stray "ghost crease" edge whose
        // (infinite) 2D line slices through BOTH rectangles' interiors.
        //
        //   (0,2)  +--------+ (1,2)                (1.5,1.5) +-----+ (2,1.5)
        //          |        |                                |     |
        //          |   A    |                                |  B  |
        //   y=1 -- |--------|-------- ghost crease ----------|-----|--- y=1
        //          |        |                                |     |
        //          |        |                                |     |
        //   (0,0)  +--------+ (1,0)                (1.5,0.5) +-----+ (2,0.5)
        //
        // ghost crease 8-9 endpoints at (-1,1) and (3,1). It is NOT an
        // edge of any face — it exists in frame.Edges only to give
        // BuildValleyFoldChange a 3D line.
        //
        // Picking face A: the participating set must be {A} (BFS can't
        // reach B across face-graph) so the crossing scan only splits A's
        // edges. With the legacy ApplyVertexValleyFold behaviour B's
        // edges 5 and 7 would ALSO be split — that's the phantom-crease
        // bug from ADR-0002 §"Why".

        private readonly Frame _frame;
        private readonly Frame3D _frame3d;
        private readonly Face _faceA;
        private readonly Face _faceB;
        private readonly Edge _ghostCrease;
        private readonly List<Id> _faceBVerticesAtStart;

        public BuildValleyFoldChange()
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
            _faceB = new Face { Id = 1, Vertices = new List<Id> { 4, 5, 6, 7 } };
            _frame.Faces.AddRange(new[] { _faceA, _faceB });
            _faceBVerticesAtStart = _faceB.Vertices.ToList();

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
        public void ReturnsNonNullChange()
        {
            var change = FoldInteractionApplier.BuildValleyFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            Assert.NotNull(change);
        }

        [Fact]
        public void RecordsPickedFaceId()
        {
            var change = FoldInteractionApplier.BuildValleyFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            Assert.Equal(_faceA.Id, change.PickedFace);
        }

        [Fact]
        public void RecordsPickedVertexOnGhostCrease()
        {
            // PickedVertex must lie on the crease so that the renderer's
            // fallback BFS (when PickedFace can't be resolved) still
            // anchors on a crease vertex per ADR-0001's invariant.
            var change = FoldInteractionApplier.BuildValleyFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            Assert.Equal(_ghostCrease.Vertices[0], change.PickedVertex);
        }

        [Fact]
        public void AddsExactlyTwoVerticesOnPickedFaceEdges()
        {
            // A's edges 1 and 3 each cross y=1 in their interior — two
            // splits. If B's edges 5 and 7 were ALSO scanned (the bug),
            // we'd see four added vertices.
            var change = FoldInteractionApplier.BuildValleyFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            Assert.Equal(2, change.AddedVertices.Count);
        }

        [Fact]
        public void DoesNotSplitEdgesOfNonParticipatingFace()
        {
            FoldInteractionApplier.BuildValleyFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            // B's vertex list must be byte-for-byte unchanged: no new
            // split vertices were inserted into its polygon. (If edges 5
            // or 7 of B had been split, EdgeCommands.AddVertexToEdge
            // would have inserted the new vertex Id into _faceB.Vertices.)
            Assert.Equal(_faceBVerticesAtStart, _faceB.Vertices);
        }

        [Fact]
        public void DoesNotAddEdgesBetweenSplitVerticesAndNonParticipatingFace()
        {
            // The crease-edge construction step in BuildValleyFoldChange
            // iterates pairs of AddedVertices and creates an edge only
            // when some face contains both. With the participating-set
            // restriction, both added vertices land on face A's polygon,
            // so the only new fold edge is A's internal crease. B
            // contributes nothing.
            var initialEdgeCount = _frame.Edges.Count;
            var change = FoldInteractionApplier.BuildValleyFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            // 2 vertex splits → 2 "second-half" edges added inside the
            // crossing-scan loop, then 1 crease edge added between the
            // two new vertices. Total new edges: 3.
            Assert.Equal(initialEdgeCount + 3, _frame.Edges.Count);
            Assert.Single(change.AddedEdges);
        }

        [Fact]
        public void SplitsCreatedOnlyOnFaceABorderEdges()
        {
            // Both new vertices sit on the y=1 line — at (1,1) on A's
            // right edge, and (0,1) on A's left edge. Neither should
            // coincide with any B vertex coordinate.
            var change = FoldInteractionApplier.BuildValleyFoldChange(
                _frame, _frame3d, _faceA, _ghostCrease);

            var newPositions = change.AddedVertices
                .Select(id => _frame.Vertices[id].Coord)
                .OrderBy(c => c.X).ThenBy(c => c.Y)
                .ToArray();

            Assert.Equal(2, newPositions.Length);
            Assert.Equal(new Vector2(0f, 1f), newPositions[0]);
            Assert.Equal(new Vector2(1f, 1f), newPositions[1]);
        }
    }

    public class BuildValleyFoldChangeOnDegenerateInput : ApplyValleyFoldTests
    {
        // BuildValleyFoldChange returns null (signalling "no change to
        // record") when the crossing scan finds nothing to split. That
        // path is reachable when the ghost crease lies entirely outside
        // the picked face's edges.

        [Fact]
        public void GhostCreaseOutsidePickedFace_ReturnsNull()
        {
            var frame = new Frame();
            frame.Vertices.AddRange(new[]
            {
                new Vertex { Coord = new Vector2(0, 0), Id = 0 },
                new Vertex { Coord = new Vector2(1, 0), Id = 1 },
                new Vertex { Coord = new Vector2(1, 1), Id = 2 },
                new Vertex { Coord = new Vector2(0, 1), Id = 3 },
                // crease endpoints far above the face — the line y=5
                // doesn't cross any of A's edges' interiors.
                new Vertex { Coord = new Vector2(-1, 5), Id = 4 },
                new Vertex { Coord = new Vector2(2, 5),  Id = 5 }
            });
            frame.Edges.AddRange(new[]
            {
                new Edge { Vertices = new Id[] { 0, 1 }, Id = 0, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 1, 2 }, Id = 1, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 2, 3 }, Id = 2, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 3, 0 }, Id = 3, Assignment = Assignment.B },
                new Edge { Vertices = new Id[] { 4, 5 }, Id = 4, Assignment = Assignment.B }
            });
            var faceA = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2, 3 } };
            frame.Faces.Add(faceA);

            var frame3d = new Frame3D();
            for (var i = 0; i < frame.Vertices.Count; i++)
            {
                var v2d = frame.Vertices[i].Coord;
                frame3d.Vertices.Add(new Vertex3D
                {
                    Coord = new Vector3(v2d.X, 0f, v2d.Y)
                });
            }

            var change = FoldInteractionApplier.BuildValleyFoldChange(
                frame, frame3d, faceA, frame.Edges[4]);

            Assert.Null(change);
        }
    }
}

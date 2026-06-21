using Godot;
using valleyfold.Templates;
using valleyfold.TwoDeeModels;
using Xunit;

namespace Testing.Templates;

public class PaperResolverTests
{
    public class ResolveVertexAt
    {
        private readonly Frame _frame;

        public ResolveVertexAt()
        {
            _frame = new Frame();
            _frame.InitializePaper();
        }

        [Fact]
        public void ReturnsCornerVertexAtOrigin()
        {
            var id = PaperResolver.ResolveVertexAt(_frame, new Vector2(0, 0));
            Assert.Equal((Id)0, id);
        }

        [Fact]
        public void ReturnsCornerVertexAtOppositeCorner()
        {
            var id = PaperResolver.ResolveVertexAt(_frame, new Vector2(1, 1));
            Assert.Equal((Id)2, id);
        }

        [Fact]
        public void ReturnsNegativeOneWhenNoVertexInRange()
        {
            var id = PaperResolver.ResolveVertexAt(_frame, new Vector2(0.5f, 0.5f));
            Assert.Equal((Id)(-1), id);
        }

        [Fact]
        public void TolerantToSmallOffsetWithinThreshold()
        {
            var id = PaperResolver.ResolveVertexAt(
                _frame, new Vector2(0.0005f, 0.0005f), tolerance: 0.01f);
            Assert.Equal((Id)0, id);
        }
    }

    public class ResolveOrCreateVertexAt
    {
        private readonly Frame _frame;

        public ResolveOrCreateVertexAt()
        {
            _frame = new Frame();
            _frame.InitializePaper();
        }

        [Fact]
        public void ReturnsExistingVertexWithoutMutating()
        {
            var beforeCount = _frame.Vertices.Count;
            var id = PaperResolver.ResolveOrCreateVertexAt(_frame, new Vector2(1, 0));
            Assert.Equal((Id)1, id);
            Assert.Equal(beforeCount, _frame.Vertices.Count);
        }

        [Fact]
        public void SplitsEdgeAtMidpointWhenNoVertexMatches()
        {
            // Edge 0 is the bottom edge (0,0)-(1,0). A point at (0.5, 0)
            // lies on its interior and should create a new vertex there.
            var id = PaperResolver.ResolveOrCreateVertexAt(_frame, new Vector2(0.5f, 0));
            Assert.NotEqual((Id)(-1), id);
            Assert.Equal(new Vector2(0.5f, 0), _frame.Vertices[id].Coord);
            Assert.Equal(5, _frame.Vertices.Count);
        }

        [Fact]
        public void ReturnsNegativeOneWhenPointIsInsideFaceInterior()
        {
            // (0.5, 0.5) is interior to the only face but not on any edge.
            var id = PaperResolver.ResolveOrCreateVertexAt(_frame, new Vector2(0.5f, 0.5f));
            Assert.Equal((Id)(-1), id);
        }
    }
}

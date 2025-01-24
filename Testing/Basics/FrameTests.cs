using Godot;
using valleyfold;
using valleyfold.Fold;

namespace Testing.Basics;

public class FrameTests
{
    private readonly Frame _frame;

    public FrameTests()
    {
        _frame = new Frame();
        _frame.InitializePaper();
    }

    public class NearestEdgeTo : FrameTests
    {
        [Fact]
        public void PointIsOutside_ReturnsSomeEdges()
        {
            var point = new Vector3(-0.5f, 0, 0.5f);

            var edges = _frame.NearestEdgesTo(point);

            Assert.Equivalent(new[] { _frame.Edges.ElementAt(1), _frame.Edges.ElementAt(3) }, edges);
        }

        [Fact]
        public void PointIsInside_ReturnAllPolygonEdges()
        {
            var point = new Vector3(0.5f, 0, 0.5f);

            var edges = _frame.NearestEdgesTo(point);

            Assert.Equivalent(_frame.Edges, edges);
        }
    }
}
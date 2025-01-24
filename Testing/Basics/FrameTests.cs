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
    
    public class NearestVertexIdTo : FrameTests
    {
        [Fact]
        public void ReturnsNearestVertexToPoint()
        {
            var point = new Vector3(1.5f, 0, 1.5f);

            var result = _frame.NearestVertexIdTo(point);
            
            Assert.Equivalent((Id)2, result);
        }
    }

    public class NearestPointOnEdgeTo : FrameTests
    {
        [Fact]
        public void CalculateNearestPointOnEdge()
        {
            var point = new Vector3(-0.5f, 0, 0.5f);
            var edge = _frame.Edges.ElementAt(3);

            var result = _frame.NearestPointOnEdgeTo(point, edge);
            
            Assert.Equivalent(new Vector3(0, 0, 0.5f), result);
        }

        [Fact]
        public void CalculateCorrectlyForDiagonalEdge()
        {
            var point = new Vector3(0.5f, 0, 0.5f);
            var edge = new Edge()
            {
                Vertices = [0, 2],
                Assignment = Assignment.V
            };
            
            var result = _frame.NearestPointOnEdgeTo(point, edge);
            Assert.Equivalent(new Vector3(0.5f, 0, 0.5f), result);
        }

        [Fact]
        public void CalculateNearestPointOnEdges()
        {
            var point = new Vector3(-0.5f, 0, 0.5f);
            var edges = new List<Edge>() { _frame.Edges.ElementAt(3), _frame.Edges.ElementAt(1) };
            
            var result = _frame.NearestPointOnEdgeTo(point, edges);
            
            Assert.Equivalent(new Vector3(0, 0, 0.5f), result);
        }
        
        [Fact]
        public void CalculateNearestPointOnEdgesRight()
        {
            var point = new Vector3(1.5f, 0, 0.5f);
            var edges = new List<Edge>() { _frame.Edges.ElementAt(3), _frame.Edges.ElementAt(1) };
            
            var result = _frame.NearestPointOnEdgeTo(point, edges);
            
            Assert.Equivalent(new Vector3(1f, 0, 0.5f), result);
        }
    }
}
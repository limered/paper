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
    
    public class AddVertexOnBorderEdge : FrameTests
    {
        [Fact]
        public void AddsTheVertexToFrame()
        {
            var point = new Vector3(1f, 0, 0.5f);
            var edge = _frame.Edges.ElementAt(1);

            var id = _frame.AddVertexOnEdge(point, edge);

            Assert.Equivalent(new Vertex { Coord = point }, _frame.Vertices[id]);
        }

        [Fact]
        public void SplitsEdge()
        {
            var point = new Vector3(1f, 0, 0.5f);
            var edge = _frame.Edges.ElementAt(1);
            var oldEdgeEndId = edge.Vertices[1];
            
            var id = _frame.AddVertexOnEdge(point, edge);
            
            Assert.Equivalent(id, edge.Vertices[1]);
            Assert.Equivalent(id, _frame.Edges.Last().Vertices[0]);
            Assert.Equivalent(oldEdgeEndId, _frame.Edges.Last().Vertices[1]);
        }

        [Fact]
        public void AddVertexToAdjacentFaces()
        {
            var point = new Vector3(1f, 0, 0.5f);
            var edge = _frame.Edges.ElementAt(1);
            var edgeAdjacentFace = _frame.Faces.First();
            
            var id = _frame.AddVertexOnEdge(point, edge);
            
            Assert.Equivalent(id, edgeAdjacentFace.Vertices[2]);
        }
        
        [Fact]
        public void FaceKeepsOtherVertices()
        {
            var point = new Vector3(1f, 0, 0.5f);
            var edge = _frame.Edges.ElementAt(1);
            var edgeAdjacentFace = _frame.Faces.First();
            
            _ = _frame.AddVertexOnEdge(point, edge);
            
            Assert.Equivalent((Id)2, edgeAdjacentFace.Vertices[3]);
            Assert.Equivalent((Id)3, edgeAdjacentFace.Vertices[4]);
        }

        [Fact]
        public void EdgeIstLastEdge_AddsVertexAtEndpoint()
        {
            var point = new Vector3(0, 0, 0.5f);
            var edge = _frame.Edges.Last();
            var edgeAdjacentFace = _frame.Faces.First();

            _ = _frame.AddVertexOnEdge(point, edge);
            
            Assert.Equivalent((Id)4, edgeAdjacentFace.Vertices[4]);
            Assert.Equivalent((Id)3, edgeAdjacentFace.Vertices[3]);
        }
    }
}
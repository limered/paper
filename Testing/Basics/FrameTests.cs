using Godot;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;

namespace Testing.Basics;

public class FrameTests
{
    private readonly Frame _frame;

    protected FrameTests()
    {
        _frame = new Frame();
        _frame.InitializePaper();
    }

    public class NearestEdgeTo : FrameTests
    {
        [Fact]
        public void PointIsOutside_ReturnsSomeEdges()
        {
            var point = new Vector2(-0.5f, 0.5f);

            var edges = EdgeQueries.NearestEdgesToPoint(_frame, point);

            Assert.Equivalent(new[] { _frame.Edges[1], _frame.Edges[3] }, edges);
        }

        [Fact]
        public void PointIsInside_ReturnAllPolygonEdges()
        {
            var point = new Vector2(0.5f, 0.5f);

            var edges = EdgeQueries.NearestEdgesToPoint(_frame, point);

            Assert.Equivalent(_frame.Edges, edges);
        }
    }

    public class NearestVertexIdTo : FrameTests
    {
        [Fact]
        public void ReturnsNearestVertexToPoint()
        {
            var point = new Vector2(1.5f, 1.5f);

            var result = VertexQueries.NearestVertexIdTo(_frame, point);

            Assert.Equivalent((Id)2, result);
        }

        [Fact]
        public void IfThresholdIsGivenOnlyPointInsideThreshold()
        {
            var point = new Vector2(1.1f, 1.1f);

            var result = VertexQueries.NearestVertexIdTo(_frame, point, 0.2f);

            Assert.Equivalent((Id)2, result);
        }

        [Fact]
        public void IfThresholdIsGivenAndPointsOutside_ReturnNegativeOne()
        {
            var point = new Vector2(1.3f, 1.3f);

            var result = VertexQueries.NearestVertexIdTo(_frame, point, 0.2f);

            Assert.Equivalent(new Id { Value = -1 }, result);
        }
    }

    public class NearestPointOnEdgeTo : FrameTests
    {
        [Fact]
        public void CalculateNearestPointOnEdge()
        {
            var point = new Vector2(-0.5f, 0.5f);
            var edge = _frame.Edges.ElementAt(3);

            var result = EdgeQueries.NearestPointOnEdgeTo(_frame, point, edge);

            Assert.Equivalent(new Vector2(0, 0.5f), result);
        }

        [Fact]
        public void CalculateCorrectlyForDiagonalEdge()
        {
            var point = new Vector2(0.5f, 0.5f);
            var edge = new Edge
            {
                Vertices = [0, 2],
                Assignment = Assignment.V
            };

            var result = EdgeQueries.NearestPointOnEdgeTo(_frame, point, edge);
            Assert.Equivalent(new Vector2(0.5f, 0.5f), result);
        }

        [Fact]
        public void CalculateNearestPointOnEdges()
        {
            var point = new Vector2(-0.5f, 0.5f);
            var edges = new List<Edge> { _frame.Edges.ElementAt(3), _frame.Edges.ElementAt(1) };

            var result = EdgeQueries.NearestPointOnEdgeToPoint(_frame, point, edges);

            Assert.Equivalent(new Vector2(0, 0.5f), result.Item1);
        }

        [Fact]
        public void CalculateNearestPointOnEdgesRight()
        {
            var point = new Vector2(1.5f, 0.5f);
            var edges = new List<Edge> { _frame.Edges.ElementAt(3), _frame.Edges.ElementAt(1) };

            var result = EdgeQueries.NearestPointOnEdgeToPoint(_frame, point, edges);

            Assert.Equivalent(new Vector2(1f, 0.5f), result.Item1);
        }

        [Fact]
        public void ReturnsEdgeContainingPoint()
        {
            var point = new Vector2(1.5f, 0.5f);
            var edges = new List<Edge> { _frame.Edges.ElementAt(3), _frame.Edges.ElementAt(1) };

            var result = EdgeQueries.NearestPointOnEdgeToPoint(_frame, point, edges);

            Assert.Equivalent(_frame.Edges.ElementAt(1), result.Item2);
        }
    }

    public class AddVertexOnBorderEdge : FrameTests
    {
        [Fact]
        public void AddsTheVertexToFrame()
        {
            var point = new Vector2(1f, 0.5f);
            var edge = _frame.Edges.ElementAt(1);

            var id = EdgeCommands.AddVertexToEdge(_frame, point, edge);

            Assert.Equal(point, _frame.Vertices[id].Coord);
        }

        [Fact]
        public void SplitsEdge()
        {
            var point = new Vector2(1f, 0.5f);
            var edge = _frame.Edges.ElementAt(1);
            var oldEdgeEndId = edge.Vertices[1];

            var id = EdgeCommands.AddVertexToEdge(_frame, point, edge);

            Assert.Equivalent(id, edge.Vertices[1]);
            Assert.Equivalent(id, _frame.Edges.Last().Vertices[0]);
            Assert.Equivalent(oldEdgeEndId, _frame.Edges.Last().Vertices[1]);
        }

        [Fact]
        public void AddVertexToAdjacentFaces()
        {
            var point = new Vector2(1f, 0.5f);
            var edge = _frame.Edges.ElementAt(1);
            var edgeAdjacentFace = _frame.Faces.First();

            var id = EdgeCommands.AddVertexToEdge(_frame, point, edge);

            Assert.Equivalent(id, edgeAdjacentFace.Vertices[2]);
        }

        [Fact]
        public void FaceKeepsOtherVertices()
        {
            var point = new Vector2(1f, 0.5f);
            var edge = _frame.Edges.ElementAt(1);
            var edgeAdjacentFace = _frame.Faces.First();

            _ = EdgeCommands.AddVertexToEdge(_frame, point, edge);

            Assert.Equivalent((Id)2, edgeAdjacentFace.Vertices[3]);
            Assert.Equivalent((Id)3, edgeAdjacentFace.Vertices[4]);
        }

        [Fact]
        public void EdgeIstLastEdge_AddsVertexAtEndpoint()
        {
            var point = new Vector2(0, 0.5f);
            var edge = _frame.Edges
                .Last();
            var edgeAdjacentFace = _frame.Faces.First();

            _ = EdgeCommands.AddVertexToEdge(_frame, point, edge);

            Assert.Equivalent((Id)4, edgeAdjacentFace.Vertices[4]);
            Assert.Equivalent((Id)3, edgeAdjacentFace.Vertices[3]);
        }
    }
}
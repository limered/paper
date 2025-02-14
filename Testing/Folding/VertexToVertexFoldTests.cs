using Godot;
using valleyfold.Fold;
using valleyfold.Folding;

namespace Testing.Folding;

public class VertexToVertexFoldTests
{
    private readonly Frame _testingFrame;

    protected VertexToVertexFoldTests()
    {
        _testingFrame = new Frame();
        _testingFrame.InitializePaper();
    }

    public class SimpleFold : VertexToVertexFoldTests
    {
        [Fact]
        public void IfVerticesAreTheSame_DoNotChangeFrame()
        {
            new VertexToVertexFold(0, 0).Apply(_testingFrame);

            Assert.Equivalent(1, _testingFrame.Faces.Count);
        }

        [Fact]
        public void IfVerticesLieTheSameEdge_DoNotChangeFrame()
        {
            new VertexToVertexFold(0, 1).Apply(_testingFrame);

            Assert.Equivalent(1, _testingFrame.Faces.Count);
        }

        [Fact]
        public void CreateANewEdgeBetweenPoints()
        {
            new VertexToVertexFold(0, 2).Apply(_testingFrame);

            Assert.Equivalent(new Id[] { 0, 2 }, _testingFrame.Edges.Last().Vertices);
        }

        [Fact]
        public void CreateTwoNewFaces()
        {
            new VertexToVertexFold(0, 2).Apply(_testingFrame);

            Assert.Equivalent(new Id[] { 0, 1, 2 }, _testingFrame.Faces.ElementAt(1).Vertices);
            Assert.Equivalent(new Id[] { 2, 3, 0 }, _testingFrame.Faces.ElementAt(0).Vertices);
        }
    }

    public class CrossingFolds : VertexToVertexFoldTests
    {
        [Fact]
        public void CrossFoldEdges()
        {
            SplitHorizontally();
            SplitVertically();

            Assert.Equivalent(4, _testingFrame.Faces.Count);
            Assert.Equivalent(new Vertex { Coord = new Vector3(0.5f, 0, 0.5f) }, _testingFrame.Vertices[8]);
        }

        [Fact]
        public void CrossFoldVertices()
        {
            new VertexToVertexFold(0, 2).Apply(_testingFrame);
            new VertexToVertexFold(1, 3).Apply(_testingFrame);

            Assert.Equivalent(4, _testingFrame.Faces.Count);
            Assert.Equivalent(new Vertex { Coord = new Vector3(0.5f, 0, 0.5f) }, _testingFrame.Vertices[4]);
        }

        [Fact]
        public void CrossFoldThenEdgeFold()
        {
            SplitHorizontally();
            SplitVertically();
            new VertexToVertexFold(0, 2).Apply(_testingFrame);

            Assert.Equivalent(6, _testingFrame.Faces.Count);
            Assert.Equal(1, _testingFrame.Vertices.Count(v => v == new Vertex { Coord = new Vector3(0.5f, 0, 0.5f) }));
        }

        private void SplitHorizontally()
        {
            var pointA = new Vector3(0, 0, 0.5f);
            var pointB = new Vector3(1f, 0, 0.5f);
            var edgeA = _testingFrame.Edges.ElementAt(3);
            var edgeB = _testingFrame.Edges.ElementAt(1);

            new EdgeToEdgeFold(edgeA, edgeB, pointA, pointB).Apply(_testingFrame);
        }

        private void SplitVertically()
        {
            var pointA = new Vector3(0.5f, 0, 0);
            var pointB = new Vector3(0.5f, 0, 1f);
            var edgeA = _testingFrame.Edges.ElementAt(0);
            var edgeB = _testingFrame.Edges.ElementAt(2);

            new EdgeToEdgeFold(edgeA, edgeB, pointA, pointB).Apply(_testingFrame);
        }
    }
}
using Godot;
using valleyfold.Fold;
using valleyfold.Folding;

namespace Testing.Folding;

public class EdgeToEdgeTests
{
    private readonly Frame _testingFrame;

    public EdgeToEdgeTests()
    {
        _testingFrame = new Frame();
        _testingFrame.InitializePaper();
    }

    [Fact]
    public void IfEdgesAreTheSame_DoNotChangeFrame()
    {
        var edge = _testingFrame.Edges.ElementAt(0);
        new EdgeToEdgeFold(edge, edge, new Vector2(), new Vector2())
            .Apply(_testingFrame);

        Assert.Equivalent(1, _testingFrame.Faces.Count);
    }

    [Fact]
    public void AddANewEdgeBetweenPoints()
    {
        var vertexA = new Vertex { Coord = new Vector2(0, 0.5f) };
        var vertexB = new Vertex { Coord = new Vector2(1f, 0.5f) };
        var edgeA = _testingFrame.Edges.ElementAt(3);
        var edgeB = _testingFrame.Edges.ElementAt(1);

        var expectedEdge = new Edge
        {
            Vertices = [4, 5],
            Assignment = Assignment.U,
            FoldAngle = 0
        };

        new EdgeToEdgeFold(edgeA, edgeB, vertexA.Coord, vertexB.Coord)
            .Apply(_testingFrame);

        Assert.Equivalent(expectedEdge, _testingFrame.Edges[6]);
    }

    [Fact]
    public void CreateTwoNewFaces()
    {
        var vertexA = new Vertex { Coord = new Vector2(0, 0.5f) };
        var vertexB = new Vertex { Coord = new Vector2(1f, 0.5f) };
        var edgeA = _testingFrame.Edges.ElementAt(3);
        var edgeB = _testingFrame.Edges.ElementAt(1);

        new EdgeToEdgeFold(edgeA, edgeB, vertexA.Coord, vertexB.Coord)
            .Apply(_testingFrame);

        Assert.Equivalent(new Id[] { 5, 2, 3, 4 }, _testingFrame.Faces.ElementAt(0).Vertices);
        Assert.Equivalent(new Id[] { 4, 0, 1, 5 }, _testingFrame.Faces.ElementAt(1).Vertices);
    }
}
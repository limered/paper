using Godot;
using valleyfold.Folding;
using valleyfold.TwoDeeModels;

namespace Testing.Folding;

public class VertexToEdgeFoldTests
{
    private readonly Frame _testingFrame;

    public VertexToEdgeFoldTests()
    {
        _testingFrame = new Frame();
        _testingFrame.InitializePaper();
    }

    [Fact]
    public void IfEdgeAlreadyExists_DoNotChangeFrame()
    {
        var edge = _testingFrame.Edges.ElementAt(0);
        new VertexToEdgeFold(edge, new Vector2(), 0)
            .Apply(_testingFrame);

        Assert.Equivalent(1, _testingFrame.Faces.Count);
    }

    [Fact]
    public void AddANewEdgeBetweenPoints()
    {
        var point = new Vector2(0, 0.5f);
        var edge = _testingFrame.Edges.ElementAt(3);

        var expectedEdge = new Edge
        {
            Vertices = [1, 4],
        };

        new VertexToEdgeFold(edge, point, 1)
            .Apply(_testingFrame);

        Assert.Equivalent(expectedEdge.Vertices, _testingFrame.Edges[5].Vertices);
    }
    
    [Fact]
    public void CreateTwoNewFaces()
    {
        var point = new Vector2(0, 0.5f);
        var edge = _testingFrame.Edges.ElementAt(3);

        new VertexToEdgeFold(edge, point, 1)
            .Apply(_testingFrame);

        Assert.Equivalent(new Id[] { 4, 0, 1 }, _testingFrame.Faces.ElementAt(0).Vertices);
        Assert.Equivalent(new Id[] { 1, 2, 3, 4 }, _testingFrame.Faces.ElementAt(1).Vertices);
    }
}
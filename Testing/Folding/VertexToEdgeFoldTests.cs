using Godot;
using valleyfold.Fold;
using valleyfold.Folding;

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
        new VertexToEdgeFold(edge, new Vector3(), 0)
            .Apply(_testingFrame);

        Assert.Equivalent(1, _testingFrame.Faces.Count);
    }

    [Fact]
    public void AddANewEdgeBetweenPoints()
    {
        var point = new Vector3(0, 0, 0.5f);
        var edge = _testingFrame.Edges.ElementAt(3);

        var expectedEdge = new Edge
        {
            Vertices = [1, 4],
            Assignment = Assignment.U,
            FoldAngle = 0
        };

        new VertexToEdgeFold(edge, point, 1)
            .Apply(_testingFrame);

        Assert.Equivalent(expectedEdge, _testingFrame.Edges[5]);
    }
}
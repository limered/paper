using Godot;
using valleyfold.Fold;
using valleyfold.Folding;

namespace Testing.Folding;

public class ValleyfoldTests
{
    private readonly Frame _testingFrame;
    
    public ValleyfoldTests()
    {
        _testingFrame = new Frame();
        _testingFrame.InitializePaper();
    }

    [Fact]
    public void IfEdgeAlreadyExists_DoNotChangeFrame()
    {
        new Valleyfold { Vertices = { 0, 1 } }.Apply(_testingFrame);
        
        Assert.Equivalent(1, _testingFrame.Faces.Count());
    }

    [Fact]
    public void SplitPolygonsClockwise() 
    {
        new Valleyfold { Vertices = { 0, 2 } }.Apply(_testingFrame);
        
        Assert.Equivalent(new Id[]{2, 3, 0}, _testingFrame.Faces.ElementAt(0).Vertices);
        Assert.Equivalent(new Id[]{0, 1, 2}, _testingFrame.Faces.ElementAt(1).Vertices);
    }

    [Fact]
    public void NewEdgeIsAValleyfold() 
    {
        new Valleyfold { Vertices = { 0, 2 } }.Apply(_testingFrame);
        
        Assert.Equivalent(Assignment.V, _testingFrame.Edges.Last().Assignment);
    }

    [Fact]
    public void SplitTheInitialPaperCorrectly()
    {
        _testingFrame.AddVertexOnEdge(new Vector3(0, 0, 0.5f), _testingFrame.Edges.ElementAt(3));
        _testingFrame.AddVertexOnEdge(new Vector3(1f, 0, 0.5f), _testingFrame.Edges.ElementAt(1));
        
        new Valleyfold{Vertices = {4, 5}}.Apply(_testingFrame);
        
        Assert.Equivalent(new Id[]{ 5, 2, 3, 4 }, _testingFrame.Faces.ElementAt(0).Vertices);
        Assert.Equivalent(new Id[]{ 4, 0, 1, 5 }, _testingFrame.Faces.ElementAt(1).Vertices);
    }
}
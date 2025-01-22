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
}
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
    
    
}
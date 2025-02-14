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
}
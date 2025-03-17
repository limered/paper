using Godot;
using Testing.Utils;
using valleyfold;
using valleyfold.Folding;
using valleyfold.TwoDeeModels;

namespace Testing.Folding;

public class VertexInteractionTests
{
    private readonly Frame _testingFrame;
    
    public VertexInteractionTests()
    {
        _testingFrame = new Frame();
        _testingFrame.InitializePaper();
        Statics.Frame = _testingFrame;
    }

    public class DraftTests : VertexInteractionTests
    {
        [Fact]
        public void DraggedVertexCreatesEdgeBetweenPoints()
        {
            var vertexAction = new VertexInteraction(0, new Vector2(.5f, .5f));
            var edgeStrip = vertexAction.Draft(_testingFrame);
            
            Assert.Equivalent(2, edgeStrip.Length);
            Assert.Equivalent(_testingFrame.Edges[0], edgeStrip.Edges[0]);
            Assert.Equivalent(_testingFrame.Edges[3], edgeStrip.Edges[1]);
            Assert.Equivalent(new Vector2(0.5f, 0), edgeStrip.Points[0]);
            Assert.Equivalent(new Vector2(0, 0.5f), edgeStrip.Points[1]);
        }
        
        [Fact]
        public void IfNewEdgeCrossesUnspecifiedFold_DraftTwoEdgesInOneLine()
        {
            var startEdge = _testingFrame.Edges[0];
            var endEdge = _testingFrame.Edges[2];
            var startPoint = new Vector2(0.25f, 0);
            var endPoint = new Vector2(0.25f, 1f);
            
            new EdgeToEdgeFold(startEdge, endEdge, startPoint, endPoint, Assignment.U)
                .Apply(_testingFrame);
            
            var vertexAction = new VertexInteraction(0, new Vector2(0.5f, 0.5f));
            var edgeStrip = vertexAction.Draft(_testingFrame);
            
            Assert.Equivalent(3, edgeStrip.Length);
            Assert.Equivalent(_testingFrame.Edges[4], edgeStrip.Edges[0]);
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.Edges[1]);
            Assert.Equivalent(_testingFrame.Edges[3], edgeStrip.Edges[2]);
            
            AssertUtils.EquivalentWithEpsilonVector2(new Vector2(0.5f, 0), edgeStrip.Points[0]);
            AssertUtils.EquivalentWithEpsilonVector2(new Vector2(0.25f, 0.25f), edgeStrip.Points[1]);
            AssertUtils.EquivalentWithEpsilonVector2(new Vector2(0f, 0.5f), edgeStrip.Points[2]);
        }
        
        [Fact]
        public void IfNewEdgeCrossesValleyFold_DraftTwoEdgesInOneLine()
        {
            var startEdge = _testingFrame.Edges[0];
            var endEdge = _testingFrame.Edges[2];
            var startPoint = new Vector2(0.25f, 0);
            var endPoint = new Vector2(0.25f, 1f);
            
            new EdgeToEdgeFold(startEdge, endEdge, startPoint, endPoint, Assignment.V)
                .Apply(_testingFrame);
            
            var vertexAction = new VertexInteraction(0, new Vector2(0.4f, 0.4f));
            var edgeStrip = vertexAction.Draft(_testingFrame);
            
            Assert.Equivalent(3, edgeStrip.Length);
            Assert.Equivalent(_testingFrame.Edges[3], edgeStrip.Edges[0]);
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.Edges[1]);
            
            Assert.Equivalent(_testingFrame.Edges[1], edgeStrip.Edges[2]);
            
            AssertUtils.EquivalentWithEpsilon(edgeStrip.Points[0].Y, 0.4f);
            AssertUtils.EquivalentWithEpsilon(edgeStrip.Points[1].X, 0.25f);
            Assert.InRange(edgeStrip.Points[1].Y, 0.1f, 0.2f);
            
            AssertUtils.InRangeVector2(edgeStrip.Points[2], 
                new Vector2(0.99f, 0.9f), new Vector2(1.01f, 1f));
        }
    }
}
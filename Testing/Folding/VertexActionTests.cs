using Godot;
using valleyfold;
using valleyfold.Fold;
using valleyfold.Folding;

namespace Testing.Folding;

public class VertexActionTests
{
    private readonly Frame _testingFrame;
    
    public VertexActionTests()
    {
        _testingFrame = new Frame();
        _testingFrame.InitializePaper();
        Statics.Frame = _testingFrame;
    }

    public class DraftTests : VertexActionTests
    {
        [Fact]
        public void DraggedVertexCreatesEdgeBetweenPoints()
        {
            var vertexAction = new VertexAction(0, new Vector2(.5f, .5f));
            var edgeStrip = vertexAction.Draft(_testingFrame);
            
            Assert.Equivalent(1, edgeStrip.Length);
            Assert.Equivalent(_testingFrame.Edges[0], edgeStrip.StartEdges[0]);
            Assert.Equivalent(_testingFrame.Edges[3], edgeStrip.EndEdges[0]);
            Assert.Equivalent(new Vector2(0.5f, 0), edgeStrip.StartPoints[0]);
            Assert.Equivalent(new Vector2(0, 0.5f), edgeStrip.EndPoints[0]);
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
            
            var vertexAction = new VertexAction(0, new Vector2(0.5f, 0.5f));
            var edgeStrip = vertexAction.Draft(_testingFrame);
            
            Assert.Equivalent(2, edgeStrip.Length);
            Assert.Equivalent(_testingFrame.Edges[4], edgeStrip.StartEdges[0]);
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.EndEdges[0]);
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.StartEdges[1]);
            Assert.Equivalent(_testingFrame.Edges[3], edgeStrip.EndEdges[1]);
            
            Assert.Equivalent(new Vector2(0.5f, 0), edgeStrip.StartPoints[0]);
            Assert.Equivalent(new Vector2(0.25f, 0.25f), edgeStrip.EndPoints[0]);
            Assert.Equivalent(new Vector2(0.25f, 0.25f), edgeStrip.StartPoints[1]);
            Assert.Equivalent(new Vector2(0f, 0.5f), edgeStrip.EndPoints[1]);
        }
        
        // special case for Line through vertex
        // [Fact]
        public void IfNewEdgeCrossesValleyFold_GoesThroughVertex_DraftTwoEdgesInOneLine()
        {
            var startEdge = _testingFrame.Edges[0];
            var endEdge = _testingFrame.Edges[2];
            var startPoint = new Vector2(0.25f, 0);
            var endPoint = new Vector2(0.25f, 1f);
            
            new EdgeToEdgeFold(startEdge, endEdge, startPoint, endPoint, Assignment.V)
                .Apply(_testingFrame);
            
            var vertexAction = new VertexAction(0, new Vector2(0.5f, 0.5f));
            var edgeStrip = vertexAction.Draft(_testingFrame);
            
            Assert.Equivalent(2, edgeStrip.Length);
            Assert.Equivalent(_testingFrame.Edges[4], edgeStrip.StartEdges[0]);
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.EndEdges[0]);
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.StartEdges[1]);
            Assert.Equivalent(_testingFrame.Edges[3], edgeStrip.EndEdges[1]);
            
            Assert.Equivalent(new Vector2(0.5f, 0), edgeStrip.StartPoints[0]);
            Assert.Equivalent(new Vector2(0.25f, 0.25f), edgeStrip.EndPoints[0]);
            Assert.Equivalent(new Vector2(0.25f, 0.25f), edgeStrip.StartPoints[1]);
            Assert.Equivalent(new Vector2(0f, 0.5f), edgeStrip.EndPoints[1]);
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
            
            var vertexAction = new VertexAction(0, new Vector2(0.4f, 0.4f));
            var edgeStrip = vertexAction.Draft(_testingFrame);
            
            Assert.Equivalent(2, edgeStrip.Length);
            Assert.Equivalent(_testingFrame.Edges[3], edgeStrip.StartEdges[0]);
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.EndEdges[0]);
            
            Assert.Equivalent(_testingFrame.Edges[6], edgeStrip.StartEdges[1]);
            Assert.Equivalent(_testingFrame.Edges[1], edgeStrip.EndEdges[1]);
            
            Assert.InRange(edgeStrip.StartPoints[0].Y, 0.39f, 0.41f);
            Assert.InRange(edgeStrip.EndPoints[0].X, 0.24f, 0.26f);
            Assert.InRange(edgeStrip.EndPoints[0].Y, 0.1f, 0.2f);
            
            Assert.InRange(edgeStrip.StartPoints[1].X, 0.24f, 0.26f);
            Assert.InRange(edgeStrip.StartPoints[1].Y, 0.1f, 0.2f);
            Assert.InRange(edgeStrip.EndPoints[1].X, 0.99f, 1.01f);
            Assert.InRange(edgeStrip.EndPoints[1].Y, 0.9f, 1f);
        }
    }
}
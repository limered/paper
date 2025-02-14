using Godot;
using valleyfold.Fold;
using valleyfold.Folding;
using valleyfold.FrameModifications;

namespace Testing.Folding;

public class UnspecifiedFoldTests
{
    private readonly Frame _testingFrame;

    public UnspecifiedFoldTests()
    {
        _testingFrame = new Frame();
        _testingFrame.InitializePaper();
    }

    [Fact]
    public void IfEdgeAlreadyExists_DoNotChangeFrame()
    {
        new UnspecifiedFold
        {
            VertexIds = { [0] = 0, [1] = 1 },
            VertexExists = { [0] = true, [1] = true }
        }.Apply(_testingFrame);

        Assert.Equivalent(1, _testingFrame.Faces.Count);
    }

    [Fact]
    public void SplitPolygonsClockwise()
    {
        var fold = new UnspecifiedFold { VertexIds = { [0] = 0, [1] = 2 }, VertexExists = { [0] = true, [1] = true } };
        fold.Apply(_testingFrame);

        Assert.Equivalent(new Id[] { 2, 3, 0 }, _testingFrame.Faces.ElementAt(0).Vertices);
        Assert.Equivalent(new Id[] { 0, 1, 2 }, _testingFrame.Faces.ElementAt(1).Vertices);
    }

    [Fact]
    public void NewEdgeIsUnspecified()
    {
        new UnspecifiedFold { VertexIds = { [0] = 0, [1] = 2 }, VertexExists = { [0] = true, [1] = true } }
            .Apply(_testingFrame);

        Assert.Equivalent(Assignment.U, _testingFrame.Edges.Last().Assignment);
    }

    [Fact]
    public void SplitCorrectlyEdgeToEdge()
    {
        var vertexA = new Vertex { Coord = new Vector3(0, 0, 0.5f) };
        var vertexB = new Vertex { Coord = new Vector3(1f, 0, 0.5f) };
        var edgeA = _testingFrame.Edges.ElementAt(3);
        var edgeB = _testingFrame.Edges.ElementAt(1); 

        new UnspecifiedFold
        {
            Vertices = { [0] = vertexA, [1] = vertexB },
            VertexEdges = { [0] = edgeA, [1] = edgeB },
            VertexExists = { [0] = false, [1] = false }
        }.Apply(_testingFrame);

        Assert.Equivalent(new Id[] { 5, 2, 3, 4 }, _testingFrame.Faces.ElementAt(0).Vertices);
        Assert.Equivalent(new Id[] { 4, 0, 1, 5 }, _testingFrame.Faces.ElementAt(1).Vertices);
    }

    [Fact]
    public void SplitCorrectlyVertexToVertex()
    {
        var fold = new UnspecifiedFold
        {
            VertexIds = { [0] = 0, [1] = 2 },
            VertexExists = { [0] = true, [1] = true }
        };
        fold.Apply(_testingFrame);

        Assert.Equivalent(new Id[] { 2, 3, 0 }, _testingFrame.Faces.ElementAt(0).Vertices);
        Assert.Equivalent(new Id[] { 0, 1, 2 }, _testingFrame.Faces.ElementAt(1).Vertices);

    }

    [Fact]
    public void SplitCorrectlyWithOneCrossingEdge()
    {
        SplitHorizontally();
        
        var coordA = new Vector3(0.5f, 0, 0);
        var coordB = new Vector3(0.5f, 0, 1f);
        var edgesForA = EdgeQueries.NearestEdgesToPoint(_testingFrame, coordA);
        var pointAndEdgeA = EdgeQueries.NearestPointOnEdgeToPoint(_testingFrame, coordA, edgesForA);
        var edgesForB = EdgeQueries.NearestEdgesToPoint(_testingFrame, coordB);
        var pointAndEdgeB = EdgeQueries.NearestPointOnEdgeToPoint(_testingFrame, coordB, edgesForB);

        var fold = new UnspecifiedFold
        {
            Vertices =
            {
                [0] = new Vertex { Coord = pointAndEdgeA.Item1 }, [1] = new Vertex { Coord = pointAndEdgeB.Item1 }
            },
            VertexEdges = { [0] = pointAndEdgeA.Item2, [1] = pointAndEdgeB.Item2 },
            VertexExists = { [0] = false, [1] = false }
        };
        
        fold.Apply(_testingFrame);

        Assert.Equivalent(4, _testingFrame.Faces.Count);
    }
    
    [Fact]
    public void SplitCorrectlyWhenCrossingVertex()
    {
        SplitHorizontally();
        SplitVertically();

        var startVertex = _testingFrame.Vertices[0];
        var endVertex = _testingFrame.Vertices[2];

        var fold = new UnspecifiedFold
        {
            Vertices = { [0] = startVertex, [1] = endVertex },
            VertexEdges = { [0] = null, [1] = null },
            VertexExists = { [0] = true, [1] = true }
        };
        fold.Apply(_testingFrame);
        
        Assert.Equivalent(6, _testingFrame.Faces.Count);
    }

    // todo: split vertex to edge
    
    // todo: fix split vertex to edge 0 bug

    private void SplitHorizontally()
    {
        var vertexA = new Vertex { Coord = new Vector3(0, 0, 0.5f) };
        var vertexB = new Vertex { Coord = new Vector3(1f, 0, 0.5f) };
        var edgeA = _testingFrame.Edges.ElementAt(3);
        var edgeB = _testingFrame.Edges.ElementAt(1); 

        new UnspecifiedFold
        {
            Vertices = { [0] = vertexA, [1] = vertexB },
            VertexEdges = { [0] = edgeA, [1] = edgeB },
            VertexExists = { [0] = false, [1] = false }
        }.Apply(_testingFrame);
    }
    
    private void SplitVertically()
    {
        var vertexA = new Vertex { Coord = new Vector3(0.5f, 0, 0) };
        var vertexB = new Vertex { Coord = new Vector3(0.5f, 0, 1f) };
        var edgeA = _testingFrame.Edges.ElementAt(0);
        var edgeB = _testingFrame.Edges.ElementAt(2); 

        new UnspecifiedFold
        {
            Vertices = { [0] = vertexA, [1] = vertexB },
            VertexEdges = { [0] = edgeA, [1] = edgeB },
            VertexExists = { [0] = false, [1] = false }
        }.Apply(_testingFrame);
    }
}
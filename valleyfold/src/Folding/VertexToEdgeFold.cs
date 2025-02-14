using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Folding;

public class VertexToEdgeFold : IFold
{
    private readonly Edge _edge;
    private readonly Vector3 _point;
    private readonly Id _vertexId;

    public VertexToEdgeFold(Edge edge, Vector3 point, Id vertexId)
    {
        _edge = edge;
        _point = point;
        _vertexId = vertexId;
    }

    public void Apply(Frame frame)
    {
        var newVertexId = EdgeCommands.AddVertexToEdge(frame, new Vertex { Coord = _point }, _edge);

        new VertexToVertexFold(_vertexId, newVertexId).Apply(frame);
    }
}
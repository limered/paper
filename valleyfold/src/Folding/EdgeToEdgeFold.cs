using System.Collections.Generic;
using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Folding;

public class EdgeToEdgeFold : IFold
{
    private readonly Edge _edgeA;
    private readonly Edge _edgeB;
    private readonly Vector2 _pointA;
    private readonly Vector2 _pointB;

    public EdgeToEdgeFold(Edge edgeA, Edge edgeB, Vector2 pointA, Vector2 pointB)
    {
        _edgeA = edgeA;
        _edgeB = edgeB;
        _pointA = pointA;
        _pointB = pointB;
    }

    public void Apply(Frame frame)
    {
        if (NewFoldOnSameEdge()) return;

        var vertexIds = new List<Id>
        {
            EdgeCommands.AddVertexToEdge(frame, new Vertex { Coord = _pointA }, _edgeA),
            EdgeCommands.AddVertexToEdge(frame, new Vertex { Coord = _pointB }, _edgeB)
        };

        new VertexToVertexFold(vertexIds[0], vertexIds[1]).Apply(frame);
    }

    private bool NewFoldOnSameEdge()
    {
        return _edgeA == _edgeB;
    }
}
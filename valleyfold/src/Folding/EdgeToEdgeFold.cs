using System.Collections.Generic;
using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Folding;

public class EdgeToEdgeFold : IFold
{
    private readonly Edge _edgeA;
    private readonly Edge _edgeB;
    private readonly Vector3 _pointA;
    private readonly Vector3 _pointB;

    public EdgeToEdgeFold(Edge edgeA, Edge edgeB, Vector3 pointA, Vector3 pointB)
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

        var faceToSplit = FaceQueries.FacesContainingVertexIds(frame, vertexIds);
        if (faceToSplit == null) return;

        FaceCommands.SplitFace(frame, faceToSplit, vertexIds[0], vertexIds[1]);

        frame.AddEdge(new Edge
        {
            Assignment = Assignment.U,
            FoldAngle = 0,
            Vertices = new[] { vertexIds[0], vertexIds[1] }
        });
    }

    private bool NewFoldOnSameEdge()
    {
        return _edgeA == _edgeB;
    }
}
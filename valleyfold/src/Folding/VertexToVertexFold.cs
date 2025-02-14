using System.Collections.Generic;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Folding;

public class VertexToVertexFold : IFold
{
    private readonly Id _vertexIdA;
    private readonly Id _vertexIdB;

    public VertexToVertexFold(Id vertexIdA, Id vertexIdB)
    {
        _vertexIdA = vertexIdA;
        _vertexIdB = vertexIdB;
    }

    public void Apply(Frame frame)
    {
        var faceToSplit = FaceQueries.FacesContainingVertexIds(frame, new List<Id> { _vertexIdA, _vertexIdB });
        if (faceToSplit == null) return;

        FaceCommands.SplitFace(frame, faceToSplit, _vertexIdA, _vertexIdB);

        frame.AddEdge(new Edge
        {
            Assignment = Assignment.U,
            FoldAngle = 0,
            Vertices = new[] { _vertexIdA, _vertexIdB }
        });
    }
}
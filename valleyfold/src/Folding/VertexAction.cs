using System;
using Godot;
using valleyfold.Fold;
using valleyfold.FrameModifications;

namespace valleyfold.Folding;

public class VertexAction
{
    private readonly Vector2 _endPoint;
    private readonly Id _selectedVertexId;

    public VertexAction(Id selectedVertexId, Vector2 endPoint)
    {
        _selectedVertexId = selectedVertexId;
        _endPoint = endPoint;
    }

    public EdgeStrip Draft(Frame frame)
    {
        var startVertex = frame.Vertices[_selectedVertexId];
        var direction = startVertex.Coord.DirectionTo(_endPoint);
        
        var center = startVertex.Coord.Lerp(_endPoint, 0.5f);
        var perpendicular = new Vector2(-direction.Y, direction.X);

        var polyWithCenter = FaceQueries.FaceContainingPoint(frame, center);
        if (polyWithCenter == null) return EdgeStrip.Empty;

        var edgeStrip = new EdgeStrip();
        
        var firstCrossings =
            EdgeQueries.CrossedEdgesOnPolygon(frame, center, perpendicular, polyWithCenter);

        edgeStrip.Add(firstCrossings);
        
        // March along edges
        
        return edgeStrip;
    }
}
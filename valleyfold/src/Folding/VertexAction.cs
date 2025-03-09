using System;
using System.Linq;
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

        var nextEdge = firstCrossings.edgeA;
        var lastPoint = firstCrossings.pointA;
        var lastPolygon = polyWithCenter;
        if (nextEdge.Assignment != Assignment.B)
        {
            if(nextEdge.Assignment == Assignment.V)
            {
                var edge = frame.Vertices[nextEdge.Vertices[0]].Coord
                    .DirectionTo(frame.Vertices[nextEdge.Vertices[1]].Coord); 
                perpendicular = perpendicular.Reflect(edge);
            }
            var nextPolygon = nextEdge.Faces().First(p => p != lastPolygon);
            var nextCrossings = EdgeQueries.CrossedEdgesOnPolygon(frame, lastPoint, perpendicular, nextPolygon);
            edgeStrip.Add(nextCrossings);
        }
        
        nextEdge = firstCrossings.edgeB;
        lastPoint = firstCrossings.pointB;
        lastPolygon = polyWithCenter;
        if (nextEdge.Assignment != Assignment.B)
        {
            if(nextEdge.Assignment == Assignment.V)
            {
                var edge = frame.Vertices[nextEdge.Vertices[0]].Coord
                    .DirectionTo(frame.Vertices[nextEdge.Vertices[1]].Coord); 
                perpendicular = perpendicular.Reflect(edge);
            }
            var nextPolygon = nextEdge.Faces().First(p => p != lastPolygon);
            var nextCrossings = EdgeQueries.CrossedEdgesOnPolygon(frame, lastPoint, perpendicular, nextPolygon);
            edgeStrip.Add(nextCrossings);
        }
        
        return edgeStrip;
    }
}
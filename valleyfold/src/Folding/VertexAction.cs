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
    private EdgeStrip _edgeStrip;

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

        _edgeStrip = new EdgeStrip();
        
        var firstCrossings =
            EdgeQueries.CrossedEdgesOnPolygon(frame, center, perpendicular, polyWithCenter);
        
        // Add Folds from Start Point
        var lastEdge = firstCrossings.edgeA;
        var lastPoint = firstCrossings.pointA;
        var lastPolygon = polyWithCenter;
        var foldDirection = perpendicular;
        while (lastEdge.Assignment != Assignment.B)
        {
            if(lastEdge.Assignment == Assignment.V)
            {
                var edge = frame.Vertices[lastEdge.Vertices[0]].Coord
                    .DirectionTo(frame.Vertices[lastEdge.Vertices[1]].Coord); 
                foldDirection = foldDirection.Reflect(edge);
            }
            var polygon = lastEdge.Faces().First(p => p != lastPolygon);
            var crossings = EdgeQueries.CrossedEdgesOnPolygon(frame, lastPoint, perpendicular, polygon);
            
            lastEdge = crossings.edgeA == lastEdge ? crossings.edgeB : crossings.edgeA;
            lastPoint = crossings.edgeA == lastEdge ? crossings.pointB : crossings.pointA;
            lastPolygon = polygon;
            
            _edgeStrip.Add(crossings);
        }
        
        // Add center fold
        _edgeStrip.Add(firstCrossings);
        
        //Add Folds from end point
        lastEdge = firstCrossings.edgeB;
        lastPoint = firstCrossings.pointB;
        lastPolygon = polyWithCenter;
        while (lastEdge.Assignment != Assignment.B)
        {
            if(lastEdge.Assignment == Assignment.V)
            {
                var edge = frame.Vertices[lastEdge.Vertices[0]].Coord
                    .DirectionTo(frame.Vertices[lastEdge.Vertices[1]].Coord); 
                perpendicular = perpendicular.Reflect(edge);
            }
            var polygon = lastEdge.Faces().First(p => p != lastPolygon);
            var crossings = EdgeQueries.CrossedEdgesOnPolygon(frame, lastPoint, perpendicular, polygon);
            
            lastEdge = crossings.edgeA == lastEdge ? crossings.edgeB : crossings.edgeA;
            lastPoint = crossings.edgeA == lastEdge ? crossings.pointB : crossings.pointA;
            lastPolygon = polygon;
            
            _edgeStrip.Add(crossings);
        }
        
        return _edgeStrip;
    }

    public void ApplyFoldedEdges(Frame frame)
    {
        _edgeStrip.Sort();
        
        new EdgeToEdgeFold(
                _edgeStrip.StartEdges[0], 
                _edgeStrip.EndEdges[0], 
                _edgeStrip.StartPoints[0], 
                _edgeStrip.EndPoints[0])
            .Apply(frame);
        
        if (_edgeStrip.Length > 1)
        {
            for (var i = 1; i < _edgeStrip.Length; i++)
            {
                var lastVertex = frame.Vertices.Count - 1;
                var point = _edgeStrip.EndPoints[i];
                var edge = _edgeStrip.EndEdges[i];

                new VertexToEdgeFold(edge,point, lastVertex)
                    .Apply(frame);
            }
        }
    }
}
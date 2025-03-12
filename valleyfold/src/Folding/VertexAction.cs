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

    public (Vector2 point, Edge edge)? CalculateEdgeCrossing(
        Frame frame, Vector2 lineA, Vector2 lineB, Face face, Edge edgeToSkip)
    {
        var faceEdges = face.Edges();
        for (var i = 0; i < faceEdges.Length; i++)
        {
            var edge = faceEdges[i];
            if (edge == edgeToSkip) continue;
            var point = LineSegmentCrossing(
                lineA, lineB,
                frame.Vertices[edge.Vertices[0]].Coord,
                frame.Vertices[edge.Vertices[1]].Coord);
            
            if(point is null) continue;
            return (point.Value, edge);
        }

        return default;
    }

    public Vector2? LineSegmentCrossing(
        Vector2 lineA, Vector2 lineB, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var lineDirection = lineB - lineA;
        var segmentDirection = segmentEnd - segmentStart;
        
        var denominator = lineDirection.X * segmentDirection.Y - lineDirection.Y * segmentDirection.X;
        
        // Parallel
        if (Math.Abs(denominator) < float.Epsilon)
            return null;
        
        var startDiff = new Vector2(segmentStart.X - lineA.X, segmentStart.Y - lineA.Y);
        var t = (startDiff.X * segmentDirection.Y - startDiff.Y * segmentDirection.X) / denominator;
        var s = (startDiff.X * lineDirection.Y - startDiff.Y * lineDirection.X) / denominator;
        
        if (s is >= 0 and <= 1)
        {
            return new Vector2(
                lineA.X + t * lineDirection.X,
                lineA.Y + t * lineDirection.Y);
        }
        return null;
    }

    public EdgeStrip Draft(Frame frame)
    {
        // TODO: Rebuild calculation to only use one point on edges
        // e.g. a->b->c->d, not: a->b,b->c,c->d

        // TODO: Support edges through vertex

        var startVertex = frame.Vertices[_selectedVertexId];
        var direction = startVertex.Coord.DirectionTo(_endPoint);

        var center = startVertex.Coord.Lerp(_endPoint, 0.5f);
        var perpendicular = new Vector2(-direction.Y, direction.X);

        var polyWithCenter = FaceQueries.FaceContainingPoint(frame, center);
        if (polyWithCenter == null) return EdgeStrip.Empty;

        _edgeStrip = new EdgeStrip();

        var firstCrossings = EdgeQueries
            .CrossedEdgesOnPolygon(frame, center, perpendicular, polyWithCenter);
        if (firstCrossings == default) return EdgeStrip.Empty;

        _edgeStrip.AddSingle(firstCrossings.Value.pointA, firstCrossings.Value.edgeA);
        _edgeStrip.AddSingle(firstCrossings.Value.pointB, firstCrossings.Value.edgeB);

        var lastEdge = firstCrossings.Value.edgeA;
        var lastPoint = firstCrossings.Value.pointA;
        var lastPolygon = polyWithCenter;
        var foldDirection = perpendicular;
        var moveToNext = true;
        while (lastEdge.Assignment != Assignment.B && moveToNext)
        {
            if(lastEdge.Assignment == Assignment.V)
            {
                var edge = frame.Vertices[lastEdge.Vertices[0]].Coord
                    .DirectionTo(frame.Vertices[lastEdge.Vertices[1]].Coord); 
                foldDirection = foldDirection.Reflect(edge);
            }
            var nextPolygon = lastEdge.Faces().First(p => p != lastPolygon);
            var nextCrossings = CalculateEdgeCrossing(
                frame, lastPoint, lastPoint + foldDirection, nextPolygon, lastEdge
            );
            if (nextCrossings == default) break;
            if (_edgeStrip.Edges.Contains(nextCrossings.Value.edge)) moveToNext = false;
            
            lastEdge = nextCrossings.Value.edge;
            lastPoint = nextCrossings.Value.point;
            lastPolygon = nextPolygon;

            _edgeStrip.PrependSingle(lastPoint, lastEdge);
        }
        
        lastEdge = firstCrossings.Value.edgeB;
        lastPoint = firstCrossings.Value.pointB;
        lastPolygon = polyWithCenter;
        foldDirection = perpendicular;
        moveToNext = true;
        while (lastEdge.Assignment != Assignment.B && moveToNext)
        {
            if(lastEdge.Assignment == Assignment.V)
            {
                var edge = frame.Vertices[lastEdge.Vertices[0]].Coord
                    .DirectionTo(frame.Vertices[lastEdge.Vertices[1]].Coord); 
                foldDirection = foldDirection.Reflect(edge);
            }
            var nextPolygon = lastEdge.Faces().First(p => p != lastPolygon);
            var nextCrossings = CalculateEdgeCrossing(
                frame, lastPoint, lastPoint + foldDirection, nextPolygon, lastEdge
            );
            if (nextCrossings == default) break;
            if (_edgeStrip.Edges.Contains(nextCrossings.Value.edge)) moveToNext = false;

            lastEdge = nextCrossings.Value.edge;
            lastPoint = nextCrossings.Value.point;
            lastPolygon = nextPolygon;

            _edgeStrip.AddSingle(lastPoint, lastEdge);
        }
        
        return _edgeStrip;
    }

    public void ApplyFoldedEdges(Frame frame)
    {
        new EdgeToEdgeFold(
                _edgeStrip.Edges[0],
                _edgeStrip.Edges[1],
                _edgeStrip.Points[0],
                _edgeStrip.Points[1],
                Assignment.V)
            .Apply(frame);

        if (_edgeStrip.Points.Count <= 2) return;
        
        for (var i = 2; i < _edgeStrip.Points.Count; i++)
        {
            var lastVertex = frame.Vertices.Count - 1;
            var point = _edgeStrip.Points[i];
            var edge = _edgeStrip.Edges[i];

            new VertexToEdgeFold(edge, point, lastVertex, Assignment.V)
                .Apply(frame);
        }
    }
}
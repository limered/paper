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
        Frame frame, Vector2 start, Vector2 end, Face face, Edge edgeToSkip)
    {
        var faceEdges = face.Edges();
        for (var i = 0; i < faceEdges.Length; i++)
        {
            var edge = faceEdges[i];
            if (edge == edgeToSkip) continue;
            if (EdgeQueries.EdgesIntersect(
                    frame.Vertices[edge.Vertices[0]].Coord,
                    frame.Vertices[edge.Vertices[1]].Coord,
                    start,
                    end))
            {
                var point = EdgeQueries.EdgeToEdgeIntersectionPoint(
                    new Vertex { Coord = start },
                    new Vertex { Coord = end },
                    frame.Vertices[edge.Vertices[0]],
                    frame.Vertices[edge.Vertices[1]]);
                return (point, edge);
            }
        }

        return default;
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

        // March crossings starting at start point
        var lastEdge = firstCrossings.Value.edgeA;
        var lastPoint = firstCrossings.Value.pointA;
        var lastPolygon = polyWithCenter;
        var foldDirection = perpendicular;
        while (lastEdge.Assignment != Assignment.B)
        {
            var nextPolygon = lastEdge.Faces().First(p => p != lastPolygon);
            var nextCrossings = CalculateEdgeCrossing(
                frame, lastPoint, lastPoint + foldDirection * 5f, nextPolygon, lastEdge
            );
            if (nextCrossings == default) break;

            lastEdge = nextCrossings.Value.edge;
            lastPoint = nextCrossings.Value.point;
            lastPolygon = nextPolygon;

            _edgeStrip.PrependSingle(lastPoint, lastEdge);
        }


        // // Add Folds from Start Point
        // var lastEdge = firstCrossings.edgeA;
        // var lastPoint = firstCrossings.pointA;
        // var lastPolygon = polyWithCenter;
        // var foldDirection = perpendicular;
        // while (lastEdge.Assignment != Assignment.B)
        // {
        //     if(lastEdge.Assignment == Assignment.V)
        //     {
        //         var edge = frame.Vertices[lastEdge.Vertices[0]].Coord
        //             .DirectionTo(frame.Vertices[lastEdge.Vertices[1]].Coord); 
        //         foldDirection = foldDirection.Reflect(edge);
        //     }
        //     var polygon = lastEdge.Faces().First(p => p != lastPolygon);
        //     var crossings = EdgeQueries.CrossedEdgesOnPolygon(frame, lastPoint, foldDirection, polygon);
        //     
        //     lastEdge = crossings.edgeA == lastEdge ? crossings.edgeB : crossings.edgeA;
        //     lastPoint = crossings.edgeA == lastEdge ? crossings.pointB : crossings.pointA;
        //     lastPolygon = polygon;
        //     
        //     _edgeStrip.Prepend(crossings);
        // }
        //
        // // Add center fold
        // _edgeStrip.Add(firstCrossings);
        //
        // //Add Folds from end point
        // lastEdge = firstCrossings.edgeB;
        // lastPoint = firstCrossings.pointB;
        // lastPolygon = polyWithCenter;
        // foldDirection = perpendicular;
        // while (lastEdge.Assignment != Assignment.B)
        // {
        //     if(lastEdge.Assignment == Assignment.V)
        //     {
        //         var edge = frame.Vertices[lastEdge.Vertices[0]].Coord
        //             .DirectionTo(frame.Vertices[lastEdge.Vertices[1]].Coord); 
        //         foldDirection = foldDirection.Reflect(edge);
        //     }
        //     var polygon = lastEdge.Faces().First(p => p != lastPolygon);
        //     var crossings = EdgeQueries.CrossedEdgesOnPolygon(frame, lastPoint, foldDirection, polygon);
        //     
        //     lastEdge = crossings.edgeA == lastEdge ? crossings.edgeB : crossings.edgeA;
        //     lastPoint = crossings.edgeA == lastEdge ? crossings.pointB : crossings.pointA;
        //     lastPolygon = polygon;
        //     
        //     _edgeStrip.Add(crossings);
        // }

        _edgeStrip.Sort();
        return _edgeStrip;
    }

    public void ApplyFoldedEdges(Frame frame)
    {
        _edgeStrip.Sort();

        new EdgeToEdgeFold(
                _edgeStrip.StartEdges[0],
                _edgeStrip.EndEdges[0],
                _edgeStrip.StartPoints[0],
                _edgeStrip.EndPoints[0],
                Assignment.V)
            .Apply(frame);

        if (_edgeStrip.Length > 1)
            for (var i = 1; i < _edgeStrip.Length; i++)
            {
                var lastVertex = frame.Vertices.Count - 1;
                var point = _edgeStrip.EndPoints[i];
                var edge = _edgeStrip.EndEdges[i];

                new VertexToEdgeFold(edge, point, lastVertex)
                    .Apply(frame);
            }
    }
}
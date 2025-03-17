using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.FrameModifications;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Folding;

public class EdgeInteraction : IInteraction
{
    private readonly Vector2 _startPoint;
    private readonly Vector2 _endPoint;
    private readonly Edge _selectedEdge;
    private EdgeStrip _edgeStrip;

    public EdgeInteraction(Vector2 startPoint, Vector2 endPoint, Edge selectedEdge)
    {
        _startPoint = startPoint;
        _endPoint = endPoint;
        _selectedEdge = selectedEdge;
    }

    private void MarchEdges(
        Frame frame, Edge startEdge, Vector2 startPoint, Face startFace, Vector2 foldDirection, bool prepend
    )
    {
        var lastEdge = startEdge;
        var lastPoint = startPoint;
        var lastPolygon = startFace;
        var moveToNext = true;
        while (lastEdge.Assignment != Assignment.B && moveToNext)
        {
            if (lastEdge.Assignment == Assignment.V)
            {
                var edge = frame.Vertices[lastEdge.Vertices[0]].Coord
                    .DirectionTo(frame.Vertices[lastEdge.Vertices[1]].Coord);
                foldDirection = foldDirection.Reflect(edge);
            }

            var nextPolygon = lastEdge.Faces().First(p => p != lastPolygon);
            var nextCrossings = FoldMath.CalculateEdgeOtherEdgeCrossing(
                frame, lastPoint, lastPoint + foldDirection, nextPolygon, lastEdge
            );
            if (nextCrossings == default) break;
            if (_edgeStrip.Edges.Contains(nextCrossings.Value.edge)) moveToNext = false;

            lastEdge = nextCrossings.Value.edge;
            lastPoint = nextCrossings.Value.point;
            lastPolygon = nextPolygon;

            if (prepend)
                _edgeStrip.PrependSingle(lastPoint, lastEdge);
            else
                _edgeStrip.AddSingle(lastPoint, lastEdge);
        }
    }
    
    public EdgeStrip Draft(Frame frame)
    {
        var direction = _startPoint.DirectionTo(_endPoint);
        var center = _startPoint.Lerp(_endPoint, 0.5f);
        var perpendicular = new Vector2(-direction.Y, direction.X);
        
        var polyWithCenter = FaceQueries.FaceContainingPoint(frame, center);
        if (polyWithCenter == null) return EdgeStrip.Empty;
        
        _edgeStrip = new EdgeStrip();

        var firstCrossings = EdgeQueries
            .CrossedEdgesOnPolygon(frame, center, perpendicular, polyWithCenter);
        if (firstCrossings == default) return EdgeStrip.Empty;

        _edgeStrip.AddSingle(firstCrossings.Value.pointA, firstCrossings.Value.edgeA);
        _edgeStrip.AddSingle(firstCrossings.Value.pointB, firstCrossings.Value.edgeB);

        MarchEdges(
            frame, firstCrossings.Value.edgeA, firstCrossings.Value.pointA, polyWithCenter, perpendicular, true
        );
        MarchEdges(
            frame, firstCrossings.Value.edgeB, firstCrossings.Value.pointB, polyWithCenter, perpendicular, false
        );

        return _edgeStrip;
    }

    public ChangeRecord ApplyFoldedEdges(Frame frame)
    {
        var (startId, endId) = new EdgeToEdgeFold(
                _edgeStrip.Edges[0],
                _edgeStrip.Edges[1],
                _edgeStrip.Points[0],
                _edgeStrip.Points[1],
                Assignment.V)
            .Apply(frame);
        
        var changeRecord = new ChangeRecord()
        {
            FoldLine = (startId, endId),
            StartPoint = _startPoint.Vector3XZ(), // TODO: use 3d coord
        };

        if (_edgeStrip.Points.Count <= 2) return changeRecord;

        for (var i = 2; i < _edgeStrip.Points.Count; i++)
        {
            var lastVertex = frame.Vertices.Count - 1;
            var point = _edgeStrip.Points[i];
            var edge = _edgeStrip.Edges[i];

            new VertexToEdgeFold(edge, point, lastVertex, Assignment.V)
                .Apply(frame);
        }

        return changeRecord;
    }
}
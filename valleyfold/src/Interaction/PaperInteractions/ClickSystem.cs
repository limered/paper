using System.Linq;
using Godot;
using valleyfold.Fold;
using valleyfold.Folding;
using valleyfold.FrameModifications;
using valleyfold.Render.Edges;

namespace valleyfold.Interaction.PaperInteractions;

public class WorldNewPointData
{
    public Vector3 Coord;
    public Edge Edge;
    public Id ExistingVertex;
    public bool IsOnEdge;

    public WorldNewPointData Copy()
    {
        return new WorldNewPointData
        {
            Coord = Coord,
            ExistingVertex = ExistingVertex,
            IsOnEdge = IsOnEdge,
            Edge = Edge
        };
    }
}

public enum PickingMode
{
    Buttons,
    StartPoint,
    EndPoint
}

public partial class ClickSystem : Node3D
{
    private Vector3 _mouseWorldPosition;
    private PickingMode _pickingMode = PickingMode.StartPoint;
    private WorldNewPointData _startPoint;
    private WorldNewPointData _tempPointData = new();
    [Export] public Node3D FoldStartPoint;
    [Export] public Node3D GhostClickPosition;
    [Export] public EdgeLine GhostEdgeLine;
    [Export] public Area3D MouseCollision;
    [Export] public float VertexPickThreshold = 0.1f;

    public override void _Ready()
    {
        MouseCollision.InputEvent += MouseCollisionOnInputEvent;
    }

    private void MouseCollisionOnInputEvent(
        Node camera,
        InputEvent @event,
        Vector3 eventPosition,
        Vector3 normal,
        long shapeIdx)
    {
        _mouseWorldPosition = eventPosition;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return;
        if (mouseEvent.ButtonIndex != MouseButton.Left || !mouseEvent.Pressed) return;
        if (_pickingMode == PickingMode.Buttons) return;
        if (_pickingMode == PickingMode.StartPoint)
        {
            _startPoint = _tempPointData.Copy();
            _pickingMode = PickingMode.EndPoint;
            return;
        }

        if (_pickingMode == PickingMode.EndPoint)
        {
            // set end point
            // calculate fold between points
        }
        

        // if (_startPoint == null)
        // {
        //     
        // }
        // else if (_secondPoint == null)
        // {
        //     _secondPoint = _tempPointData.Copy();
        // }
        // else
        // {
        //     if (_startPoint.IsOnEdge && _secondPoint.IsOnEdge)
        //         new EdgeToEdgeFold(
        //                 _startPoint.Edge,
        //                 _secondPoint.Edge,
        //                 _startPoint.Coord,
        //                 _secondPoint.Coord)
        //             .Apply(Statics.Frame);
        //     else if (_startPoint.IsOnEdge)
        //         new VertexToEdgeFold(
        //                 _startPoint.Edge,
        //                 _startPoint.Coord,
        //                 _secondPoint.ExistingVertex)
        //             .Apply(Statics.Frame);
        //     else if (_secondPoint.IsOnEdge)
        //         new VertexToEdgeFold(
        //                 _secondPoint.Edge,
        //                 _secondPoint.Coord,
        //                 _startPoint.ExistingVertex)
        //             .Apply(Statics.Frame);
        //     else
        //         new VertexToVertexFold(
        //                 _startPoint.ExistingVertex,
        //                 _secondPoint.ExistingVertex)
        //             .Apply(Statics.Frame);
        //
        //     _startPoint = null;
        //     _secondPoint = null;
        // }
    }

    private WorldNewPointData NearestPointToPoint(Frame frame, Vector3 point)
    {
        var result = new WorldNewPointData();
        var nearestVertexId = VertexQueries.NearestVertexIdTo(frame, point, VertexPickThreshold);
        if (nearestVertexId > -1)
        {
            result.ExistingVertex = nearestVertexId;
            result.Coord = frame.Vertices[nearestVertexId].Coord;
            return result;
        }

        var nearestEdges = EdgeQueries.NearestEdgesToPoint(frame, point);
        if (nearestEdges.Any())
        {
            var pointAndEdge = EdgeQueries.NearestPointOnEdgeToPoint(frame, point, nearestEdges);
            result.Coord = pointAndEdge.Item1;
            result.Edge = pointAndEdge.Item2;
            result.Edge.IsSelected = true;
            result.IsOnEdge = true;
            return result;
        }

        result.ExistingVertex = VertexQueries.NearestVertexIdTo(frame, point);
        result.Coord = frame.Vertices[result.ExistingVertex].Coord;
        return result;
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        frame.UnmarkEdges();
        frame.UnmarkVertices();

        if (_pickingMode == PickingMode.StartPoint)
        {
            _tempPointData = NearestPointToPoint(frame, _mouseWorldPosition);
        }
        else
        {
            _tempPointData.Coord = _mouseWorldPosition;

            // render ghost edge
            if (GhostEdgeLine == null)
            {
                GhostEdgeLine = new EdgeLine();
                AddChild(GhostEdgeLine);
            }
            // calculate ghost edge line coords
            var center = _startPoint.Coord.Lerp(_tempPointData.Coord, 0.5f);
            var direction = _startPoint.Coord.DirectionTo(_tempPointData.Coord);
            var perpendicular = new Vector3(-direction.Z, 0, direction.X);
            
            var (isValidFold, start, end) = EdgeQueries.FoldBoardersVertices(frame, center, perpendicular);

            if (isValidFold)
            {
                GhostEdgeLine.LinePositions(new Vector3(start.X, 0, start.Y), new Vector3(end.X, 0, end.Y));
                GhostEdgeLine.Draw();
            }
            else
            {
                GhostEdgeLine.Clear();
            }
        }

        GhostClickPosition.Position = _tempPointData.Coord;

        FoldStartPoint.Position = _startPoint?.Coord ?? new Vector3(100, 100, 100);
    }
}
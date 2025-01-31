using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Fold;
using valleyfold.Folding;

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

public partial class ClickSystem : Node3D
{
    private WorldNewPointData _tempPointData = new();
    private WorldNewPointData _firstPoint;

    private Vector3 _mouseWorldPosition;
    private WorldNewPointData _secondPoint;
    [Export] public Node3D FirstNewVertex;
    [Export] public Node3D GhostClickPosition;
    [Export] public Area3D MouseCollision;
    [Export] public Node3D SecondNewVertex;
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

        if (_firstPoint == null)
        {
            _firstPoint = _tempPointData.Copy();
        }
        else if (_secondPoint == null)
        {
            _secondPoint = _tempPointData.Copy();
        }
        else
        {
            var firstId = _firstPoint.ExistingVertex;
            if (_firstPoint.IsOnEdge) firstId = Statics.Frame.AddVertexOnEdge(_firstPoint.Coord, _firstPoint.Edge);

            var secondId = _secondPoint.ExistingVertex;
            if (_secondPoint.IsOnEdge) secondId = Statics.Frame.AddVertexOnEdge(_secondPoint.Coord, _secondPoint.Edge);

            new Valleyfold { Vertices = new List<Id> { firstId, secondId } }.Apply(Statics.Frame);

            _firstPoint = null;
            _secondPoint = null;
        }
    }

    private WorldNewPointData NearestPointToPoint(Frame frame, Vector3 point)
    {
        var result = new WorldNewPointData();
        var nearestVertexId = frame.NearestVertexIdTo(point, VertexPickThreshold);
        if (nearestVertexId > -1)
        {
            result.ExistingVertex = nearestVertexId;
            result.Coord = frame.Vertices[nearestVertexId].Coord;
            return result;
        }

        var nearestEdges = frame.NearestEdgesTo(point);
        if (nearestEdges.Any())
        {
            var pointAndEdge = frame.NearestPointOnEdgeTo(point, nearestEdges);
            result.Coord = pointAndEdge.Item1;
            result.Edge = pointAndEdge.Item2;
            result.Edge.IsSelected = true;
            result.IsOnEdge = true;
            return result;
        }

        result.ExistingVertex = frame.NearestVertexIdTo(point);
        result.Coord = frame.Vertices[result.ExistingVertex].Coord;
        return result;
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        _tempPointData = NearestPointToPoint(frame, _mouseWorldPosition);

        GhostClickPosition.Position = _tempPointData.Coord;

        FirstNewVertex.Position = _firstPoint?.Coord ?? new Vector3(100, 100, 100);
        SecondNewVertex.Position = _secondPoint?.Coord ?? new Vector3(100, 100, 100);
    }
}
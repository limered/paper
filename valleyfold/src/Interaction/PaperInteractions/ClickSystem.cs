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
    private readonly WorldNewPointData _tempPointData = new();
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
            if (_firstPoint.IsOnEdge)
            {
                firstId = Statics.Frame.AddVertexOnEdge(_firstPoint.Coord, _firstPoint.Edge);
            }

            var secondId = _secondPoint.ExistingVertex;
            if (_secondPoint.IsOnEdge)
            {
                secondId = Statics.Frame.AddVertexOnEdge(_secondPoint.Coord, _secondPoint.Edge);
            }

            new Valleyfold { Vertices = new List<Id> { firstId, secondId } }.Apply(Statics.Frame);
            
            _firstPoint = null;
            _secondPoint = null;
        }
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        
        var nearestVertexId = frame.NearestVertexIdTo(_mouseWorldPosition);
        var nearestVertexCoord = frame.Vertices[nearestVertexId].Coord;
        
        var nearestEdges = frame.NearestEdgesTo(_mouseWorldPosition);
        (Vector3, Edge) pointAndEdge = (Vector3.Inf, null);
        if (nearestEdges.Any())
        {
            pointAndEdge = frame.NearestPointOnEdgeTo(_mouseWorldPosition, nearestEdges);
        }

        if (nearestVertexId > -1)
        {
            var movedNearestVertex = nearestVertexCoord + 
                                     nearestVertexCoord.DirectionTo(_mouseWorldPosition) * VertexPickThreshold;
            if (movedNearestVertex.DistanceSquaredTo(_mouseWorldPosition) <
                pointAndEdge.Item1.DistanceSquaredTo(_mouseWorldPosition))
            {
                // choose point
                _tempPointData.IsOnEdge = false;
                if (_tempPointData.Edge != null)
                {
                    _tempPointData.Edge.IsSelected = false;
                    _tempPointData.Edge = null;
                }
                _tempPointData.ExistingVertex = nearestVertexId;
                _tempPointData.Coord = nearestVertexCoord;
            }
            else
            {
                // choose edge
                if (_tempPointData.Edge != null && pointAndEdge.Item2 != _tempPointData.Edge)
                {
                    _tempPointData.Edge.IsSelected = false;
                }
                _tempPointData.Coord = pointAndEdge.Item1;
                _tempPointData.Edge = pointAndEdge.Item2;
                _tempPointData.Edge!.IsSelected = true;
                _tempPointData.IsOnEdge = true;
            }
        }

        GhostClickPosition.Position = _tempPointData.Coord;

        FirstNewVertex.Position = _firstPoint?.Coord ?? new Vector3(-100, 0, -100);
        SecondNewVertex.Position = _secondPoint?.Coord ?? new Vector3(-100, 0, -100);
    }
}
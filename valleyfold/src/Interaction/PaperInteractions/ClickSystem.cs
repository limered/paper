using System.Linq;
using Godot;
using valleyfold.Fold;

namespace valleyfold.Interaction.PaperInteractions;

public class WorldNewPointData
{
    public Vector3 Coord;
    public bool IsOnEdge;
    public Id ExistingVertex;
}

public partial class ClickSystem : Node3D
{
    [Export] public Node3D GhostClickPosition;
    [Export] public Node3D FirstNewVertex;
    [Export] public Node3D SecondNewVertex;
    [Export] public Area3D MouseCollision;

    private Vector3 _mouseWorldPosition;
    private readonly WorldNewPointData _tempPointData = new();
    private WorldNewPointData _firstPoint;
    private WorldNewPointData _secondPoint;

    public override void _Ready()
    {
        MouseCollision.InputEvent += MouseCollisionOnInputEvent;
    }

    private void MouseCollisionOnInputEvent(
        Node camera,
        InputEvent @event,
        Vector3 eventposition,
        Vector3 normal,
        long shapeidx)
    {
        _mouseWorldPosition = eventposition;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return;
        if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            if (_firstPoint == null)
            {
                _firstPoint = new WorldNewPointData()
                {
                    Coord = _tempPointData.Coord,
                    ExistingVertex = _tempPointData.ExistingVertex,
                    IsOnEdge = _tempPointData.IsOnEdge
                };
            }
            else if (_secondPoint == null)
            {
                _secondPoint = new WorldNewPointData()
                {
                    Coord = _tempPointData.Coord,
                    ExistingVertex = _tempPointData.ExistingVertex,
                    IsOnEdge = _tempPointData.IsOnEdge
                };
                
                // accept cut
            }
            else
            {
                _firstPoint = null;
                _secondPoint = null;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;

        var nearestEdges = Statics.Frame.NearestEdgesTo(_mouseWorldPosition);
        _tempPointData.ExistingVertex = Statics.Frame.NearestVertexIdTo(_mouseWorldPosition);
        _tempPointData.Coord = Statics.Frame.Vertices[_tempPointData.ExistingVertex].Coord;
        _tempPointData.IsOnEdge = false;
        if (nearestEdges.Any())
        {
            var pointOnEdge = Statics.Frame.NearestPointOnEdgeTo(_mouseWorldPosition, nearestEdges);
            if (pointOnEdge.DistanceSquaredTo(_mouseWorldPosition) <
                _tempPointData.Coord.DistanceSquaredTo(_mouseWorldPosition))
            {
                _tempPointData.Coord = pointOnEdge;
                _tempPointData.IsOnEdge = true;
            }
        }

        GhostClickPosition.Position = _tempPointData.Coord;

        FirstNewVertex.Position = _firstPoint?.Coord ?? new Vector3(-100, 0, -100);
        SecondNewVertex.Position = _secondPoint?.Coord ?? new Vector3(-100, 0, -100);
    }
}
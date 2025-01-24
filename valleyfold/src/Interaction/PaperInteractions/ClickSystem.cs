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
    [Export] public Area3D MouseCollision;

    private Vector3 _mouseWorldPosition;
    private WorldNewPointData _newPointData;

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
        if (@event is InputEventMouseButton mouseEvent)
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                // accept point
            }
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;

        var nearestEdges = Statics.Frame.NearestEdgesTo(_mouseWorldPosition);
        _newPointData.ExistingVertex = Statics.Frame.NearestVertexIdTo(_mouseWorldPosition);
        _newPointData.Coord = Statics.Frame.Vertices[_newPointData.ExistingVertex].Coord;
        _newPointData.IsOnEdge = false;
        if (nearestEdges.Any())
        {
            var pointOnEdge = Statics.Frame.NearestPointOnEdgeTo(_mouseWorldPosition, nearestEdges);
            if (pointOnEdge.DistanceSquaredTo(_mouseWorldPosition) <
                _newPointData.Coord.DistanceSquaredTo(_mouseWorldPosition))
            {
                _newPointData.Coord = pointOnEdge;
                _newPointData.IsOnEdge = true;
            }
        }

        GhostClickPosition.Position = _newPointData.Coord;
    }
}
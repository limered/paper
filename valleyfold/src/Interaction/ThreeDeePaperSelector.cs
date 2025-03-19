using System.Linq;
using Godot;
using valleyfold.Interaction.PaperInteractions;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;

namespace valleyfold.Interaction;

public partial class ThreeDeePaperSelector : Node3D
{
    private PickingMode _lastPickingMode = PickingMode.StartPoint;
    private Vector3 _mouseWorldPosition;
    private PickingMode _pickingMode = PickingMode.StartPoint;
    [Export] public Area3D MouseCollisionArea;
    [Export] public Node3D MouseMarker;

    public override void _Ready()
    {
        MouseCollisionArea.InputEvent += MouseCollisionAreaOnInputEvent;
        MouseCollisionArea.MouseExited += ChangeToButtonsPicking;
        MouseCollisionArea.MouseEntered += ChangeToLastPaperPicking;
    }

    private void ChangeToLastPaperPicking()
    {
        _pickingMode = _lastPickingMode;
    }

    private void ChangeToButtonsPicking()
    {
        _lastPickingMode = _pickingMode;
        _pickingMode = PickingMode.Buttons;
    }

    private void MouseCollisionAreaOnInputEvent(
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
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        if (@event is not InputEventMouseButton mouseEvent) return;

        switch (_pickingMode)
        {
            case PickingMode.Buttons:
                return;
            case PickingMode.StartPoint when mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed:
                StartFoldInteraction(frame);
                _pickingMode = PickingMode.EndPoint;
                return;
            case PickingMode.EndPoint when mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed:
                ConfirmFoldInteraction(frame);
                _pickingMode = PickingMode.StartPoint;
                break;
            case PickingMode.EndPoint when mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed:
                ResetFoldInteraction();
                _pickingMode = PickingMode.StartPoint;
                break;
            default:
                return;
        }
    }

    private void ResetFoldInteraction()
    {
    }

    private void ConfirmFoldInteraction(Frame frame)
    {
    }

    private void StartFoldInteraction(Frame frame)
    {
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;
        
        frame.UnmarkVertices();
        frame.UnmarkEdges();
        
        if (_pickingMode == PickingMode.Buttons) return;
        if (_pickingMode == PickingMode.StartPoint)
        {
            MouseMarker.GlobalPosition = _mouseWorldPosition;
            MarkVertexInFrame(frame3d, _mouseWorldPosition);
            return;
        }
    }

    private static void MarkVertexInFrame(Frame3D frame3d, Vector3 mouseWorldPosition)
    {
        if(frame3d.Vertices is null || !frame3d.Vertices.Any()) return;
        var closestId = 0;
        var closestDistance = float.MaxValue;
        for (var i = 0; i < frame3d.Vertices.Count; i++)
        {
            var vertex = frame3d.Vertices[i].Coord;
            var distance = vertex.DistanceTo(mouseWorldPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestId = i;
            }
        }
        Statics.Frame.Vertices[closestId].IsSelected = true;
    }
}
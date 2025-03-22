using System.Linq;
using Godot;
using valleyfold.ChangeTracking;
using valleyfold.Folding;
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

    private Id _pickedVertex;
    private PreviewLine _previewLine;

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
        _mouseWorldPosition.Y = 0;
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
        _previewLine?.ChangeVisibility(false);
    }

    private void ConfirmFoldInteraction(Frame frame)
    {
        FoldInteractionApplier.ApplyVertexValleyFold(_pickedVertex, _mouseWorldPosition);
        _previewLine?.ChangeVisibility(false);
    }

    private void StartFoldInteraction(Frame frame)
    {
        if (_previewLine is null)
        {
            _previewLine = new PreviewLine();
            AddChild(_previewLine);
        }

        _previewLine.ChangeVisibility(true);
        _previewLine.LineColor(Colors.Aqua);
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;
        var frame3d = Statics.Frame3d;

        frame.UnmarkVertices();
        frame.UnmarkEdges();

        switch (_pickingMode)
        {
            case PickingMode.Buttons:
                return;
            case PickingMode.StartPoint:
                _pickedVertex = MarkVertexInFrame(frame3d, _mouseWorldPosition);
                break;
            case PickingMode.EndPoint:
                UpdateFoldPreview(frame3d, _pickedVertex, _mouseWorldPosition, _previewLine);
                break;
        }

        MouseMarker.GlobalPosition = _mouseWorldPosition;
    }

    private static Id MarkVertexInFrame(Frame3D frame3d, Vector3 mouseWorldPosition)
    {
        if (frame3d.Vertices is null || !frame3d.Vertices.Any()) return -1;
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
        return closestId;
    }

    private static void UpdateFoldPreview(
        Frame3D frame3d,
        Id pickedVertex,
        Vector3 mouseWorldPosition,
        PreviewLine previewLine)
    {
        var centerPoint = frame3d.Vertices[pickedVertex].Coord.Lerp(mouseWorldPosition, 0.5f);
        var direction = (mouseWorldPosition - frame3d.Vertices[pickedVertex].Coord).Normalized();
        var perpendicular = new Vector3(-direction.Z, 0, direction.X);
        previewLine.UpdatePosition(centerPoint, perpendicular);

        previewLine.Draw();
    }
}
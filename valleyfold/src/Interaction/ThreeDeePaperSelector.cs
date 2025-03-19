using Godot;
using valleyfold.Interaction.PaperInteractions;
using valleyfold.TwoDeeModels;

namespace valleyfold.Interaction;

public partial class ThreeDeePaperSelector : Node3D
{
    private PickingMode _lastPickingMode;
    private Vector3 _mouseWorldPosition;
    private PickingMode _pickingMode;
    [Export] public Area3D MouseCollisionArea;

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
}
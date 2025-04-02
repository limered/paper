using Godot;
using valleyfold.Interaction.Events;
using valleyfold.Utils;

namespace valleyfold.Interaction;

public class MousePosition
{
    private Vector3 _mouseWorldPosition;
    public Vector3 Current => _mouseWorldPosition;
    
    public void Init(Area3D mouseCollisionArea)
    {
        mouseCollisionArea.InputEvent += MouseCollisionAreaOnInputEvent;
        mouseCollisionArea.MouseExited += ChangeToButtonsPicking;
        mouseCollisionArea.MouseEntered += ChangeToLastPaperPicking;
    }
    
    private static void ChangeToLastPaperPicking()
    {
        EventBus.Emit(new HoverAreaChangedEvent(){IsHovered = true});
    }

    private static void ChangeToButtonsPicking()
    {
        EventBus.Emit(new HoverAreaChangedEvent(){IsHovered = false});
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
}
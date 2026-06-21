using Godot;

namespace valleyfold.Interaction;

public class MousePosition
{
    private Vector3 _mouseWorldPosition;
    public Vector3 Current => _mouseWorldPosition;

    public void Init(Area3D mouseCollisionArea)
    {
        mouseCollisionArea.InputEvent += MouseCollisionAreaOnInputEvent;
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
using Godot;

namespace valleyfold.Interaction.PaperInteractions;

public partial class ClickSystem : Node3D
{
    [Export] public Node3D GhostClickPosition;

    [Export] public Area3D MouseCollision;

    private Vector3 MouseWorldPosition;

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
        MouseWorldPosition = eventposition;
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
        // get the nearest edge to mouse pos
    }
}
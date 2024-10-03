using Godot;

namespace valleyfold.Rendering;

public partial class CornerHandleNode : Node3D
{
    private MeshInstance3D _mesh;

    public override void _Ready()
    {
        var area = GetNodeOrNull<Area3D>("Area3D");
        area.MouseEntered += AreaOnMouseEntered;
        area.MouseExited += AreaOnMouseExited;
        area.InputEvent += AreaOnInputEvent;

        _mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
    }

    private void AreaOnInputEvent(Node camera, InputEvent @event, Vector3 eventposition, Vector3 normal, long shapeidx)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
        }
    }

    private void AreaOnMouseExited()
    {
        _mesh.Scale *= 1.0f / 1.1f;
    }

    private void AreaOnMouseEntered()
    {
        _mesh.Scale *= 1.1f;
    }
}
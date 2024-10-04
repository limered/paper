using Godot;
using Corner = valleyfold.Models.Corner;

namespace valleyfold.Rendering;

public partial class CornerHandleNode : Node3D
{
    private Camera3D _camera;
    private bool _isGrabbed;
    private MeshInstance3D _mesh;
    
    public Corner Corner { get; set; }

    public override void _Ready()
    {
        var area = GetNodeOrNull<Area3D>("Area3D");
        area.MouseEntered += AreaOnMouseEntered;
        area.MouseExited += AreaOnMouseExited;
        area.InputEvent += AreaOnInputEvent;

        _mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        _camera = GetViewport().GetCamera3D();
    }

    public override void _Process(double delta)
    {
        if (!_isGrabbed) return;

        var mousePosition = GetViewport().GetMousePosition();
        var origin = _camera.ProjectRayOrigin(mousePosition);
        var direction = _camera.ProjectRayNormal(mousePosition);
        var dropPlane = new Plane(Vector3.Up, 0);
        var camPos = dropPlane.IntersectsRay(origin, direction);
        Position = camPos ?? Position;
    }

    private void AreaOnInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, long shapeIdx)
    {
        if (@event is not InputEventMouseButton inputEvent) return;
        if (inputEvent.ButtonIndex != MouseButton.Left) return;
        _isGrabbed = inputEvent.Pressed;
        
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
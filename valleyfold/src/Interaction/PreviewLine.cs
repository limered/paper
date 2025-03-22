using Godot;

namespace valleyfold.Interaction;

public partial class PreviewLine : MeshInstance3D
{
    private Vector3 _center;
    private Vector3 _direction;
    private bool _isVisible;

    private ImmediateMesh _lineMesh;
    private ShaderMaterial _shaderMaterial;
    private float _width = 0.01f;

    public override void _Ready()
    {
        _lineMesh = new ImmediateMesh();
        Mesh = _lineMesh;

        _shaderMaterial = new ShaderMaterial();
        _shaderMaterial.Shader = ResourceLoader
            .Load<Shader>("res://src/Render/Edges/edgeline.gdshader");
        MaterialOverride = _shaderMaterial;
    }

    public void UpdatePosition(Vector3 center, Vector3 direction)
    {
        _center = center;
        _direction = direction;
    }

    public void ChangeVisibility(bool visible)
    {
        _isVisible = visible;
    }

    public void LineWidth(float width)
    {
        _width = width;
    }

    public void LineColor(Color color)
    {
        _shaderMaterial?.SetShaderParameter("line_color", color);
    }

    public void Draw()
    {
        _lineMesh.ClearSurfaces();
        if (!_isVisible) return;

        _lineMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

        var start = _center - _direction * 5;
        var end = _center + _direction * 5;

        var direction = _direction.Normalized();
        var perpendicular = direction.Cross(Vector3.Up).Normalized() * _width;

        var v1 = start - perpendicular;
        var v2 = start + perpendicular;
        var v3 = end - perpendicular;
        var v4 = end + perpendicular;

        _lineMesh.SurfaceAddVertex(v4);
        _lineMesh.SurfaceAddVertex(v2);
        _lineMesh.SurfaceAddVertex(v1);

        _lineMesh.SurfaceAddVertex(v3);
        _lineMesh.SurfaceAddVertex(v4);
        _lineMesh.SurfaceAddVertex(v1);

        _lineMesh.SurfaceEnd();
    }
}
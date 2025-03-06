using Godot;

namespace valleyfold.Render.Edges;

public partial class EdgeLine : MeshInstance3D
{
    private Vector3 _start;
    private Vector3 _end;
    
    private ImmediateMesh _lineMesh;
    private ShaderMaterial _shaderMaterial;
    private float _width = 0.01f;


    public override void _Ready()
    {
        _lineMesh = new ImmediateMesh();
        Mesh = _lineMesh;

        _shaderMaterial = new ShaderMaterial();
        _shaderMaterial.Shader = ResourceLoader.Load<Shader>("res://src/Render/Edges/edgeline.gdshader");
        MaterialOverride = _shaderMaterial;
    }

    public void LinePositions(Vector3 start, Vector3 end)
    {
        _start = start;
        _end = end;
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
        _lineMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

        var direction = (_end - _start).Normalized();
        var perpendicular = direction.Cross(Vector3.Up).Normalized() * _width;

        var v1 = _start - perpendicular;
        var v2 = _start + perpendicular;
        var v3 = _end - perpendicular;
        var v4 = _end + perpendicular;

        _lineMesh.SurfaceAddVertex(v4);
        _lineMesh.SurfaceAddVertex(v2);
        _lineMesh.SurfaceAddVertex(v1);

        _lineMesh.SurfaceAddVertex(v3);
        _lineMesh.SurfaceAddVertex(v4);
        _lineMesh.SurfaceAddVertex(v1);

        _lineMesh.SurfaceEnd();
    }

    public void Clear()
    {
        _lineMesh.ClearSurfaces();
    }
}
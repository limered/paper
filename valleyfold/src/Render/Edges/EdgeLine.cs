using System.Collections.Generic;
using Godot;

namespace valleyfold.Render.Edges;

public partial class EdgeLine : MeshInstance3D
{
    private readonly List<Vector3> _end = new();
    private readonly List<Vector3> _start = new();

    private ImmediateMesh _lineMesh;
    // Front: depth_test enabled — gets occluded by paper. Back: depth_test
    // disabled, render_priority -1 — drawn first, peeks through where front
    // got occluded. Selected: always-on-top single pass, bypasses the
    // two-pass occlusion cue (Q4.1).
    private ShaderMaterial _frontMaterial;
    private ShaderMaterial _backMaterial;
    private ShaderMaterial _selectedMaterial;
    private float _width = 0.01f;
    private bool _selected;

    public override void _Ready()
    {
        _lineMesh = new ImmediateMesh();
        Mesh = _lineMesh;

        var front = ResourceLoader.Load<Shader>("res://src/Render/Edges/edgeline.gdshader");
        var back = ResourceLoader.Load<Shader>("res://src/Render/Edges/edgeline_back.gdshader");

        _frontMaterial = new ShaderMaterial { Shader = front };
        _backMaterial = new ShaderMaterial { Shader = back, RenderPriority = -1 };
        _selectedMaterial = new ShaderMaterial { Shader = back, RenderPriority = 1 };
    }

    public void AddPositions(Vector3 start, Vector3 end)
    {
        _start.Add(start);
        _end.Add(end);
    }

    public void ClearPositions()
    {
        _start.Clear();
        _end.Clear();
    }

    public void LineWidth(float width)
    {
        _width = width;
    }

    public void LineColors(Color front, Color back)
    {
        _frontMaterial.SetShaderParameter("line_color", front);
        _backMaterial.SetShaderParameter("line_color", back);
        _selectedMaterial.SetShaderParameter("line_color", front);
    }

    public void LineSelected(bool selected)
    {
        _selected = selected;
    }

    public void Draw()
    {
        _lineMesh.ClearSurfaces();
        // ImmediateMesh.SurfaceEnd crashes when no vertices were added.
        if (_start.Count == 0) return;

        if (_selected)
        {
            EmitSurface(_selectedMaterial);
        }
        else
        {
            EmitSurface(_frontMaterial);
            EmitSurface(_backMaterial);
        }
    }

    private void EmitSurface(Material material)
    {
        _lineMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles, material);
        for (var i = 0; i < _start.Count; ++i)
        {
            var start = _start[i];
            var end = _end[i];

            var direction = (end - start).Normalized();
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
        }

        _lineMesh.SurfaceEnd();
    }
}

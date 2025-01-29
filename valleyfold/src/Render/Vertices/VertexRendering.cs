using Godot;
using valleyfold.Fold;

namespace valleyfold.Render.Vertices;

public partial class VertexRendering : Node3D
{
    private ShaderMaterial _shader;
    private PackedScene _vertexMesh;

    public override void _Ready()
    {
        _vertexMesh = ResourceLoader.Load<PackedScene>("res://scenes/vertex.tscn");

        _shader = new ShaderMaterial();
        _shader.Shader = ResourceLoader.Load<Shader>("res://src/Render/Vertices/vertex.gdshader");
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        if (GetChildCount() < frame.Vertices.Count) AddOrShowVertices(frame);
        else if (GetChildCount() > frame.Vertices.Count) HideVertices(frame);

        for (var i = 0; i < frame.Vertices.Count; i++)
        {
            var child = GetChild<MeshInstance3D>(i);
            child.Position = frame.Vertices[i].Coord;
        }
    }

    private void HideVertices(Frame frame)
    {
        for (var i = frame.Vertices.Count - 1; i < GetChildCount(); i++)
        {
            var child = GetChild<MeshInstance3D>(i);
            child.Hide();
        }
    }

    private void AddOrShowVertices(Frame frame)
    {
        for (var i = GetChildCount() - 1; i < frame.Vertices.Count; i++)
        {
            var child = GetChildOrNull<MeshInstance3D>(i);
            if (child == null)
            {
                var instance = (MeshInstance3D)_vertexMesh.Instantiate();
                AddChild(instance);
            }
            else
            {
                child.Show();
            }
        }
    }
}
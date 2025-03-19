using System.Collections.Generic;
using Godot;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

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
        
    }

    public void Render(List<Vertex3D> vertices)
    {
        if (GetChildCount() < vertices.Count) AddOrShowVertices(vertices);
        else if (GetChildCount() > vertices.Count) HideVertices(vertices);

        for (var i = 0; i < vertices.Count; i++)
        {
            var child = GetChild<MeshInstance3D>(i);
            child.Position = vertices[i].Coord;
            child.Scale = vertices[i].IsSelected ? 
                new Vector3(1.5f, 1.5f, 1.5f) : 
                new Vector3(1, 1, 1);
        }
    }

    private void HideVertices(List<Vertex3D> vertices)
    {
        for (var i = vertices.Count - 1; i < GetChildCount(); i++)
        {
            var child = GetChild<MeshInstance3D>(i);
            child.Hide();
        }
    }

    private void AddOrShowVertices(List<Vertex3D> vertices)
    {
        for (var i = GetChildCount() - 1; i < vertices.Count; i++)
        {
            var child = GetChildOrNull<MeshInstance3D>(i);
            if (child == null)
            {
                var instance = (MeshInstance3D)_vertexMesh.Instantiate();
                // instance.MaterialOverride = _shader;
                AddChild(instance);
            }
            else
            {
                child.Show();
            }
        }
    }
}
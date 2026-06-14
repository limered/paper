using System.Collections.Generic;
using Godot;
using valleyfold.ThreeDeeModels;
using valleyfold.Ui.Events;
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
        
        EventBus.Register<ResetPaperEvent>(_ => Clear());
    }
    
    private void Clear()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    public void Render(List<Vertex3D> vertices, Frame3D frame3d)
    {
        if (GetChildCount() < vertices.Count) AddOrShowVertices(vertices);
        else if (GetChildCount() > vertices.Count) HideVertices(vertices);

        for (var i = 0; i < vertices.Count; i++)
        {
            var child = GetChild<MeshInstance3D>(i);
            // Vertex nudges to the max layer of its incident faces, so shared
            // crease vertices render attached to the topmost layer.
            var nudge = Vector3.Up * (frame3d.LayerOfVertex(i) * Frame3D.LayerEpsilon);
            child.Position = vertices[i].Coord + nudge;
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
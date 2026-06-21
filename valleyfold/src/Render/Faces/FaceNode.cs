using Godot;
using Godot.Collections;

namespace valleyfold.Render.Faces;

public partial class FaceNode : Node3D
{
    [Export] public MeshInstance3D Mesh;

    public override void _Ready()
    {
        Mesh = GetNode<MeshInstance3D>("FaceBody/FaceMesh");
        Mesh.Mesh = new ArrayMesh();
    }

    public void SetMesh(Array arr)
    {
        var mesh = (ArrayMesh)Mesh.Mesh;
        mesh.ClearSurfaces();
        mesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arr);
    }
}
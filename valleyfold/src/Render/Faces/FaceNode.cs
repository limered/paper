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

    /// <summary>
    /// Clear the surface without writing a new one. Used when a face's XZ
    /// projection collapses (edge-on mid-fold) and triangulation returns
    /// no indices — leaving the prior surface up would render a stale
    /// shape against current vertex positions.
    /// </summary>
    public void SetEmpty()
    {
        var mesh = (ArrayMesh)Mesh.Mesh;
        mesh.ClearSurfaces();
    }
}
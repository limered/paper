using Godot;
using Godot.Collections;
using valleyfold.Models;

namespace valleyfold.Rendering;

public partial class ModelRenderer : MeshInstance3D
{
    private PaperModel _paperModel;
    private PackedScene _cornerHandleScene;

    public override void _Ready()
    {
        _cornerHandleScene = GD.Load<PackedScene>("res://scenes/CornerHandle.tscn");
        
        _paperModel = new PaperModel();
        _paperModel.AddFace(Statics.StartingPaper);

        RenderPaper();
    }

    private void RenderPaper()
    {
        foreach (var face in _paperModel.Faces) RenderFace(face);
    }

    private void RenderFace(PaperFace face)
    {
        var faceVerts = face.Corners;
        var faceNormals = face.Normals();
        var faceIndices = face.Triangulate();
        
        var surfaceArray = new Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        
        surfaceArray[(int)Mesh.ArrayType.Vertex] = faceVerts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = faceNormals;
        surfaceArray[(int)Mesh.ArrayType.Index] = faceIndices;

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        Mesh = mesh;
        
        // Add Handles per corner
        foreach (var corner in faceVerts)
        {
            var handle = (Node3D)_cornerHandleScene.Instantiate();
            handle.Position = corner;
            AddChild(handle);
        }
    }
}
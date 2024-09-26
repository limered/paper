using System.Collections.Generic;
using Godot;
using Godot.Collections;
using valleyfold.Models;

namespace valleyfold.Rendering;

[Tool]
public partial class ModelRenderer : MeshInstance3D
{
    private PaperModel _paperModel;

    public override void _Ready()
    {
        var startPaper = new List<Vector3>
        {
            new(1, 0, 1),
            new(1, 0, -1),
            new(-1, 0, -1),
            new(-1, 0, 1),
        };

        _paperModel = new PaperModel();
        _paperModel.AddFace(new PaperFace(startPaper, true));

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
    }
}
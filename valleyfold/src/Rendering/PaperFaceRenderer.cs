using System.Linq;
using Godot;
using Godot.Collections;
using valleyfold.Models;

namespace valleyfold.Rendering;

public class PaperFaceRenderer
{
    private readonly PackedScene _cornerHandleScene = GD.Load<PackedScene>("res://scenes/CornerHandle.tscn");

    public void Render(PaperFace face, MeshInstance3D parent)
    {
        var faceVerts = face.Corners;
        var faceNormals = face.Normals();
        var faceIndices = face.Triangulate();

        var surfaceArray = new Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        surfaceArray[(int)Mesh.ArrayType.Vertex] = faceVerts.Select(corner => corner.Position).ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = faceNormals;
        surfaceArray[(int)Mesh.ArrayType.Index] = faceIndices;

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        parent.Mesh = mesh;

        // Add Handles per corner
        foreach (var corner in faceVerts)
        {
            var handle = (CornerHandleNode)_cornerHandleScene.Instantiate();
            handle.Corner = corner;
            handle.Position = corner.Position;
            parent.AddChild(handle);
        }
    }
}
using Godot;
using Godot.Collections;

namespace valleyfold.Interaction;

public partial class PaperFaceMeshComponent : MeshInstance3D
{
    private Array _surfaceArray = new();

    public void Apply(Vector3[] vertices, Vector3[] normals, int[] indices)
    {
        _surfaceArray.Clear();
        _surfaceArray[(int)Mesh.ArrayType.Vertex] = vertices;
        _surfaceArray[(int)Mesh.ArrayType.Normal] = normals;
        _surfaceArray[(int)Mesh.ArrayType.Index] = indices;

        var mesh = (ArrayMesh)Mesh;
        mesh.ClearSurfaces();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, _surfaceArray);
        Mesh = mesh;
    }
}
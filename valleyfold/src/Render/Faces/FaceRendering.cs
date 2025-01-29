using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace valleyfold.Render.Faces;

public partial class FaceRendering : MeshInstance3D
{
    public override void _Ready()
    {
        Mesh = new ArrayMesh();
    }

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        var mesh = (ArrayMesh)Mesh;
        mesh.ClearSurfaces();

        foreach (var face in frame.Faces)
        {
            var faceVertexes = new List<Vector3>();
            faceVertexes.AddRange(face.Vertices.Select(i => frame.Vertices[i].Coord));

            var faceNormals = faceVertexes
                .Select(_ => Vector3.Up)
                .ToArray();

            int[] faceIndices;
            if (faceVertexes.Count > 3)
                faceIndices = Geometry2D
                    .TriangulatePolygon(faceVertexes.Select(c => new Vector2(c.X, c.Z))
                        .ToArray());
            else
                faceIndices = new[] { 0, 1, 2 };

            var surfaceArray = new Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            surfaceArray[(int)Mesh.ArrayType.Vertex] = faceVertexes.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Normal] = faceNormals;
            surfaceArray[(int)Mesh.ArrayType.Index] = faceIndices;

            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        }
    }
}
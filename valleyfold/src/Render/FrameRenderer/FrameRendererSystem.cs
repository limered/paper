using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using valleyfold.Fold;

namespace valleyfold.Render.FrameRenderer;

public class FrameRendererSystem
{
    public void Render(Frame frame, MeshInstance3D parent)
    {
        foreach (var face in frame.Faces)
        {
            var faceVertexes = new List<Vector3>();
            faceVertexes.AddRange(face.Vertices.Select(i => frame.Vertices[i].Coord));

            var faceNormals = faceVertexes.Select(_ => Vector3.Up).ToArray();
            var faceIndices = Geometry2D
                .TriangulatePolygon(faceVertexes.Select(c => new Vector2(c.X, c.Z))
                    .ToArray());

            var surfaceArray = new Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            surfaceArray[(int)Mesh.ArrayType.Vertex] = faceVertexes.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Normal] = faceNormals;
            surfaceArray[(int)Mesh.ArrayType.Index] = faceIndices;

            var mesh = new ArrayMesh();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
            parent.Mesh = mesh;
        }
    }
}
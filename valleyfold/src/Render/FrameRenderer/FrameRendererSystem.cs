using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Fold;
using Array = Godot.Collections.Array;

namespace valleyfold.Render.FrameRenderer;

public class FrameRendererSystem
{
    public void Render(Frame frame, ModelRenderer parent)
    {
        // RenderFaces(frame, parent);
    }

    private static void RenderFaces(Frame frame, ModelRenderer parent)
    {
        var mesh = (ArrayMesh)parent.Mesh;
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
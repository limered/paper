using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using valleyfold.Fold;
using valleyfold.Utils;

namespace valleyfold.Render.Faces;

public partial class FaceRendering : Node3D
{
    private PackedScene _faceScene = ResourceLoader.Load<PackedScene>("res://scenes/face.tscn");

    public override void _Process(double delta)
    {
        if (Statics.Frame == null) return;
        var frame = Statics.Frame;

        var faceCount = frame.Faces.Count;
        if (GetChildCount() < faceCount) AddOrShowFaces(frame);
        else if (GetChildCount() > faceCount) HideFaces(frame);

        for (var i = 0; i < faceCount; i++)
        {
            var face = frame.Faces[i];

            var faceVertexes = new List<Vector3>();
            faceVertexes.AddRange(face.Vertices.Select(v => frame.Vertices[v].Coord.Vector3XZ()));

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

            var child = GetChild<FaceNode>(i);
            child.SetMesh(surfaceArray);
        }
    }

    private void HideFaces(Frame frame)
    {
        for (var i = frame.Faces.Count - 1; i < GetChildCount(); i++)
        {
            var child = GetChild<FaceNode>(i);
            child.Hide();
        }
    }

    private void AddOrShowFaces(Frame frame)
    {
        for (var i = GetChildCount() - 1; i < frame.Faces.Count; i++)
        {
            var child = GetChildOrNull<FaceNode>(i);
            if (child == null)
            {
                var instance = (FaceNode)_faceScene.Instantiate();
                AddChild(instance);
            }
            else
            {
                child.Show();
            }
        }
    }
}
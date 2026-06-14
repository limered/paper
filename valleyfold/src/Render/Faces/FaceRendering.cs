using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;
using valleyfold.Ui.Events;
using valleyfold.Utils;

namespace valleyfold.Render.Faces;

public partial class FaceRendering : Node3D
{
    private PackedScene _faceScene = ResourceLoader.Load<PackedScene>("res://scenes/face.tscn");

    public override void _Ready()
    {
        EventBus.Register<ResetPaperEvent>(_ => Clear());
    }

    private void Clear()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    public void RenderFaces(List<Face> faces, List<Vector3> vertices, Frame3D frame3d)
    {
        var faceCount = faces.Count;
        if (GetChildCount() < faceCount) AddOrShowFaces(faces);
        else if (GetChildCount() > faceCount) HideFaces(faces);

        for (var i = 0; i < faceCount; i++)
        {
            var face = faces[i];

            var nudge = Vector3.Up * (frame3d.LayerOf(face) * Frame3D.LayerEpsilon);
            var faceVertexes = new List<Vector3>();
            faceVertexes.AddRange(face.Vertices.Select(v => vertices[v] + nudge));

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

    private void HideFaces(List<Face> faces)
    {
        for (var i = faces.Count - 1; i < GetChildCount(); i++)
        {
            var child = GetChild<FaceNode>(i);
            child.Hide();
        }
    }

    private void AddOrShowFaces(List<Face> faces)
    {
        for (var i = GetChildCount() - 1; i < faces.Count; i++)
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
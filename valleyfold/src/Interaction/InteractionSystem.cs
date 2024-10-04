using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Models;
using valleyfold.Rendering;

namespace valleyfold.Interaction;

public class InteractionSystem
{
    private readonly PackedScene _cornerHandleScene = GD.Load<PackedScene>("res://scenes/corner_handle.tscn");
    private readonly List<Node3D> _faceNodes;
    private readonly List<PaperFace> _faces;
    private readonly PackedScene _faceScene = GD.Load<PackedScene>("res://scenes/paper_face.tscn");
    private readonly List<List<CornerHandleNode>> _handles;

    public InteractionSystem(List<PaperFace> faces)
    {
        _faces = faces;
        _faceNodes = new List<Node3D>(faces.Count);
        _handles = new List<List<CornerHandleNode>>(faces.Count);
    }


    public Node ModelParent { get; set; }

    public void Process()
    {
        for (var i = 0; i < _faces.Count; i++) CreateFaceMesh(_faces[i], i);
    }

    private void CreateFaceMesh(PaperFace face, int i)
    {
        var meshNode = _faceNodes.ElementAtOrDefault(i);
        var handles = _handles.ElementAtOrDefault(i);
        if (meshNode == null)
        {
            meshNode = _faceScene.Instantiate<Node3D>();
            _faceNodes.Add(meshNode);
            ModelParent?.AddChild(meshNode);

            handles = new List<CornerHandleNode>();
            _handles.Add(handles);

            for (var f = 0; f < face.Corners.Count; f++)
            {
                var corner = face.Corners[f];
                var handle = _cornerHandleScene.Instantiate<CornerHandleNode>();
                handles.Add(handle);
                handle.Position = corner.Position;
                meshNode.AddChild(handle);
            }
        }

        var mesh = _faceNodes[i].GetNode<PaperFaceMeshComponent>("Mesh");
        mesh.Apply(
            face.Corners.Select(c => c.Position).ToArray(),
            face.Normals(),
            face.Triangulate());
    }
}
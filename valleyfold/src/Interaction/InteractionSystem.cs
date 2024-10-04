using System.Collections.Generic;
using System.Linq;
using Godot;
using valleyfold.Models;

namespace valleyfold.Interaction;

public class InteractionSystem
{
    private readonly List<Node3D> _faceNodes;
    private readonly List<PaperFace> _faces;
    private readonly PackedScene _faceScene = GD.Load<PackedScene>("res://scenes/paper_face.tscn");

    public Node ModelParent { get; set; }
    
    public InteractionSystem(List<PaperFace> faces)
    {
        _faces = faces;
        _faceNodes = new List<Node3D>(faces.Count);
    }

    public void Process()
    {
        for (var i = 0; i < _faces.Count; i++)
        {
            CreateFaceMesh(_faces[i], i);
        }
    }

    private void CreateFaceMesh(PaperFace face, int i)
    {
        var meshNode = _faceNodes.ElementAtOrDefault(i);
        if (meshNode == null)
        {
            meshNode = _faceScene.Instantiate<Node3D>();
            _faceNodes.Add(meshNode);
            ModelParent?.AddChild(meshNode);
        }

        var mesh = _faceNodes[i].GetNode<PaperFaceMeshComponent>("Mesh");
        mesh.Apply(
            face.Corners.Select(c => c.Position).ToArray(),
            face.Normals(),
            face.Triangulate());
    }
}
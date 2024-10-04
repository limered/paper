using System.Collections.Generic;
using Godot;
using valleyfold.Models;

namespace valleyfold.Interaction;


public class InteractionSystem
{
    private PackedScene _faceScene = GD.Load<PackedScene>("res://scenes/paper_face.tscn");
    private readonly List<PaperFace> _faces;
    private readonly List<Node3D> _faceNodes;

    public InteractionSystem(List<PaperFace> faces)
    {
        _faces = faces;
        _faceNodes = new List<Node3D>(faces.Count);
    }

    public void Process()
    {
        for (var i = 0; i < _faces.Count; i++)
        {
            RenderFace(_faces[i], i);
        }
    }

    private void RenderFace(PaperFace face, int i)
    {
        var meshNode = _faceNodes[i];
        if (meshNode == null)
        {
            meshNode = _faceScene.Instantiate<Node3D>();
            _faceNodes[i] = meshNode;
        }
        
        var mesh = _faceNodes[i].GetNode<PaperFaceMeshComponent>("Mesh");
        
    }
}
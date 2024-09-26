using System.Collections.Generic;
using Godot;
using valleyfold.Models;

namespace valleyfold.Rendering;

public partial class ModelRenderer : Node2D
{
    private PaperModel _paperModel;

    public override void _Ready()
    {
        var startPaper = new List<Vector3>
        {
            new(0, 0, 0),
            new(1, 0, 0),
            new(0, 1, 0),
            new(1, 1, 0)
        };

        _paperModel = new PaperModel();
        _paperModel.AddFace(new PaperFace(startPaper, true));
    }
}
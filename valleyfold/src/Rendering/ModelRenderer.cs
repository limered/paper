using System.Linq;
using Godot;
using Godot.Collections;
using valleyfold.Models;

namespace valleyfold.Rendering;

public partial class ModelRenderer : MeshInstance3D
{
    private PaperModel _paperModel;
    private PaperFaceRenderer _paperFaceRenderer;    

    public override void _Ready()
    {
        _paperFaceRenderer = new PaperFaceRenderer();
        
        _paperModel = new PaperModel();
        _paperModel.AddFace(Statics.StartingPaper);
        
        RenderPaper();
    }

    private void RenderPaper()
    {
        foreach (var face in _paperModel.Faces) _paperFaceRenderer.Render(face, this);
    }
}
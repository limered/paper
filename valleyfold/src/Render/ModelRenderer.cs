using Godot;
using valleyfold.Fold;
using valleyfold.Render.FrameRenderer;

namespace valleyfold.Render;

public partial class ModelRenderer : MeshInstance3D
{
    private Frame _frame;
    private readonly FrameRendererSystem _frameRendererSystem = new();

    public override void _Ready()
    {
        _frame = new Frame();
        _frame.InitializePaper();
    }

    public override void _Process(double delta)
    {
        _frameRendererSystem.Render(_frame, this);
    }
}
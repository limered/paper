using Godot;
using valleyfold.Fold;
using valleyfold.Render.FrameRenderer;

namespace valleyfold.Render;

public partial class ModelRenderer : MeshInstance3D
{
    private Frame _frame;
    private readonly FrameRendererSystem _frameRendererSystem = new();

    [Export]
    public MeshInstance3D LineRenderer;

    public override void _Ready()
    {
        LineRenderer.Mesh = new ImmediateMesh();
        
        _frame = new Frame();
        _frame.InitializePaper();
    }

    public override void _Process(double delta)
    {
        _frameRendererSystem.Render(_frame, this);
    }
}
using Godot;
using valleyfold.Fold;

namespace valleyfold.Render;

public partial class ModelRenderer : Node3D
{
    public Frame Frame;

    public override void _Ready()
    {
        Frame = new Frame();
        Frame.InitializePaper();
    }

    public override void _Process(double delta)
    {
        
    }
}
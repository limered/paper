using System.Collections.Generic;
using Godot;
using valleyfold.Fold;
using valleyfold.Folding;

namespace valleyfold.Render;

public partial class ModelRenderer : MeshInstance3D
{
    private Frame _frame;

    public override void _Ready()
    {
        Mesh = new ArrayMesh();

        _frame = new Frame();
        _frame.InitializePaper();
        Statics.Frame = _frame;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
            if (eventKey.IsPressed() && eventKey.Keycode == Key.G)
                new Valleyfold
                {
                    Vertices = new List<Id> { 0, 2 }
                }.Apply(_frame);
    }
}
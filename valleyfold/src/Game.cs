using Godot;
using valleyfold.Fold;

namespace valleyfold;

public partial class Game : Node
{
    public override void _Ready()
    {
        Statics.Frame = new Frame();
        Statics.Frame.InitializePaper();
    }

    public override void _Process(double delta)
    {
    }
}
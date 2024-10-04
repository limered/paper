using Godot;
using valleyfold.Interaction;
using valleyfold.Models;

namespace valleyfold;

public partial class Game : Node
{
    private PaperModel _paperModel;
    private InteractionSystem _interactionSystem;

    public override void _Ready()
    {
        _paperModel = new PaperModel();
        _paperModel.AddFace(Statics.StartingPaper);

        _interactionSystem = new InteractionSystem(_paperModel.Faces)
        {
            ModelParent = this
        };
    }

    public override void _Process(double delta)
    {
        _interactionSystem.Process();
    }
}
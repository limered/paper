using Godot;

namespace valleyfold.Ui;

public partial class GameInterface : Control
{
    public override void _Ready()
    {
        var animateButton = GetNode<Button>("Sidepane/animate");
        animateButton.Pressed += AnimateButtonOnPressed;
    }

    private void AnimateButtonOnPressed()
    {
        GD.Print("animate");
    }
}
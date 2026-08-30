using Godot;

namespace valleyfold.Ui;

public partial class DioramaScreen : Control
{
    public override void _Ready()
    {
        GetNode<Button>("BackButton").Pressed +=
            () => SceneTransition.To("res://scenes/main.tscn", SceneTransition.TransitionDirection.Left);
    }
}

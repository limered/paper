using Godot;

namespace valleyfold.Ui;

public partial class DioramaScreen : Control
{
    public override void _Ready()
    {
        GetNode<Button>("BackButton").Pressed +=
            () => GetTree().ChangeSceneToFile("res://scenes/main.tscn");
    }
}

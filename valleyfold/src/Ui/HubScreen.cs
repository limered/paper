using Godot;

namespace valleyfold.Ui;

public partial class HubScreen : Control
{
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/DeskButton").Pressed +=
            () => SceneTransition.To("res://scenes/desk.tscn", SceneTransition.TransitionDirection.Up);
        GetNode<Button>("VBoxContainer/DioramaButton").Pressed +=
            () => SceneTransition.To("res://scenes/diorama.tscn", SceneTransition.TransitionDirection.Right);
        GetNode<Button>("VBoxContainer/SettingsButton").Pressed +=
            () => GD.Print("[Hub] Settings not implemented yet.");
    }
}

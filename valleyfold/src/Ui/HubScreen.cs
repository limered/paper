using Godot;

namespace valleyfold.Ui;

public partial class HubScreen : Control
{
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/DeskButton").Pressed +=
            () => GetTree().ChangeSceneToFile("res://scenes/desk.tscn");
        GetNode<Button>("VBoxContainer/DioramaButton").Pressed +=
            () => GetTree().ChangeSceneToFile("res://scenes/diorama.tscn");
        GetNode<Button>("VBoxContainer/SettingsButton").Pressed +=
            () => GD.Print("[Hub] Settings not implemented yet.");
    }
}

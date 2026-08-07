using Godot;

namespace valleyfold.Ui;

public partial class DeskScreen : Control
{
    private readonly DeskSelection _selection = new();

    public override void _Ready()
    {
        var drawer = GetNode<Control>("Drawer");
        var openDrawer = GetNode<Button>("OpenDrawer");
        var closeDrawer = GetNode<Button>("Drawer/VBoxContainer/CloseButton");
        var beginButton = GetNode<Button>("Drawer/VBoxContainer/BeginButton");
        var backButton = GetNode<Button>("BackButton");
        var defaultPaper = GetNode<Button>("Drawer/Papers/DefaultPaper");
        var lockedPaper = GetNode<Button>("Drawer/Papers/LockedPaper");

        openDrawer.Pressed += () => drawer.Visible = true;
        closeDrawer.Pressed += () => drawer.Visible = false;
        backButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/main.tscn");

        defaultPaper.Pressed += () =>
        {
            _selection.SelectPaper(DeskSelection.DefaultPaperId);
            UpdateSelectionVisuals();
        };

        lockedPaper.Disabled = true;
        lockedPaper.Modulate = new Color(0.5f, 0.5f, 0.5f);

        beginButton.Pressed += () =>
        {
            if (!_selection.CanBegin) return;
            drawer.Visible = false;
            _selection.Begin();
        };

        // Boat is the only template in M0.
        _selection.SelectTemplate(DeskSelection.BoatTemplateId);
        UpdateSelectionVisuals();
    }

    private void UpdateSelectionVisuals()
    {
        var defaultPaper = GetNode<Button>("Drawer/Papers/DefaultPaper");
        defaultPaper.Modulate = _selection.SelectedPaperId == DeskSelection.DefaultPaperId
            ? new Color(1, 1, 1)
            : new Color(0.7f, 0.7f, 0.7f);
    }
}

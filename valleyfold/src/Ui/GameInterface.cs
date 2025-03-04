using Godot;
using valleyfold.Utils;

namespace valleyfold.Ui;

public partial class GameInterface : Control
{
    private Button _animateButton;

    public override void _Ready()
    {
        _animateButton = GetNode<Button>("Sidepane/animate");
        _animateButton.Pressed += AnimateButtonOnPressed;

        var valleyfoldButton = GetNode<Button>("Sidepane/valleyfold");
        valleyfoldButton.Pressed += ValleyfoldButtonOnPressed;

        var unspecifiedButton = GetNode<Button>("Sidepane/unspecifiedfold");
        unspecifiedButton.Pressed += UnspecifiedButtonOnPressed;
    }

    private void UnspecifiedButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange() { NextFoldMode = FoldMode.Unspecified });
    }

    private void ValleyfoldButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange() { NextFoldMode = FoldMode.Valley });
    }

    private void AnimateButtonOnPressed()
    {
        EventBus.Emit(new AnimationModeChange());
    }
}
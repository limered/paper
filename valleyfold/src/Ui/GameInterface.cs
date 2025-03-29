using Godot;
using valleyfold.TwoDeeModels;
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
        EventBus.Emit(new FoldModeChange() { NextFoldMode = Assignment.F });
    }

    private void ValleyfoldButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange() { NextFoldMode = Assignment.V });
    }

    private void AnimateButtonOnPressed()
    {
        EventBus.Emit(new AnimationModeChange());
    }
}
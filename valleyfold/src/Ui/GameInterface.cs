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

        var unspecifiedButton = GetNode<Button>("Sidepane/unfold");
        unspecifiedButton.Pressed += UnspecifiedButtonOnPressed;
    }

    private static void UnspecifiedButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange { NextFoldMode = Assignment.F });
    }

    private static void ValleyfoldButtonOnPressed()
    {
        EventBus.Emit(new FoldModeChange { NextFoldMode = Assignment.V });
    }

    private static void AnimateButtonOnPressed()
    {
        EventBus.Emit(new AnimationModeChange());
    }
}
using System;
using Godot;
using valleyfold.TwoDeeModels;
using valleyfold.Utils;

namespace valleyfold.Ui;

public partial class GameInterface : Control
{
    private Button _animateButton;
    private Label _activeFoldLabel;

    public override void _Ready()
    {
        _animateButton = GetNode<Button>("Sidepane/animate");
        _animateButton.Pressed += AnimateButtonOnPressed;

        var valleyfoldButton = GetNode<Button>("Sidepane/valleyfold");
        valleyfoldButton.Pressed += ValleyfoldButtonOnPressed;

        var unspecifiedButton = GetNode<Button>("Sidepane/unfold");
        unspecifiedButton.Pressed += UnspecifiedButtonOnPressed;

        _activeFoldLabel = GetNode<Label>("Sidepane/active_fold_mode");
        _activeFoldLabel.Text = "Valleyfold";
        EventBus.Register<FoldModeChange>(MapFoldModeToText);
    }

    private void MapFoldModeToText(FoldModeChange msg)
    {
        switch (msg.NextFoldMode)
        {
            case Assignment.M:
                _activeFoldLabel.Text = "Mountainfold";
                break;
            case Assignment.V:
                _activeFoldLabel.Text = "Valleyfold";
                break;
            case Assignment.F:
                _activeFoldLabel.Text = "Unfold";
                break;
            default:
                _activeFoldLabel.Text = "Unspecified";
                break;
        }
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